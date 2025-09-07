using System.Diagnostics;
using System.Text;
using Afra_App.Backbone.Email.Services.Contracts;
using Afra_App.Profundum.Configuration;
using Afra_App.Profundum.Domain.DTO;
using Afra_App.Profundum.Domain.Models;
using Afra_App.User.Domain.DTO;
using Afra_App.User.Domain.Models;
using Afra_App.User.Services;
using Afra_App.Profundum.Domain.Contracts.Services;
using Google.OrTools.Sat;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Person = Afra_App.User.Domain.Models.Person;

namespace Afra_App.Profundum.Services;

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
    private readonly IEmailOutbox _emailOutbox;
    private readonly ILogger _logger;
    private readonly IOptions<ProfundumConfiguration> _profundumConfiguration;
    private readonly UserService _userService;
    private readonly IRulesFactory _rulesFactory;

    /// <summary>
    ///     Constructs the EnrollmentService. Usually called by the DI container.
    /// </summary>
    public ProfundumEnrollmentService(AfraAppContext dbContext,
        ILogger<ProfundumEnrollmentService> logger, UserService userService,
        IOptions<ProfundumConfiguration> profundumConfiguration,
        IEmailOutbox emailOutbox, IRulesFactory rulesFactory)
    {
        _dbContext = dbContext;
        _logger = logger;
        _userService = userService;
        _profundumConfiguration = profundumConfiguration;
        _emailOutbox = emailOutbox;
        _rulesFactory = rulesFactory;
    }

    ///
    public bool isProfundumBlockiert(Person student, IEnumerable<ProfundumQuartal> quartale)
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
    public bool isProfilPflichtig(Person student, IEnumerable<ProfundumQuartal> quartale)
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
    public bool isProfilZulässig(Person student, IEnumerable<ProfundumQuartal> quartale)
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
    public IEnumerable<ProfundumInstanz> GetAvailableProfundaInstanzen(Person student, IEnumerable<ProfundumSlot> slots)
    {
        var klasse = _userService.GetKlassenstufe(student);
        var profilPflichtig = isProfilPflichtig(student, slots.Select(s => s.Quartal));
        var profilZulässig = isProfilZulässig(student, slots.Select(s => s.Quartal));
        var profundaInstanzen = _dbContext.ProfundaInstanzen
            .Include(p => p.Slots)
            .Include(p => p.Profundum).ThenInclude(p => p.Kategorie)
            .Where(p => (p.Profundum.minKlasse == null || klasse >= p.Profundum.minKlasse)
                        && (p.Profundum.maxKlasse == null || klasse <= p.Profundum.maxKlasse))
            .Where(p => !p.Profundum.Kategorie.ProfilProfundum || profilPflichtig || profilZulässig)
            .ToArray()
            .Where(p => p.Slots.Any(s => slots.Any(sl => sl.Id == s.Id)))
            .Where(p => !p.Slots.Any(s => !slots.Any(sl => sl.Id == s.Id)));
        return profundaInstanzen;
    }

    ///
    public ProfundumEinwahlZeitraum? getCurrentEinwahlZeitraum()
    {
        var now = DateTime.UtcNow;
        return _dbContext.ProfundumEinwahlZeitraeume
            .Include(ez => ez.Slots)
            .Where(ez => ez.EinwahlStart <= now && now < ez.EinwahlStop)
            .ToArray()
            .FirstOrDefault(defaultValue: null);
    }

    /// <summary>
    ///     Get all options for slots currently open for enrollment
    /// </summary>
    public ICollection<BlockKatalog>? GetKatalog(Person student)
    {
        var einwahlZeitraum = getCurrentEinwahlZeitraum();
        if (einwahlZeitraum is null)
        {
            return [];
        }

        var blockiert = isProfundumBlockiert(student, einwahlZeitraum.Slots.Select(s => s.Quartal));
        if (blockiert)
        {
            return [];
        }

        var slots = einwahlZeitraum.Slots.Order(new ProfundumSlotComparer());

        var katalog = new List<BlockKatalog>() { };
        var angebote = GetAvailableProfundaInstanzen(student, slots);

        foreach (var slot in slots)
        {
            var profundumInstanzenBeginningInSlot = angebote
                .Where(p => p.Slots.Any() && p.Slots.Min(new ProfundumSlotComparer())!.Id == slot.Id);

            katalog.Add(new BlockKatalog
            {
                label = $"{slot.Jahr} {slot.Quartal} {slot.Wochentag switch
                {
                    DayOfWeek.Monday => "Montag",
                    DayOfWeek.Tuesday => "Dienstag",
                    DayOfWeek.Wednesday => "Mittwoch",
                    DayOfWeek.Thursday => "Donnerstag",
                    DayOfWeek.Friday => "Freitag",
                    DayOfWeek.Saturday => "Samstag",
                    DayOfWeek.Sunday => "Sonntag",
                    _ => "",
                }}",
                id = slot.ToString(),
                options = profundumInstanzenBeginningInSlot
                    .OrderBy(x => !x.Profundum.Kategorie.ProfilProfundum)
                    .ThenBy(x => x.Profundum.Bezeichnung)
                    .Select(p => new BlockOption
                    {
                        label = p.Slots.Count() <= 1
                            ? p.Profundum.Bezeichnung
                            : $"{p.Profundum.Bezeichnung} ({p.Slots.Count()} Quartale)",
                        value = p.Id,
                        alsoIncludes = p.Slots.Order(new ProfundumSlotComparer()).Skip(1)
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
    public async Task RegisterBelegWunschAsync(Person student, Dictionary<String, Guid[]> wuensche)
    {
        var einwahlZeitraum = getCurrentEinwahlZeitraum();
        if (einwahlZeitraum is null)
        {
            throw new ProfundumEinwahlWunschException("Momentan keine offene Einschreibung");
        }

        var slots = einwahlZeitraum.Slots;
        if (slots is null)
        {
            throw new ProfundumEinwahlWunschException("einwahlzeitraum hat keine slots");
        }

        var blockiert = isProfundumBlockiert(student, slots.Select(s => s.Quartal));
        if (blockiert)
        {
            throw new ProfundumEinwahlWunschException("Klassenstufe vom Profundum ausgeschlossen.");
        }

        var konflikte = _dbContext.ProfundaBelegWuensche
            .Include(bw => bw.ProfundumInstanz).ThenInclude(pi => pi.Slots)
            .Include(bw => bw.BetroffenePerson)
            .Where(p => p.BetroffenePerson.Id == student.Id)
            .ToArray()
            .Where(p => p.ProfundumInstanz.Slots.Any(s => s.EinwahlZeitraum.Id == einwahlZeitraum.Id));
        if (konflikte.Any())
        {
            _dbContext.ProfundaBelegWuensche.RemoveRange(konflikte);
        }

        var angebote = GetAvailableProfundaInstanzen(student, slots).ToHashSet();
        var angeboteUsed = new HashSet<ProfundumInstanz>();

        var wuenscheDict = new Dictionary<ProfundumBelegWunschStufe, HashSet<ProfundumInstanz>>();
        wuenscheDict[ProfundumBelegWunschStufe.ErstWunsch] = new();
        wuenscheDict[ProfundumBelegWunschStufe.ZweitWunsch] = new();
        wuenscheDict[ProfundumBelegWunschStufe.DrittWunsch] = new();

        foreach (var (str, l) in wuensche)
        {
            var s = slots.Where(sm => sm.ToString() == str).FirstOrDefault();
            if (s is null)
            {
                throw new ProfundumEinwahlWunschException("Kein solcher Slot");
            }

            if (l.Length != 3)
            {
                throw new ProfundumEinwahlWunschException("Zu viele Wünsche für einen Slot");
            }

            for (int i = 0; i < l.Length; ++i)
            {
                if (!ProfundumBelegWunschStufe.IsDefined(typeof(ProfundumBelegWunschStufe), i + 1))
                {
                    throw new ProfundumEinwahlWunschException("Belegwunschstufe nicht definiert.");
                }

                var stufe = (ProfundumBelegWunschStufe)(i + 1);

                if (angeboteUsed.Where(a => a.Id == l[i]).FirstOrDefault() is not null)
                {
                    continue;
                }

                var angebot = angebote.Where(a => a.Id == l[i]).FirstOrDefault();
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

        foreach (var s in slots)
        {
            foreach (var pi in einwahl[s])
            {
                if (pi is null)
                {
                    throw new ProfundumEinwahlWunschException("Leerer Slot in Einwahl.");
                }
            }
        }

        foreach (var (stufe, instanzen) in wuenscheDict)
        {
            foreach (var angebot in instanzen)
            {
                var belegWunsch = new ProfundumBelegWunsch
                {
                    BetroffenePerson = student,
                    ProfundumInstanz = angebot,
                    Stufe = stufe
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
            int n = belegWuensche.Count(b => b.ProfundumInstanz.Profundum.Kategorie == kat);
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

    private Task sendWuenscheEMail(Person student,
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

        return _emailOutbox.ScheduleNotificationAsync(student, "Deine Profunda Einwahl-Wünsche",
            wuenscheString.ToString(), TimeSpan.Zero);
    }

    ///
    public async Task<string> GetStudentMatchingCSV(ProfundumEinwahlZeitraum einwahlZeitraum)
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
            var enrollments = _dbContext.ProfundaEinschreibungen.Where(e => e.BetroffenePersonId == student.Id)
                .Include(e => e.ProfundumInstanz).ThenInclude(pi => pi.Slots);

            sb.AppendLine($"{student.Gruppe}{sep} {student.Nachname}{sep} {student.Vorname}{slots.Select(s =>
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
        if (einwahlZeitraum.HasBeenMatched)
        {
            throw new ArgumentException("Final matching has been already performed.");
        }

        var slots = einwahlZeitraum.Slots.ToArray();

        var alteEinschreibungen = _dbContext.ProfundaEinschreibungen
            .Where(e => e.ProfundumInstanz.Slots.Any(s => slots.Contains(s)));
        _logger.LogInformation("delting {numEnrollments} old enrollments", alteEinschreibungen.Count());
        _dbContext.RemoveRange(alteEinschreibungen);
        await _dbContext.SaveChangesAsync();

        var angebote = (await _dbContext.ProfundaInstanzen
                .Include(pi => pi.Slots)
                .Include(pi => pi.Profundum)
                .ToArrayAsync())
            .Where(pi => pi.Slots.Any(s => slots.Any(sl => sl.Id == s.Id)))
            .ToArray();
        var belegwünsche = await _dbContext.ProfundaBelegWuensche
            .Include(b => b.BetroffenePerson)
            .Include(b => b.ProfundumInstanz).ThenInclude(pi => pi.Profundum).ThenInclude(p => p.Kategorie)
            .Where(b => angebote.Contains(b.ProfundumInstanz))
            .ToArrayAsync();
        var students = belegwünsche.Select(b => b.BetroffenePerson).ToHashSet().ToArray();

        if (!_profundumConfiguration.Value.deterministicMatching)
        {
            Random.Shared.Shuffle(angebote);
            Random.Shared.Shuffle(belegwünsche);
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
        foreach (var wunsch in belegwünsche)
        {
            belegVariables[wunsch] =
                model.NewBoolVar($"beleg-{wunsch.BetroffenePerson.Id}-{wunsch.ProfundumInstanz.Id}");
            belegVariablesOnlyIndividualRules[wunsch] =
                modelOnlyIndividualRules.NewBoolVar($"beleg-{wunsch.BetroffenePerson.Id}-{wunsch.ProfundumInstanz.Id}");
        }

        long notMatchedPenalty = einwahlZeitraum.Slots.Count() * weights[ProfundumBelegWunschStufe.ErstWunsch] *
                                 students.Count();
        var PersonNotEnrolledVariables = new Dictionary<Person, BoolVar>();
        var PersonNotEnrolledVariablesOnlyIndividualRules = new Dictionary<Person, BoolVar>();
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
                var psBeleg = belegwünsche
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

        foreach (var r in _rulesFactory.GetIndividualRules())
        {
            foreach (var s in students)
            {
                var sBelegWuensche = belegwünsche.Where(w => w.BetroffenePerson.Id == s.Id);
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
            r.AddConstraints(einwahlZeitraum, students, belegwünsche, belegVariables, model);
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
            else
            {
                throw new ArgumentException("No solution found in Matching due to capacity constraints");
            }
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
            foreach (var bw in belegwünsche)
            {
                var bw_var = belegVariables[bw];
                if (solver.Value(bw_var) > 0)
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
                p => $"{p.Gruppe}: {p.Vorname} {p.Nachname}",
                p =>
                {
                    double score = belegwünsche
                        .Where(x => x.BetroffenePerson.Id == p.Id)
                        .Select(bw =>
                            solver.Value(belegVariables[bw]) * weights[bw.Stufe] * bw.ProfundumInstanz.Slots.Count())
                        .Sum();
                    double scorePossible = belegwünsche
                        .Where(x => x.BetroffenePerson.Id == p.Id)
                        .Select(bw =>
                            solverOnlyIndividualRules.Value(belegVariablesOnlyIndividualRules[bw]) * weights[bw.Stufe] *
                            bw.ProfundumInstanz.Slots.Count()).Sum();
                    return new StudentMatchingStats
                    {
                        optim = scorePossible == 0 ? 0 : score / scorePossible,
                        einschreibungen = slots.ToDictionary(s => s.ToString(),
                            s => _dbContext.ProfundaEinschreibungen.Include(e => e.ProfundumInstanz)
                                .ThenInclude(p => p.Profundum)
                                .Where(e => e.BetroffenePerson == p)
                                .Where(e => e.ProfundumInstanz.Slots.Contains(s))
                                .Select(e => e.ProfundumInstanz.Profundum.Bezeichnung).ToArray()
                        ),
                        Wuensche = slots.ToDictionary(s => s.ToString(),
                            s => _dbContext.ProfundaBelegWuensche
                                .Include(e => e.ProfundumInstanz).ThenInclude(p => p.Profundum)
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
                    .Where(e => e.ProfundumInstanz.Id == a.Id)
                    .Count() > 0)
                .ToDictionary(a => $"{a.Slots.First().ToString()} {a.Profundum.Bezeichnung} {a.Id}", a =>
                    new ProfundumMatchingStats
                    {
                        Einschreibungen = _dbContext.ProfundaEinschreibungen
                            .Include(e => e.ProfundumInstanz)
                            .Where(e => e.ProfundumInstanz.Id == a.Id)
                            .Count(),
                        MaxEinschreibungen = a.MaxEinschreibungen
                    }),
            NotMatchedStudents = students.Where(p => solver.Value(PersonNotEnrolledVariables[p]) > 0)
                .Select(p => $"{p.Gruppe}: {p.Vorname} {p.Nachname}").ToList()
        };
    }

    ///
    public async Task<IEnumerable<PersonInfoMinimal>> GetMissingStudentsAsync(ICollection<Guid> slotIds)
    {
        var slots = await _dbContext.ProfundaSlots.Where(s => slotIds.Contains(s.Id)).ToArrayAsync();
        return (await _dbContext.Personen
                .Include(p => p.ProfundaBelegwuensche).ThenInclude(bw => bw.ProfundumInstanz)
                .ThenInclude(pi => pi.Slots)
                .Where(p => p.Rolle == Rolle.Mittelstufe)
                .Where(p => !p.ProfundaBelegwuensche
                    .Where(bw => bw.ProfundumInstanz.Slots.Any(s => slotIds.Any(sl => sl == s.Id)))
                    .Where(bw => !bw.ProfundumInstanz.Slots.Any(s => !slotIds.Any(sl => sl == s.Id)))
                    .Any())
                .AsSplitQuery().ToArrayAsync())
            .Where(p => !isProfundumBlockiert(p, slots.Select(s => s.Quartal)))
            .Select(p => new PersonInfoMinimal(p));
    }

    ///
    public async Task<IEnumerable<string>> GetMissingStudentsEmailsAsync(ICollection<Guid> slotIds)
    {
        var slots = await _dbContext.ProfundaSlots.Where(s => slotIds.Contains(s.Id)).ToArrayAsync();
        return (await _dbContext.Personen
                .Include(p => p.ProfundaBelegwuensche).ThenInclude(bw => bw.ProfundumInstanz)
                .ThenInclude(pi => pi.Slots)
                .Where(p => p.Rolle == Rolle.Mittelstufe)
                .Where(p => !p.ProfundaBelegwuensche
                    .Where(bw => bw.ProfundumInstanz.Slots.Any(s => slotIds.Any(sl => sl == s.Id)))
                    .Where(bw => !bw.ProfundumInstanz.Slots.Any(s => !slotIds.Any(sl => sl == s.Id)))
                    .Any())
                .AsSplitQuery().ToArrayAsync())
            .Where(p => !isProfundumBlockiert(p, slots.Select(s => s.Quartal)))
            .Select(p => p.Email);
    }

    ///
    public async Task<Dictionary<string, DTOProfundumDefinition>> GetEnrollment(Person student,
        ICollection<Guid> slotIds)
    {
        return (await _dbContext.ProfundaSlots.Where(s => slotIds.Contains(s.Id)).ToArrayAsync()).ToDictionary(
            s => s.ToString(), s =>
                _dbContext.ProfundaEinschreibungen
                    .Include(pe => pe.ProfundumInstanz).ThenInclude(pi => pi.Profundum).ThenInclude(p => p.Kategorie)
                    .Where(pe => pe.BetroffenePerson.Id == student.Id)
                    .Where(p => p.ProfundumInstanz.Slots.Contains(s))
                    .Select(pe => new DTOProfundumDefinition
                    { Bezeichnung = pe.ProfundumInstanz.Profundum.Bezeichnung })
                    .First());
    }
}
