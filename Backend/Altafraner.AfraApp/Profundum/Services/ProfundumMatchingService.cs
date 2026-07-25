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
    private readonly AfraAppContext _dbContext;
    private readonly ILogger _logger;
    private readonly IOptions<ProfundumConfiguration> _profundumConfiguration;
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
        _profundumConfiguration = profundumConfiguration;
        _rulesFactory = rulesFactory;
        _userService = userService;
    }


    /// <summary>
    ///     Perform a matching for the given slots and return information about the result
    /// </summary>
    public async Task<MatchingStats> PerformMatching()
    {
        var stopwatch = Stopwatch.StartNew();

        await _dbContext.ProfundaEinschreibungen
            .Where(e => !e.IsFixed)
            .ExecuteDeleteAsync();

        var slots = _dbContext.ProfundaSlots.Include(s => s.EinwahlZeitraum).ToArray();
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
            .ToArrayAsync();
        var students = _dbContext.Personen.Where(p => p.Rolle == Rolle.Mittelstufe).ToArray();

        if (!_profundumConfiguration.Value.DeterministicMatching)
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

        // Create vars for each possible enrollment
        foreach (var currentSlot in slots)
        {
            var angeboteInSlot = angebote.Where(a => a.Slots.Contains(currentSlot)).ToArray();
            foreach (var currentStudent in students)
            {
                List<BoolVar> personsVariablesInSlot = [];
                var fixE = fixEinschreibungen
                    .SingleOrDefault(e => e.BetroffenePerson == currentStudent
                                         && e.Slot == currentSlot);

                // Not enrolled var
                var nev = model.NewBoolVar($"beleg-{currentStudent.Id}-not-enrolled-in-{currentSlot.Id}");
                personNotEnrolledVariables[(currentStudent, currentSlot)] = nev;
                personsVariablesInSlot.Add(nev);
                if (fixE is not null && fixE.ProfundumInstanz is null)
                {
                    model.Add(nev == 1);
                }

                // angebote vars
                foreach (var currentInstanzInSlot in angeboteInSlot)
                {
                    var currentVar =
                        model.NewBoolVar($"beleg-{currentStudent.Id}-{currentSlot.Id}-{currentInstanzInSlot.Id}");
                    belegVars[(currentStudent, currentSlot, currentInstanzInSlot)] = currentVar;
                    personsVariablesInSlot.Add(currentVar);

                    // fix einschreibungen
                    if (fixE?.ProfundumInstanz == currentInstanzInSlot)
                    {
                        model.Add(currentVar == 1);
                    }
                }
                model.AddExactlyOne(personsVariablesInSlot);
            }
        }

        var cfg = _profundumConfiguration.Value;

        // Set-Up Objective
        foreach (var currentSlot in slots)
        {
            var angeboteInSlot = angebote.Where(a => a.Slots.Contains(currentSlot)).ToArray();
            foreach (var currentStudent in students)
            {
                // Not enrolled var
                var nev = personNotEnrolledVariables[(currentStudent, currentSlot)];
                objective.AddTerm(nev, 1); // Not matched is slightly better than stupid solutions.

                var wuensche = belegwuensche.Where(b => b.BetroffenePerson == currentStudent
                        && b.EinwahlZeitraum.Slots.Contains(currentSlot))
                    .ToArray();

                // angebote vars
                foreach (var currentInstanzInSlot in angeboteInSlot)
                {
                    var currentVar = belegVars[(currentStudent, currentSlot, currentInstanzInSlot)];

                    var wunsch = wuensche.FirstOrDefault(w => w.ProfundumDefinition == currentInstanzInSlot.Profundum);
                    if (wunsch is not null)
                    {
                        objective.AddTerm(currentVar, WunschReward(wunsch.Rang, cfg));
                    }
                }
            }
        }


        var historienByPerson = _userService.LoadGruppenHistorien(students.Select(s => s.Id));

        foreach (var student in students)
        {
            var sBelegWuensche = belegwuensche.Where(w => w.BetroffenePerson == student).ToArray();
            var sBelegVars = belegVars.Where(k => k.Key.p == student)
                .ToDictionary(x => (x.Key.s, x.Key.i), x => x.Value);
            var sNotEnrolledVars = personNotEnrolledVariables.Where(k => k.Key.p == student)
                .ToDictionary(x => x.Key.s, x => x.Value);
            var klasse = UserService.GetKlassenstufe(student, DateTime.UtcNow, historienByPerson);

            foreach (var r in _rulesFactory.GetIndividualRules())
            {
                r.AddConstraints(student,
                    klasse,
                    slots,
                    sBelegWuensche,
                    sBelegVars,
                    sNotEnrolledVars,
                    model,
                    objective
                );
            }
        }

        foreach (var r in _rulesFactory.GetAggregateRules())
            r.AddConstraints(slots, students, belegwuensche, belegVars, model, objective);

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

        var newEinschreibungen = new List<ProfundumEinschreibung>();
        foreach (var p in students)
            foreach (var i in angebote)
                foreach (var s in i.Slots)
                {
                    if (fixEinschreibungen.Any(e => e.BetroffenePerson == p && e.Slot == s))
                    {
                        continue;
                    }

                    if (solver.Value(belegVars[(p, s, i)]) > 0)
                    {
                        newEinschreibungen.Add(new ProfundumEinschreibung
                        {
                            ProfundumInstanz = i,
                            BetroffenePerson = p,
                            Slot = s,
                        });
                    }
                }
        await _dbContext.ProfundaEinschreibungen.AddRangeAsync(newEinschreibungen);
        await _dbContext.SaveChangesAsync();

        var rangVerteilung = new Dictionary<int, int>();
        foreach (var e in newEinschreibungen)
        {
            var wunsch = belegwuensche.FirstOrDefault(w => w.BetroffenePerson == e.BetroffenePerson
                    && w.ProfundumDefinition == e.ProfundumInstanz!.Profundum
                    && w.EinwahlZeitraum.Slots.Contains(e.Slot));
            if (wunsch is null) continue;
            rangVerteilung[wunsch.Rang] = rangVerteilung.GetValueOrDefault(wunsch.Rang) + 1;
        }

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
            RangVerteilung = rangVerteilung,
        };
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
        IReadOnlyDictionary<Guid, List<PersonGruppenHistorie>> historienByPerson,
        ProfundumSlot[] slots,
        ProfundumEinschreibung[] enrollments)
    {
        int KlasseAsOf(DateTime asOf) => UserService.GetKlassenstufe(student, asOf, historienByPerson);
        return _rulesFactory.GetIndividualRules().SelectMany(r => r.GetWarnings(student, KlasseAsOf, slots, enrollments));
    }

    /// <summary>
    ///     Staff-facing view of every Mittelstufe student's current enrollments, wishes, rule warnings, and
    ///     team-partner pairing sync status - independent of whether a matching run just happened, e.g. also used to
    ///     inspect the effect of a manual override.
    /// </summary>
    public async IAsyncEnumerable<DTOProfundumEnrollmentSet> GetAllEnrollmentsAsync()
    {
        var slots = await _dbContext.ProfundaSlots.Include(s => s.EinwahlZeitraum).ToArrayAsync();

        var mittelstufeIds = await _dbContext.Personen
            .Where(p => p.Rolle == Rolle.Mittelstufe)
            .Select(p => p.Id)
            .ToArrayAsync();
        var historienByPerson = _userService.LoadGruppenHistorien(mittelstufeIds);

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

        var personenWithData = _dbContext.Personen
            .AsSplitQuery()
            .Where(p => p.Rolle == Rolle.Mittelstufe)
            .OrderBy(p => p.Gruppe)
            .ThenBy(p => p.LastName)
            .ThenBy(p => p.FirstName)
            .Include(p => p.ProfundaBelegwuensche)
            .ThenInclude(p => p.ProfundumDefinition)
            .ThenInclude(p => p.Instanzen)
            .ThenInclude(i => i.Slots)
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
            var personsEnrollments = slots.Select(slot => (slotId: slot.Id,
                    enrollment: person.ProfundaEinschreibungen.FirstOrDefault(e => e.Slot == slot)))
                .Select(e =>
                    e.enrollment is not null
                        ? new DTOProfundumEnrollment(e.enrollment)
                        : new DTOProfundumEnrollment
                        { ProfundumSlotId = e.slotId, ProfundumInstanzId = null, IsFixed = false });

            var personsWishes = person.ProfundaBelegwuensche
                .Select(e => new DTOWunsch(e.ProfundumDefinition.Id,
                    e.ProfundumDefinition.Instanzen.SelectMany(i => i.Slots).Select(s => s.Id).Distinct(),
                    e.Rang));
            var warnings = GetStudentWarnings(person,
                historienByPerson,
                slots,
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
                Wuensche = personsWishes,
                Warnings = warnings,
                Partnerschaften = myPairings.Select(p => new DTOProfundumPartnerWunschStaff(p)),
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
