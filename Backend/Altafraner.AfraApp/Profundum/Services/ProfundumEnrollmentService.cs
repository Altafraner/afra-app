using System.Diagnostics;
using System.Text;
using Altafraner.AfraApp.Profundum.Configuration;
using Altafraner.AfraApp.Profundum.Domain.Contracts.Services;
using Altafraner.AfraApp.Profundum.Domain.DTO;
using Altafraner.AfraApp.Profundum.Domain.Models;
using Altafraner.AfraApp.User.Domain.DTO;
using Altafraner.AfraApp.User.Domain.Models;
using Altafraner.AfraApp.User.Services;
using Altafraner.Backbone.EmailSchedulingModule;
using Google.OrTools.Sat;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Models_Person = Altafraner.AfraApp.User.Domain.Models.Person;

namespace Altafraner.AfraApp.Profundum.Services;

///
public class ProfundumEinwahlWunschException : Exception
{
    ///
    public ProfundumEinwahlWunschException(string message)
        : base(message)
    {
    }
}

/// <summary>
///     A service for handling enrollments.
/// </summary>
public class ProfundumEnrollmentService
{
    private readonly AfraAppContext _dbContext;
    private readonly ILogger _logger;
    private readonly IOptions<ProfundumConfiguration> _profundumConfiguration;
    private readonly UserService _userService;
    private readonly INotificationService _notificationService;
    private readonly IRulesFactory _rulesFactory;

    /// <summary>
    ///     Constructs the EnrollmentService. Usually called by the DI container.
    /// </summary>
    public ProfundumEnrollmentService(AfraAppContext dbContext,
        ILogger<ProfundumEnrollmentService> logger, UserService userService,
        IOptions<ProfundumConfiguration> profundumConfiguration,
        INotificationService notificationService,
    IRulesFactory rulesFactory)
    {
        _dbContext = dbContext;
        _logger = logger;
        _userService = userService;
        _profundumConfiguration = profundumConfiguration;
        _notificationService = notificationService;
        _rulesFactory = rulesFactory;
    }

    ///
    public bool IsProfundumBlockiert(Models_Person student, IEnumerable<ProfundumQuartal> quartale)
    {
        var klasse = _userService.GetKlassenstufe(student);
        var blockiertQuartale = _profundumConfiguration.Value.ProfundumBlockiert.GetValueOrDefault(klasse);
        if (blockiertQuartale is null)
        {
            return false;
        }

        var ret = blockiertQuartale.Intersect(quartale).Any();
        return ret;
    }

    ///
    public bool IsProfilPflichtig(Models_Person student, IEnumerable<ProfundumQuartal> quartale)
    {
        var klasse = _userService.GetKlassenstufe(student);
        var profilQuartale = _profundumConfiguration.Value.ProfilPflichtigkeit.GetValueOrDefault(klasse);
        if (profilQuartale is null)
        {
            return false;
        }

        var ret = profilQuartale.Intersect(quartale).Any();
        return ret;
    }

    ///
    public bool IsProfilZulässig(Models_Person student, IEnumerable<ProfundumQuartal> quartale)
    {
        var klasse = student.Gruppe;
        if (klasse is null)
        {
            return false;
        }

        var profilQuartale = _profundumConfiguration.Value.ProfilZulassung.GetValueOrDefault(klasse);
        if (profilQuartale is null)
        {
            return false;
        }

        var ret = profilQuartale.Intersect(quartale).Any();
        return ret;
    }

    ///
    public IEnumerable<ProfundumInstanz> GetAvailableProfundaInstanzen(Models_Person student,
        IEnumerable<ProfundumSlot> slots)
    {
        var klasse = _userService.GetKlassenstufe(student);
        var profundumSlots = slots as ProfundumSlot[] ?? slots.ToArray();
        var profilPflichtig = IsProfilPflichtig(student, profundumSlots.Select(s => s.Quartal));
        var profilZulässig = IsProfilZulässig(student, profundumSlots.Select(s => s.Quartal));
        var profundaInstanzen = _dbContext.ProfundaInstanzen
            .Include(p => p.Slots)
            .Include(p => p.Profundum).ThenInclude(p => p.Kategorie)
            .Where(p => (p.Profundum.MinKlasse == null || klasse >= p.Profundum.MinKlasse)
                        && (p.Profundum.MaxKlasse == null || klasse <= p.Profundum.MaxKlasse))
            .Where(p => !p.Profundum.Kategorie.ProfilProfundum || profilPflichtig || profilZulässig)
            .ToArray()
            .Where(p => p.Slots.Any(s => profundumSlots.Any(sl => sl.Id == s.Id)))
            .Where(p => p.Slots.All(s => profundumSlots.Any(sl => sl.Id == s.Id)));
        return profundaInstanzen;
    }

    ///
    public ProfundumEinwahlZeitraum? GetCurrentEinwahlZeitraum()
    {
        var now = DateTime.UtcNow;
        return _dbContext.ProfundumEinwahlZeitraeume
            .Include(ez => ez.Slots)
            .ToArray()
            .FirstOrDefault(defaultValue: null);
    }

    /// <summary>
    ///     Get all options for slots currently open for enrollment
    /// </summary>
    public ICollection<BlockKatalog> GetKatalog(Models_Person student)
    {
        var einwahlZeitraum = GetCurrentEinwahlZeitraum();
        if (einwahlZeitraum is null)
        {
            return [];
        }

        var blockiert = IsProfundumBlockiert(student, einwahlZeitraum.Slots.Select(s => s.Quartal));
        if (blockiert)
        {
            return [];
        }

        var slots = einwahlZeitraum.Slots.Order(new ProfundumSlotComparer()).ToArray();

        var katalog = new List<BlockKatalog>();
        var angebote = GetAvailableProfundaInstanzen(student, slots).ToArray();

        foreach (var slot in slots)
        {
            var profundumInstanzenBeginningInSlot = angebote
                .Where(p => p.Slots.Count != 0 && p.Slots.Min(new ProfundumSlotComparer())!.Id == slot.Id);

            katalog.Add(new BlockKatalog
            {
                Label = $"{slot.Jahr} {slot.Quartal} {slot.Wochentag switch
                {
                    DayOfWeek.Monday => "Montag",
                    DayOfWeek.Tuesday => "Dienstag",
                    DayOfWeek.Wednesday => "Mittwoch",
                    DayOfWeek.Thursday => "Donnerstag",
                    DayOfWeek.Friday => "Freitag",
                    DayOfWeek.Saturday => "Samstag",
                    DayOfWeek.Sunday => "Sonntag",
                    _ => ""
                }}",
                Id = slot.ToString(),
                Options = profundumInstanzenBeginningInSlot
                    .OrderBy(x => !x.Profundum.Kategorie.ProfilProfundum)
                    .ThenBy(x => x.Profundum.Bezeichnung)
                    .Select(p => new BlockOption
                    {
                        Label = p.Slots.Count <= 1
                            ? p.Profundum.Bezeichnung
                            : $"{p.Profundum.Bezeichnung} ({p.Slots.Count} Quartale)",
                        Value = p.Id,
                        AlsoIncludes = p.Slots.Order(new ProfundumSlotComparer())
                            .Skip(1)
                            .Select(s => s.ToString()).ToArray()
                    }).ToArray()
            });
        }

        return katalog;
    }

    /// <summary>
    ///     Register a set of Profundum Belegwuensche.
    ///     Validates that all currently open slots are filled
    /// </summary>
    /// <param name="student">The student wanting to enroll</param>
    /// <param name="wuensche">A dictionary containing the ordered ids of ProdundumInstanzen given the slot</param>
    public async Task RegisterBelegWunschAsync(Models_Person student, Dictionary<String, Guid[]> wuensche)
    {
        var einwahlZeitraum = GetCurrentEinwahlZeitraum();
        if (einwahlZeitraum is null)
        {
            throw new ProfundumEinwahlWunschException("Momentan keine offene Einschreibung");
        }

        var slots = einwahlZeitraum.Slots;
        if (slots is null)
        {
            throw new ProfundumEinwahlWunschException("einwahlzeitraum hat keine slots");
        }

        var blockiert = IsProfundumBlockiert(student, slots.Select(s => s.Quartal));
        if (blockiert)
        {
            throw new ProfundumEinwahlWunschException("Klassenstufe vom Profundum ausgeschlossen.");
        }

        var konflikte = _dbContext.ProfundaBelegWuensche
            .Include(bw => bw.ProfundumInstanz)
            .ThenInclude(pi => pi.Slots)
            .ThenInclude(profundumSlot => profundumSlot.EinwahlZeitraum)
            .Include(bw => bw.BetroffenePerson)
            .Where(p => p.BetroffenePerson.Id == student.Id)
            .AsEnumerable()
            .Where(p => p.ProfundumInstanz.Slots.Any(s => s.EinwahlZeitraum.Id == einwahlZeitraum.Id))
            .ToArray();
        if (konflikte.Any())
        {
            _dbContext.ProfundaBelegWuensche.RemoveRange(konflikte);
        }

        var angebote = GetAvailableProfundaInstanzen(student, slots).ToHashSet();
        var angeboteUsed = new HashSet<ProfundumInstanz>();

        var wuenscheDict = new Dictionary<ProfundumBelegWunschStufe, HashSet<ProfundumInstanz>>
        {
            [ProfundumBelegWunschStufe.ErstWunsch] = [],
            [ProfundumBelegWunschStufe.ZweitWunsch] = [],
            [ProfundumBelegWunschStufe.DrittWunsch] = []
        };

        foreach (var (str, l) in wuensche)
        {
            var s = slots.FirstOrDefault(sm => sm.ToString() == str);
            if (s is null)
            {
                throw new ProfundumEinwahlWunschException("Kein solcher Slot");
            }

            if (l.Length != 3)
            {
                throw new ProfundumEinwahlWunschException("Zu viele Wünsche für einen Slot");
            }

            for (var i = 0; i < l.Length; ++i)
            {
                if (!Enum.IsDefined(typeof(ProfundumBelegWunschStufe), i + 1))
                {
                    throw new ProfundumEinwahlWunschException("Belegwunschstufe nicht definiert.");
                }

                var stufe = (ProfundumBelegWunschStufe)(i + 1);

                if (angeboteUsed.FirstOrDefault(a => a.Id == l[i]) is not null)
                {
                    continue;
                }

                var angebot = angebote.FirstOrDefault(a => a.Id == l[i]);
                if (angebot is null)
                {
                    throw new ProfundumEinwahlWunschException($"Profundum nicht gefundum {l[i]}.");
                }

                wuenscheDict[stufe].Add(angebot);
                angebote.Remove(angebot);
                angeboteUsed.Add(angebot);
            }
        }

        var einwahl = new Dictionary<ProfundumSlot, ProfundumInstanz?[]>();
        foreach (var s in slots)
        {
            einwahl[s] = new ProfundumInstanz?[3];
        }

        var belegWuensche = new HashSet<ProfundumBelegWunsch>();
        foreach (var (stufe, instanzen) in wuenscheDict)
        {
            foreach (var angebot in instanzen)
            {
                foreach (var angebotSlot in angebot.Slots)
                {
                    int stufeIndex = (int)stufe - 1;
                    if (einwahl[angebotSlot][stufeIndex] is not null)
                    {
                        throw new ProfundumEinwahlWunschException("Überlappende Slots in der Einwahl.");
                    }

                    einwahl[angebotSlot][stufeIndex] = angebot;
                }
            }
        }

        if (slots.SelectMany(s => einwahl[s]).Any(pi => pi is null))
        {
            throw new ProfundumEinwahlWunschException("Leerer Slot in Einwahl.");
        }

        foreach (var (stufe, instanzen) in wuenscheDict)
        {
            foreach (var angebot in instanzen)
            {
                var belegWunsch = new ProfundumBelegWunsch
                {
                    BetroffenePerson = student,
                    ProfundumInstanz = angebot,
                    Stufe = stufe,
                    // EinwahlZeitraum = einwahlZeitraum,
                };
                belegWuensche.Add(belegWunsch);
            }
        }

        foreach (var r in _rulesFactory.GetIndividualRules())
        {
            var status = r.CheckForSubmission(student, einwahlZeitraum, belegWuensche);
            if (!status.IsValid)
            {
                throw new ProfundumEinwahlWunschException(status.Messages
                        .Aggregate(new StringBuilder(), (a, b) => a.AppendLine(b))
                        .ToString());
            }
        }

        var kategorien = await _dbContext.ProfundaKategorien.Where(k => k.MaxProEinwahl != null).ToArrayAsync();
        foreach (var kat in kategorien)
        {
            var n = belegWuensche.Count(b => b.ProfundumInstanz.Profundum.Kategorie == kat);
            if (n > kat.MaxProEinwahl)
            {
                throw new ProfundumEinwahlWunschException(
                    $"Nur {kat.MaxProEinwahl} Profunda der Kategorie {kat.Bezeichnung} wählbar");
            }
        }

        await sendWuenscheEMail(student, einwahlZeitraum, belegWuensche);

        _dbContext.ProfundaBelegWuensche.AddRange(belegWuensche);
        await _dbContext.SaveChangesAsync();
    }

    private async Task sendWuenscheEMail(Models_Person student,
            ProfundumEinwahlZeitraum einwahlZeitraum,
            IEnumerable<ProfundumBelegWunsch> wuensche)
    {
        var slots = einwahlZeitraum.Slots;

        var wuenscheString = new StringBuilder();
        wuenscheString.AppendLine("Du hast die folgenden Wünsche zur Profundumseinwahl abgegeben.");
        wuenscheString.AppendLine("Falls du eine Änderung vornehmen möchtest, fülle das Formular neu aus.");
        wuenscheString.AppendLine();
        foreach (var slot in slots)
        {
            var slotString = $"{slot.Jahr} {slot.Quartal} {slot.Wochentag switch
            {
                DayOfWeek.Monday => "Montag",
                DayOfWeek.Tuesday => "Dienstag",
                DayOfWeek.Wednesday => "Mittwoch",
                DayOfWeek.Thursday => "Donnerstag",
                DayOfWeek.Friday => "Freitag",
                DayOfWeek.Saturday => "Samstag",
                DayOfWeek.Sunday => "Sonntag",
                _ => "",
            }}";
            wuenscheString.AppendLine($"{slotString}: ");

            foreach (var b in wuensche.Where(b => b.ProfundumInstanz.Slots.Contains(slot)))
            {
                wuenscheString.AppendLine($"    {(int)b.Stufe}. {b.ProfundumInstanz.Profundum.Bezeichnung}");
            }
        }

        await _notificationService.ScheduleNotificationAsync(student, "Deine Profunda Einwahl-Wünsche",
            wuenscheString.ToString(), TimeSpan.Zero);
    }

    ///
    public async Task<string> GetStudentMatchingCsv(ProfundumEinwahlZeitraum einwahlZeitraum)
    {
        var personen = await _dbContext.Personen
            .AsSplitQuery()
            .Include(s => s.ProfundaEinschreibungen)
            .ThenInclude(e => e.ProfundumInstanz)
            .ThenInclude(e => e.Profundum)
            .Include(person => person.ProfundaEinschreibungen)
            .ThenInclude(profundumEinschreibung => profundumEinschreibung.ProfundumInstanz)
            .ThenInclude(profundumInstanz => profundumInstanz.Slots)
            .Where(p => p.Rolle == Rolle.Mittelstufe)
            .Where(p => p.ProfundaEinschreibungen.Any())
            .ToArrayAsync();

        var slots = einwahlZeitraum.Slots;

        const char sep = '\t';

        var sb = new StringBuilder();
        sb.AppendLine(
            $"Klasse{sep} Name{sep} Vorname{slots.Select(s => s.ToString()).Aggregate("", (r, c) => $"{r}{sep} {c}")}");

        foreach (var student in personen)
        {
            sb.AppendLine($"{student.Gruppe}{sep} {student.LastName}{sep} {student.FirstName}{slots.Select(s =>
                student.ProfundaEinschreibungen
                    .Where(e => e.ProfundumInstanz.Slots.Any(sl => sl.Id == s.Id))
                    .Select(e => e.ProfundumInstanz.Profundum.Bezeichnung)
                    .First()
            ).Aggregate("", (r, c) => $"{r}{sep} {c}")}");
        }

        return sb.ToString();
    }

    /// <summary>
    ///     Perform a matching for the given slots and return information about the result
    /// </summary>
    /// <param name="einwahlZeitraum">The einwahlZeitraum to apply the matching to</param>
    /// <param name="writeBackOnSuccess">Whether to write the enrollments to db on complete matching</param>
    public async Task<MatchingStats> PerformMatching(ProfundumEinwahlZeitraum einwahlZeitraum,
        bool writeBackOnSuccess = false)
    {
        var slots = einwahlZeitraum.Slots.ToArray();

        var alteAutomatischeEinschreibungen = _dbContext.ProfundaEinschreibungen
            .Where(e => !e.IsFixed)
            .Where(e => e.ProfundumInstanz.Slots.Any(s => slots.Contains(s)));
        _logger.LogInformation("delting {numEnrollments} old enrollments", alteAutomatischeEinschreibungen.Count());
        _dbContext.RemoveRange(alteAutomatischeEinschreibungen);
        await _dbContext.SaveChangesAsync();


        var angebote = (await _dbContext.ProfundaInstanzen
                .Include(pi => pi.Slots)
                .Include(pi => pi.Profundum)
                .ToArrayAsync())
            .Where(pi => pi.Slots.Any(s => slots.Any(sl => sl.Id == s.Id)))
            .ToArray();
        var angeboteList = angebote.ToList();
        var belegwuensche = await _dbContext.ProfundaBelegWuensche
            .Include(b => b.BetroffenePerson)
            .Include(b => b.ProfundumInstanz).ThenInclude(pi => pi.Profundum).ThenInclude(p => p.Kategorie)
            .Where(b => angeboteList.Contains(b.ProfundumInstanz))
            .ToArrayAsync();
        var students = belegwuensche.Select(b => b.BetroffenePerson).ToHashSet().ToArray();

        if (!_profundumConfiguration.Value.DeterministicMatching)
        {
            Random.Shared.Shuffle(angebote);
            Random.Shared.Shuffle(belegwuensche);
            Random.Shared.Shuffle(students);
        }

        var model = new CpModel();
        var modelOnlyIndividualRules = new CpModel();
        var objective = LinearExpr.NewBuilder();
        var objectiveOnlyIndividualRules = LinearExpr.NewBuilder();

        var weights = new Dictionary<ProfundumBelegWunschStufe, int>
        {
            { ProfundumBelegWunschStufe.ErstWunsch, 100 },
            { ProfundumBelegWunschStufe.ZweitWunsch, 50 },
            { ProfundumBelegWunschStufe.DrittWunsch, 25 },
        }.AsReadOnly();

        var belegVariables = new Dictionary<ProfundumBelegWunsch, BoolVar>();
        var belegVariablesOnlyIndividualRules = new Dictionary<ProfundumBelegWunsch, BoolVar>();
        foreach (var wunsch in belegwuensche)
        {
            belegVariables[wunsch] =
                model.NewBoolVar($"beleg-{wunsch.BetroffenePerson.Id}-{wunsch.ProfundumInstanz.Id}");
            belegVariablesOnlyIndividualRules[wunsch] =
                modelOnlyIndividualRules.NewBoolVar($"beleg-{wunsch.BetroffenePerson.Id}-{wunsch.ProfundumInstanz.Id}");
        }

        long notMatchedPenalty = einwahlZeitraum.Slots.Count() * weights[ProfundumBelegWunschStufe.ErstWunsch] *
                                 students.Count();
        var PersonNotEnrolledVariables = new Dictionary<Models_Person, BoolVar>();
        var PersonNotEnrolledVariablesOnlyIndividualRules = new Dictionary<Models_Person, BoolVar>();
        foreach (var student in students)
        {
            PersonNotEnrolledVariables[student] = model.NewBoolVar($"beleg-{student.Id}-not-enrolled");
            objective.AddTerm(PersonNotEnrolledVariables[student], -notMatchedPenalty);
            PersonNotEnrolledVariablesOnlyIndividualRules[student] =
                modelOnlyIndividualRules.NewBoolVar($"beleg-{student.Id}-not-enrolled");
            objectiveOnlyIndividualRules.AddTerm(PersonNotEnrolledVariablesOnlyIndividualRules[student], -notMatchedPenalty);
        }

        // Exact eine Einschreibung pro Slot und Person
        // Gewichtung nach Einwahlstufe
        foreach (var s in slots)
        {
            foreach (var p in students)
            {
                var psBeleg = belegwuensche
                    .Where(b => b.BetroffenePerson.Id == p.Id)
                    .Where(b => b.ProfundumInstanz.Slots.Contains(s)).ToArray();
                var psBelegVar = psBeleg.Select(b => belegVariables[b]).Append(PersonNotEnrolledVariables[p]).ToArray();
                var psBelegVarOnlyIndividualRules = psBeleg.Select(b => belegVariablesOnlyIndividualRules[b])
                    .Append(PersonNotEnrolledVariablesOnlyIndividualRules[p]).ToArray();
                model.AddExactlyOne(psBelegVar);
                modelOnlyIndividualRules.AddExactlyOne(psBelegVarOnlyIndividualRules);
                for (int i = 0; i < psBeleg.Length; ++i)
                {
                    objective.AddTerm(psBelegVar[i], weights[psBeleg[i].Stufe]);
                    objectiveOnlyIndividualRules.AddTerm(psBelegVarOnlyIndividualRules[i], weights[psBeleg[i].Stufe]);
                }
            }
        }

        var alteEinschreibungen = _dbContext.ProfundaEinschreibungen.Where(e => e.IsFixed);
        foreach (var e in alteEinschreibungen)
        {
        }

        foreach (var r in _rulesFactory.GetIndividualRules())
        {
            foreach (var s in students)
            {
                var sBelegWuensche = belegwuensche.Where(w => w.BetroffenePerson.Id == s.Id);
                r.AddConstraints(s, einwahlZeitraum,
                    sBelegWuensche, belegVariables,
                    PersonNotEnrolledVariables[s], model);
                r.AddConstraints(s, einwahlZeitraum,
                    sBelegWuensche, belegVariablesOnlyIndividualRules,
                    PersonNotEnrolledVariables[s], modelOnlyIndividualRules);
            }
        }

        foreach (var r in _rulesFactory.GetAggregateRules())
        {
            r.AddConstraints(einwahlZeitraum, students, belegwuensche, belegVariables, model);
        }

        model.Maximize(objective);
        modelOnlyIndividualRules.Maximize(objectiveOnlyIndividualRules);
        var solver = new CpSolver();
        var solverOnlyIndividualRules = new CpSolver();
        var resultStatus = solver.Solve(model);
        var resultStatusOnlyIndividualRules = solverOnlyIndividualRules.Solve(modelOnlyIndividualRules);

        if (resultStatus != CpSolverStatus.Optimal && resultStatus != CpSolverStatus.Feasible)
        {
            if (resultStatusOnlyIndividualRules != CpSolverStatus.Optimal &&
                resultStatusOnlyIndividualRules != CpSolverStatus.Feasible)
            {
                throw new ArgumentException(
                    "No solution found in Matching likely due to errors in non-capacity constraints.");
            }

            throw new ArgumentException("No solution found in Matching due to capacity constraints");
        }


        var matchingResultStatus = (solver.ObjectiveValue, solverOnlyIndividualRules.ObjectiveValue) switch
        {
            ( >= 0, >= 0) => MatchingResultStatus.MatchingFound,
            ( < 0, >= 0) => MatchingResultStatus.MatchingIncompleteDueToCapacity,
            ( < 0, < 0) => MatchingResultStatus.MatchingIncompleteDueToHardConstraints,
            _ => throw new UnreachableException()
        };


        if (writeBackOnSuccess && matchingResultStatus == MatchingResultStatus.MatchingFound)
        {
            // Ergebnis rückschreiben
            foreach (var bw in belegwuensche)
            {
                var bwVar = belegVariables[bw];
                if (solver.Value(bwVar) > 0)
                {
                    _dbContext.ProfundaEinschreibungen.Add(new ProfundumEinschreibung()
                    {
                        ProfundumInstanz = bw.ProfundumInstanz,
                        BetroffenePerson = bw.BetroffenePerson
                    });
                }
            }

            einwahlZeitraum.HasBeenMatched = true;
            await _dbContext.SaveChangesAsync();
        }


        return new MatchingStats
        {
            CalculationTime = solver.WallTime(),
            Result = matchingResultStatus,
            ObjectiveValue = solver.ObjectiveValue,
            ObjectiveValueNoLimits = solverOnlyIndividualRules.ObjectiveValue,
            Optim = solverOnlyIndividualRules.ObjectiveValue == 0
                ? 0
                : solver.ObjectiveValue / solverOnlyIndividualRules.ObjectiveValue,
            Students = students.ToDictionary(
                p => $"{p.Gruppe}: {p.FirstName} {p.LastName}",
                p =>
                {
                    double score = belegwuensche
                        .Where(x => x.BetroffenePerson.Id == p.Id)
                        .Select(bw =>
                            solver.Value(belegVariables[bw]) * weights[bw.Stufe] * bw.ProfundumInstanz.Slots.Count)
                        .Sum();
                    double scorePossible = belegwuensche
                        .Where(x => x.BetroffenePerson.Id == p.Id)
                        .Select(bw =>
                            solverOnlyIndividualRules.Value(belegVariablesOnlyIndividualRules[bw]) * weights[bw.Stufe] *
                            bw.ProfundumInstanz.Slots.Count()).Sum();
                    return new StudentMatchingStats
                    {
                        Optim = scorePossible == 0 ? 0 : score / scorePossible,
                        Einschreibungen = slots.ToDictionary(s => s.ToString(),
                            s => _dbContext.ProfundaEinschreibungen.Include(e => e.ProfundumInstanz)
                                .ThenInclude(e => e.Profundum)
                                .Where(e => e.BetroffenePerson == p)
                                .Where(e => e.ProfundumInstanz.Slots.Contains(s))
                                .Select(e => e.ProfundumInstanz.Profundum.Bezeichnung).ToArray()
                        ),
                        Wuensche = slots.ToDictionary(s => s.ToString(),
                            s => _dbContext.ProfundaBelegWuensche
                                .Include(e => e.ProfundumInstanz)
                                .ThenInclude(e => e.Profundum)
                                .Where(e => e.BetroffenePerson == p)
                                .Where(e => e.ProfundumInstanz.Slots.Contains(s))
                                .ToArray()
                                .OrderBy(e => (int)e.Stufe)
                                .Select(e => e.ProfundumInstanz.Profundum.Bezeichnung).ToArray()
                        )
                    };
                }),
            Profunda = angebote
                .Where(a => _dbContext.ProfundaEinschreibungen
                    .Include(e => e.ProfundumInstanz)
                    .Any(e => e.ProfundumInstanz.Id == a.Id))
                .ToDictionary(a => $"{a.Slots.First()} {a.Profundum.Bezeichnung} {a.Id}",
                    a =>
                    new ProfundumMatchingStats
                    {
                        Einschreibungen = _dbContext.ProfundaEinschreibungen
                            .Include(e => e.ProfundumInstanz)
                            .Count(e => e.ProfundumInstanz.Id == a.Id),
                        MaxEinschreibungen = a.MaxEinschreibungen
                    }),
            NotMatchedStudents = students.Where(p => solver.Value(PersonNotEnrolledVariables[p]) > 0)
                .Select(p => $"{p.Gruppe}: {p.FirstName} {p.LastName}").ToList()
        };
    }

    ///
    public async Task<Dictionary<string, DTOProfundumDefinition>> GetEnrollment(Models_Person student,
        ICollection<Guid> slotIds)
    {
        return (await _dbContext.ProfundaSlots.Where(s => slotIds.Contains(s.Id)).ToArrayAsync()).ToDictionary(
            s => s.ToString(), s =>
                _dbContext.ProfundaEinschreibungen
                    .Include(pe => pe.ProfundumInstanz).ThenInclude(pi => pi.Profundum).ThenInclude(p => p.Kategorie)
                    .Where(pe => pe.BetroffenePerson.Id == student.Id)
                    .Where(p => p.ProfundumInstanz.Slots.Contains(s))
                    .Select(pe => new DTOProfundumDefinition(pe.ProfundumInstanz.Profundum))
                    .First());
    }
}
