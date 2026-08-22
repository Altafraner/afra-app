using System.Diagnostics;
using System.Text;
using Altafraner.AfraApp.Profundum.Configuration;
using Altafraner.AfraApp.Profundum.Domain.Contracts.Rules;
using Altafraner.AfraApp.Profundum.Domain.Contracts.Services;
using Altafraner.AfraApp.Profundum.Domain.DTO;
using Altafraner.AfraApp.Profundum.Domain.Models;
using Altafraner.AfraApp.User.Domain.DTO;
using Altafraner.AfraApp.User.Domain.Models;
using Altafraner.AfraApp.User.Services;
using Google.OrTools.Sat;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Person = Altafraner.AfraApp.User.Domain.Models.Person;

namespace Altafraner.AfraApp.Profundum.Services;

internal class ProfundumMatchingService
{
    // Should be greater than 0 to avoid random solutions when no wishes exist or are feasible
    private const int NotEnrolledValue = 3;

    private readonly AfraAppContext _dbContext;
    private readonly ILogger _logger;
    private readonly ProfundumConfiguration _profundumConfiguration;
    private readonly IRulesFactory _rulesFactory;
    private readonly UserService _userService;

    public ProfundumMatchingService(AfraAppContext dbContext,
        ILogger<ProfundumEnrollmentService> logger,
        IOptions<ProfundumConfiguration> profundumConfiguration,
        IRulesFactory rulesFactory,
        UserService userService)
    {
        _dbContext = dbContext;
        _logger = logger;
        _profundumConfiguration = profundumConfiguration.Value;
        _rulesFactory = rulesFactory;
        _userService = userService;
    }


    /// <summary>
    ///     Perform a matching for the given slots and return information about the result
    /// </summary>
    public async Task<MatchingStats> PerformMatching()
    {
        var stopwatch = Stopwatch.StartNew();

        var currentZeitraum = await GetCurrentEinwahlZeitraumAsync()
            ?? throw new ArgumentException("Kein Einwahlzeitraum vorhanden.");

        await _dbContext.ProfundaEinschreibungen
            .Where(e => !e.IsFixed)
            .ExecuteDeleteAsync();

        var slots = _dbContext.ProfundaSlots.Include(s => s.EinwahlZeitraum)
            .Where(s => s.EinwahlZeitraum == currentZeitraum).ToArray();
        var fixEinschreibungen = _dbContext.ProfundaEinschreibungen
            .Where(e => e.IsFixed).ToArray();
        var angebote = (await _dbContext.ProfundaInstanzen
                .Include(pi => pi.Slots).ThenInclude(s => s.EinwahlZeitraum)
                .Include(pi => pi.Profundum).ThenInclude(p => p.Kategorie)
                .ToArrayAsync())
            .ToArray();
        var belegwuensche = await _dbContext.ProfundaBelegWuensche
            .Include(b => b.BetroffenePerson)
            .Include(b => b.ProfundumDefinition).ThenInclude(p => p.Kategorie)
            .Include(b => b.EinwahlZeitraum).ThenInclude(z => z.Slots)
            .Where(e => e.EinwahlZeitraum == currentZeitraum)
            .ToArrayAsync();
        var students = _dbContext.Personen
            .Where(p => p.Rolle == Rolle.Mittelstufe)
            .Where(p => !p.Deleted)
            .Where(p => p.CreatedAt <= currentZeitraum.EinwahlStop)
            .ToArray();
        var wuenscheSlotAware = ProcessWuenscheSlotAware(belegwuensche);

        if (!_profundumConfiguration.DeterministicMatching)
        {
            Random.Shared.Shuffle(angebote);
            Random.Shared.Shuffle(belegwuensche);
            Random.Shared.Shuffle(students);
        }

        var model = new CpModel();
        var objective = LinearExpr.NewBuilder();

        var belegVars = new Dictionary<(Person p, ProfundumSlot s, ProfundumInstanz i), BoolVar>();
        var personNotEnrolledVariables = new Dictionary<(Person p, ProfundumSlot s), BoolVar>();

        var timeDbAndPrep = stopwatch.ElapsedMilliseconds;
        stopwatch.Restart();

        foreach (var currentSlot in slots)
        {
            var angeboteInSlot = angebote.Where(a => a.Slots.Contains(currentSlot)).ToArray();
            foreach (var currentStudent in students)
            {
                AddStudentVarsWithObjectives(currentStudent, currentSlot, angeboteInSlot);
            }
        }

        AddIndividualRules();
        AddAggregateRules();

        var timeConstraintsAdded = stopwatch.ElapsedMilliseconds;
        stopwatch.Restart();

        model.Maximize(objective);
        _logger.LogInformation("Model stats: {stats}", model.ModelStats());

        using var solver = new CpSolver();
        solver.StringParameters = "max_time_in_seconds:240.0";

        var timeSolverPrep = stopwatch.ElapsedMilliseconds;
        stopwatch.Restart();

        var resultStatus = solver.Solve(model, new SolutionCallBack(_logger));

        var timeSolver = stopwatch.ElapsedMilliseconds;
        stopwatch.Restart();

        if (resultStatus != CpSolverStatus.Optimal && resultStatus != CpSolverStatus.Feasible)
        {
            throw new ArgumentException("No solution found in Matching.");
        }

        var newEinschreibungen = ExtractEnrollmentsFromResults();
        await _dbContext.ProfundaEinschreibungen.AddRangeAsync(newEinschreibungen);
        await _dbContext.SaveChangesAsync();

        var rangVerteilung = CalculateRangDistributionFromEnrollments(newEinschreibungen, wuenscheSlotAware);

        var studentsWithWishes = belegwuensche.Select(w => w.BetroffenePerson).Distinct().ToHashSet();
        var nichtEingeschriebenTrotzWunsch = personNotEnrolledVariables
            .Where(kv => studentsWithWishes.Contains(kv.Key.p)
                    && !fixEinschreibungen.Any(e => e.BetroffenePerson == kv.Key.p && e.Slot == kv.Key.s)
                    && solver.Value(kv.Value) > 0)
            .Select(kv => kv.Key.p)
            .Distinct()
            .Count();

        var timeAfter = stopwatch.ElapsedMilliseconds;
        stopwatch.Stop();

        _logger.LogInformation("""
                           Solver timing:
                             DB and prep: {dbAndPrep} ms
                             Constraints: {constraints} ms
                             Solver prep: {solverPrep} ms
                             Solver     : {solver} ms
                             Memorandum : {after} ms
                           """,
            timeDbAndPrep,
            timeConstraintsAdded,
            timeSolverPrep,
            timeSolver,
            timeAfter);

        return new MatchingStats
        {
            CalculationTime = solver.WallTime(),
            Result = MatchingResultStatus.MatchingComplete,
            NichtEingeschriebenTrotzWunsch = nichtEingeschriebenTrotzWunsch,
            RangVerteilung = rangVerteilung
        };

        List<ProfundumEinschreibung> ExtractEnrollmentsFromResults()
        {
            var profundumEinschreibungs = new List<ProfundumEinschreibung>();
            foreach (var p in students)
            foreach (var i in angebote)
            foreach (var s in i.Slots.Where(slots.Contains))
            {
                if (fixEinschreibungen.Any(e => e.BetroffenePerson == p && e.Slot == s)) continue;

                if (solver.Value(belegVars[(p, s, i)]) > 0)
                    profundumEinschreibungs.Add(new ProfundumEinschreibung
                    {
                        ProfundumInstanz = i,
                        BetroffenePerson = p,
                        Slot = s
                    });
            }

            return profundumEinschreibungs;
        }

        void AddAggregateRules()
        {
            foreach (var r in _rulesFactory.GetAggregateRules())
                r.AddConstraints(slots, students, belegVars, model, objective);
        }

        void AddIndividualRules()
        {
            foreach (var student in students)
            {
                var sEnrollments = fixEinschreibungen
                    .Where(e => e.BetroffenePerson == student && e.ProfundumInstanz is not null)
                    .ToArray();
                var sBelegVars = belegVars.Where(k => k.Key.p == student)
                    .ToDictionary(x => (x.Key.s, x.Key.i), x => x.Value);
                var klasse = _userService.GetKlassenstufe(student);

                foreach (var r in _rulesFactory.GetIndividualRules())
                    r.AddConstraints(student,
                        klasse,
                        slots,
                        sEnrollments,
                        sBelegVars,
                        model,
                        objective
                    );
            }
        }

        void AddStudentVarsWithObjectives(Person currentStudent,
            ProfundumSlot currentSlot,
            ProfundumInstanz[] angeboteInSlot)
        {
            List<BoolVar> personsVariablesInSlot = [];
            var fixedEnrollment = fixEinschreibungen
                .SingleOrDefault(e => e.BetroffenePerson == currentStudent
                                      && e.Slot == currentSlot);

            var notEnrolledVar = model.NewBoolVar($"beleg-{currentStudent.Id}-not-enrolled-in-{currentSlot.Id}");
            personNotEnrolledVariables[(currentStudent, currentSlot)] = notEnrolledVar;
            personsVariablesInSlot.Add(notEnrolledVar);
            objective.AddTerm(notEnrolledVar,
                NotEnrolledValue); // Not matched is slightly better than stupid solutions.
            if (fixedEnrollment is not null && fixedEnrollment.ProfundumInstanz is null) model.Add(notEnrolledVar == 1);

            foreach (var currentInstanzInSlot in angeboteInSlot)
            {
                var currentVar =
                    model.NewBoolVar($"beleg-{currentStudent.Id}-{currentSlot.Id}-{currentInstanzInSlot.Id}");
                belegVars[(currentStudent, currentSlot, currentInstanzInSlot)] = currentVar;
                personsVariablesInSlot.Add(currentVar);

                if (fixedEnrollment?.ProfundumInstanz == currentInstanzInSlot) model.Add(currentVar == 1);

                if (wuenscheSlotAware.TryGetValue((currentStudent, currentSlot, currentInstanzInSlot),
                        out var wunschRang))
                    objective.AddTerm(currentVar, WunschReward(wunschRang, _profundumConfiguration));
            }

            model.AddExactlyOne(personsVariablesInSlot);
        }
    }

    private static Dictionary<int, int> CalculateRangDistributionFromEnrollments(
        List<ProfundumEinschreibung> newEinschreibungen,
        Dictionary<(Person student, ProfundumSlot slot, ProfundumInstanz instanz), int> wuensche)
    {
        var rangVerteilung = new Dictionary<int, int>();
        foreach (var e in newEinschreibungen)
            if (e.ProfundumInstanz is not null &&
                wuensche.TryGetValue((e.BetroffenePerson, e.Slot, e.ProfundumInstanz), out var rang))
                rangVerteilung[rang] = rangVerteilung.GetValueOrDefault(rang) + 1;

        return rangVerteilung;
    }

    private Dictionary<(Person student, ProfundumSlot slot, ProfundumInstanz instanz), int> ProcessWuenscheSlotAware(
        ProfundumBelegWunsch[] belegwuensche)
    {
        var result = new Dictionary<(Person student, ProfundumSlot slot, ProfundumInstanz instanz), int>();
        var wuenschePerStudent = belegwuensche.GroupBy(e => e.BetroffenePerson);
        foreach (var group in wuenschePerStudent)
        {
            var student = group.Key;
            var studentsWuensche = group.AsEnumerable();
            var wuenscheWithSlots = studentsWuensche.SelectMany(e =>
                e.ProfundumDefinition.Instanzen.Where(i => i.Slots.Any(e.EinwahlZeitraum.Slots.Contains))
                    .SelectMany(i => i.Slots.Select(s => (e, s, i))));
            var wuenscheBySlotsAndStudent = wuenscheWithSlots.GroupBy(e => e.s);
            foreach (var wuensche in wuenscheBySlotsAndStudent)
            {
                var ordered = wuensche.Distinct().OrderBy(w => w.e.Rang).ToArray();
                for (var rang = 0; rang < ordered.Length; rang++)
                {
                    var current = ordered[rang];
                    result[(student, current.s, current.i)] = rang + 1;
                }
            }
        }

        return result;
    }

    private static
        Dictionary<(Person student, ProfundumSlot slot, ProfundumInstanz instanz), (int processed, int unprocessed)>
        ProcessWuenscheSlotAwareForStudent(IEnumerable<ProfundumBelegWunsch> studentsWuensche,
            Person student)
    {
        Dictionary<(Person student, ProfundumSlot slot, ProfundumInstanz instanz), (int processed, int unprocessed)>
            result = [];
        var wuenscheWithSlots = studentsWuensche.SelectMany(e =>
            e.ProfundumDefinition.Instanzen.Where(i => i.Slots.Any(e.EinwahlZeitraum.Slots.Contains))
                .SelectMany(i => i.Slots.Select(s => (e, s, i))));
        var wuenscheBySlotsAndStudent = wuenscheWithSlots.GroupBy(e => e.s);
        foreach (var wuensche in wuenscheBySlotsAndStudent)
        {
            var ordered = wuensche.Distinct().OrderBy(w => w.e.Rang).ToArray();
            for (var rang = 0; rang < ordered.Length; rang++)
            {
                var current = ordered[rang];
                result[(student, current.s, current.i)] = (rang + 1, current.e.Rang);
            }
        }

        return result;
    }

    /// <summary>
    ///     The reward for satisfying a wish of the given rank. Convex-decreasing (quadratic cost subtracted from a
    ///     base reward, floored) so that spreading bad outcomes across many students is preferred over concentrating
    ///     them on a few, while every ranked wish - however low - still beats leaving a student unenrolled.
    /// </summary>
    private static int WunschReward(int rang, ProfundumConfiguration cfg)
    {
        var reward = cfg.WunschBasisWert - cfg.WunschKostenFaktor * rang * rang;
        return Math.Max(reward, cfg.WunschMindestWert);
    }

    /// <summary>Flips every matched (non-null) enrollment to <c>IsFixed = true</c>, locking it in for future runs.</summary>
    public Task FinalizeMatching()
    {
        return _dbContext.ProfundaEinschreibungen
            .Where(e => e.ProfundumInstanz != null)
            .ExecuteUpdateAsync(e => e.SetProperty(ei => ei.IsFixed, true));
    }

    /// <summary>Collects every individual rule's <see cref="IProfundumIndividualRule.GetWarnings" /> for a student.</summary>
    private IEnumerable<MatchingWarning> GetStudentWarnings(Person student,
        ProfundumSlot[] slots,
        ProfundumEinschreibung[] enrollments)
    {
        var klasse = _userService.GetKlassenstufe(student);
        return _rulesFactory.GetIndividualRules().SelectMany(r => r.GetWarnings(student, klasse, slots, enrollments));
    }

    /// <summary>
    ///     The Einwahlzeitraum matching/the staff overview currently operate on: always the last one by
    ///     <see cref="ProfundumEinwahlZeitraum.EinwahlStart" />, regardless of whether it's already open, not yet
    ///     open, or already past its <see cref="ProfundumEinwahlZeitraum.EinwahlStop" /> - unlike student-facing
    ///     submission, which needs "is the window open right now," staff may need to run/review a match for a round
    ///     that hasn't opened for students yet, so this deliberately doesn't filter by "now" at all.
    /// </summary>
    private Task<ProfundumEinwahlZeitraum?> GetCurrentEinwahlZeitraumAsync()
    {
        return _dbContext.ProfundumEinwahlZeitraeume
            .OrderByDescending(z => z.EinwahlStart)
            .FirstOrDefaultAsync();
    }

    /// <summary>
    ///     Staff-facing view of every Mittelstufe student's current enrollments, wishes, rule warnings, and
    ///     team-partner pairing sync status - independent of whether a matching run just happened, e.g. also used to
    ///     inspect the effect of a manual override.
    /// </summary>
    public async IAsyncEnumerable<DTOProfundumEnrollmentSet> GetAllEnrollmentsAsync()
    {
        var currentZeitraum = await GetCurrentEinwahlZeitraumAsync();

        var slots = await _dbContext.ProfundaSlots.Include(s => s.EinwahlZeitraum).ToArrayAsync();
        var currentSlots = currentZeitraum is null
            ? []
            : slots.Where(s => s.EinwahlZeitraum == currentZeitraum).ToArray();

        var pairings = await _dbContext.ProfundumPartnerWuensche
            .Include(w => w.ProfundumDefinition)
            .Include(w => w.PersonA)
            .Include(w => w.PersonB)
            .ToArrayAsync();
        var pairingsByPerson = pairings
            .SelectMany(p => new[] { (personId: p.PersonAId, pairing: p), (personId: p.PersonBId, pairing: p) })
            .GroupBy(x => x.personId)
            .ToDictionary(g => g.Key, g => g.Select(x => x.pairing).ToArray());
        var instanzenByPersonAndDefinition = (await _dbContext.ProfundaEinschreibungen
                .Where(e => e.ProfundumInstanz != null)
                .Select(e => new { e.BetroffenePersonId, DefinitionId = e.ProfundumInstanz!.Profundum.Id, e.ProfundumInstanzId })
                .ToArrayAsync())
            .GroupBy(e => (e.BetroffenePersonId, e.DefinitionId))
            .ToDictionary(g => g.Key, g => g.Select(x => x.ProfundumInstanzId).ToHashSet());

        var zusatzInfoByPerson = currentZeitraum is null
            ? new Dictionary<Guid, string>()
            : await _dbContext.ProfundumZusatzInformationen
                .Where(z => z.EinwahlZeitraum == currentZeitraum)
                .ToDictionaryAsync(z => z.BetroffenePersonId, z => z.Information);

        var personenWithData = _dbContext.Personen
            .AsSplitQuery()
            .Where(p => p.Rolle == Rolle.Mittelstufe)
            .Where(p => !p.Deleted)
            .OrderBy(p => p.Gruppe)
            .ThenBy(p => p.LastName)
            .ThenBy(p => p.FirstName)
            .Include(p => p.ProfundaBelegwuensche)
            .ThenInclude(p => p.ProfundumDefinition)
            .ThenInclude(p => p.Instanzen)
            .ThenInclude(i => i.Slots)
            .Include(p => p.ProfundaBelegwuensche)
            .ThenInclude(p => p.EinwahlZeitraum)
            .Include(p => p.ProfundaEinschreibungen)
            .ThenInclude(p => p.ProfundumInstanz)
            .ThenInclude(p => p!.Profundum)
            .ThenInclude(p => p.Kategorie)
            .Include(p => p.ProfundaEinschreibungen)
            .ThenInclude(p => p.ProfundumInstanz)
            .ThenInclude(p => p!.Profundum)
            .ThenInclude(p => p.Dependencies)
            .Include(p => p.ProfundaEinschreibungen)
            .ThenInclude(p => p.ProfundumInstanz)
            .ThenInclude(p => p!.Slots)
            .AsAsyncEnumerable()
            .OrderBy(x => int.Parse((x.Gruppe ?? "0").TakeWhile(char.IsDigit).ToArray()))
            .ThenBy(x =>
                (x.Gruppe ?? "").SkipWhile(c => !char.IsDigit(c))
                .Aggregate(new StringBuilder(), (a, b) => a.Append(b))
                .ToString());


        await foreach (var person in personenWithData)
        {
            var personsEnrollments = slots
                .Where(slot => person.CreatedAt <= slot.EinwahlZeitraum.EinwahlStop
                    || person.ProfundaEinschreibungen.Any(e => e.Slot == slot))
                .Select(slot => (slotId: slot.Id,
                    enrollment: person.ProfundaEinschreibungen.FirstOrDefault(e => e.Slot == slot)))
                .Select(e =>
                    e.enrollment is not null
                        ? new DTOProfundumEnrollment(e.enrollment)
                        : new DTOProfundumEnrollment
                        { ProfundumSlotId = e.slotId, ProfundumInstanzId = null, IsFixed = false });

            var personsWishes = person.ProfundaBelegwuensche
                .Where(e => currentZeitraum is not null && e.EinwahlZeitraum == currentZeitraum)
                .ToArray();
            var transforms = ProcessWuenscheSlotAwareForStudent(personsWishes, person);
            var niceWishes = new Dictionary<string, List<DtoProfundumWunsch>>();
            foreach (var w in transforms)
            {
                if (!niceWishes.TryGetValue(w.Key.slot.ToString(), out var slotWishes)) slotWishes = [];

                slotWishes = slotWishes
                    .Prepend(new DtoProfundumWunsch(w.Key.instanz.Profundum.Id, w.Value.processed, w.Value.unprocessed))
                    .OrderBy(e => e.Rang)
                    .ToList();
                niceWishes[w.Key.slot.ToString()] = slotWishes;
            }

            var warnings = GetStudentWarnings(person,
                currentSlots,
                person.ProfundaEinschreibungen
                    .Where(e => e.ProfundumInstanz is not null)
                    .ToArray()).ToList();

            var myPairings = pairingsByPerson.GetValueOrDefault(person.Id, []);
            foreach (var pairing in myPairings)
            {
                var partnerId = pairing.PersonAId == person.Id ? pairing.PersonBId : pairing.PersonAId;
                var myInstanzen = instanzenByPersonAndDefinition.GetValueOrDefault((person.Id, pairing.ProfundumDefinitionId), []);
                var partnerInstanzen = instanzenByPersonAndDefinition.GetValueOrDefault((partnerId, pairing.ProfundumDefinitionId), []);
                if (!myInstanzen.SetEquals(partnerInstanzen))
                {
                    var partner = pairing.PersonAId == person.Id ? pairing.PersonB : pairing.PersonA;
                    warnings.Add(new MatchingWarning(
                        $"Partnerschaft mit {partner.FirstName} {partner.LastName} für {pairing.ProfundumDefinition.Bezeichnung} nicht synchron - unterschiedliche Instanzen belegt."));
                }
            }

            yield return new DTOProfundumEnrollmentSet
            {
                Person = new PersonInfoMinimal(person),
                Enrollments = personsEnrollments,
                Wuensche = niceWishes,
                Warnings = warnings,
                Partnerschaften = myPairings.Select(p => new DTOProfundumPartnerWunschStaff(p)),
                ZusatzInformation = zusatzInfoByPerson.GetValueOrDefault(person.Id, ""),
            };
        }
    }

    class SolutionCallBack(in ILogger logger) : CpSolverSolutionCallback
    {
        private readonly ILogger _logger = logger;
        private int _solutionCount;
        public override void OnSolutionCallback()
        {
            _logger.LogInformation("Solution #{numSolution}: time = {time:F2} s, objective value = {objective}",
                _solutionCount,
                WallTime(),
                ObjectiveValue());
            _solutionCount++;
        }
    }
}
