using System.Diagnostics;
using System.Text;
using Altafraner.AfraApp.Profundum.Configuration;
using Altafraner.AfraApp.Profundum.Domain.DTO;
using Altafraner.AfraApp.Profundum.Domain.Models;
using Altafraner.AfraApp.User.Domain.DTO;
using Altafraner.AfraApp.User.Domain.Models;
using Altafraner.AfraApp.User.Services;
using Altafraner.Backbone.EmailSchedulingModule.Contracts;
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

    /// <summary>
    ///     Constructs the EnrollmentService. Usually called by the DI container.
    /// </summary>
    public ProfundumEnrollmentService(AfraAppContext dbContext,
        ILogger<ProfundumEnrollmentService> logger, UserService userService,
        IOptions<ProfundumConfiguration> profundumConfiguration,
        INotificationService notificationService)
    {
        _dbContext = dbContext;
        _logger = logger;
        _userService = userService;
        _profundumConfiguration = profundumConfiguration;
        _notificationService = notificationService;
    }

    ///
    public bool isProfundumBlockiert(Models_Person student, IEnumerable<ProfundumQuartal> quartale)
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
    public bool isProfilPflichtig(Models_Person student, IEnumerable<ProfundumQuartal> quartale)
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
    public bool isProfilZulässig(Models_Person student, IEnumerable<ProfundumQuartal> quartale)
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
    public ICollection<BlockKatalog>? GetKatalog(Models_Person student)
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
    public async Task RegisterBelegWunschAsync(Models_Person student, Dictionary<String, Guid[]> wuensche)
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

        if (isProfilPflichtig(student, slots.Select(s => s.Quartal)) &&
            !belegWuensche.Any(w => w.ProfundumInstanz.Profundum.Kategorie.ProfilProfundum))
        {
            throw new ProfundumEinwahlWunschException("Profilprofundum nicht in auswahl enthalten");
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

            foreach (var b in belegWuensche.Where(b => b.ProfundumInstanz.Slots.Contains(slot)))
            {
                wuenscheString.AppendLine($"    {(int)b.Stufe}. {b.ProfundumInstanz.Profundum.Bezeichnung}");
            }
        }

        await _notificationService.ScheduleNotificationAsync(student, "Deine Profunda Einwahl-Wünsche",
            wuenscheString.ToString(), TimeSpan.Zero);

        _dbContext.ProfundaBelegWuensche.AddRange(belegWuensche);
        await _dbContext.SaveChangesAsync();
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
        var personen = belegwünsche.Select(b => b.BetroffenePerson).ToHashSet().ToArray();

        if (!_profundumConfiguration.Value.deterministicMatching)
        {
            Random.Shared.Shuffle(angebote);
            Random.Shared.Shuffle(belegwünsche);
            Random.Shared.Shuffle(personen);
        }

        var model = new CpModel();
        var modelWithoutLimits = new CpModel();
        var objective = LinearExpr.NewBuilder();
        var objectiveWithoutLimits = LinearExpr.NewBuilder();

        var weights = new Dictionary<ProfundumBelegWunschStufe, int>
        {
            { ProfundumBelegWunschStufe.ErstWunsch, 100 },
            { ProfundumBelegWunschStufe.ZweitWunsch, 50 },
            { ProfundumBelegWunschStufe.DrittWunsch, 25 },
        }.AsReadOnly();

        var belegVariables = new Dictionary<ProfundumBelegWunsch, BoolVar>();
        var belegVariablesWithoutLimits = new Dictionary<ProfundumBelegWunsch, BoolVar>();
        foreach (var wunsch in belegwünsche)
        {
            belegVariables[wunsch] =
                model.NewBoolVar($"beleg-{wunsch.BetroffenePerson.Id}-{wunsch.ProfundumInstanz.Id}");
            belegVariablesWithoutLimits[wunsch] =
                modelWithoutLimits.NewBoolVar($"beleg-{wunsch.BetroffenePerson.Id}-{wunsch.ProfundumInstanz.Id}");
        }

        long notMatchedPenalty = einwahlZeitraum.Slots.Count() * weights[ProfundumBelegWunschStufe.ErstWunsch] *
                                 personen.Count();
        var PersonNotEnrolledVariables = new Dictionary<Models_Person, BoolVar>();
        var PersonNotEnrolledVariablesWithoutLimits = new Dictionary<Models_Person, BoolVar>();
        foreach (var student in personen)
        {
            PersonNotEnrolledVariables[student] = model.NewBoolVar($"beleg-{student.Id}-not-enrolled");
            objective.AddTerm(PersonNotEnrolledVariables[student], -notMatchedPenalty);
            PersonNotEnrolledVariablesWithoutLimits[student] =
                modelWithoutLimits.NewBoolVar($"beleg-{student.Id}-not-enrolled");
            objectiveWithoutLimits.AddTerm(PersonNotEnrolledVariablesWithoutLimits[student], -notMatchedPenalty);
        }

        // Exact eine Einschreibung pro Slot und Person
        // Gewichtung nach Einwahlstufe
        foreach (var s in slots)
        {
            foreach (var p in personen)
            {
                var psBeleg = belegwünsche
                    .Where(b => b.BetroffenePerson.Id == p.Id)
                    .Where(b => b.ProfundumInstanz.Slots.Contains(s)).ToArray();
                var psBelegVar = psBeleg.Select(b => belegVariables[b]).Append(PersonNotEnrolledVariables[p]).ToArray();
                var psBelegVarWithoutLimits = psBeleg.Select(b => belegVariablesWithoutLimits[b])
                    .Append(PersonNotEnrolledVariablesWithoutLimits[p]).ToArray();
                model.AddExactlyOne(psBelegVar);
                modelWithoutLimits.AddExactlyOne(psBelegVarWithoutLimits);
                for (int i = 0; i < psBeleg.Length; ++i)
                {
                    objective.AddTerm(psBelegVar[i], weights[psBeleg[i].Stufe]);
                    objectiveWithoutLimits.AddTerm(psBelegVarWithoutLimits[i], weights[psBeleg[i].Stufe]);
                }
            }
        }

        // Mindestens ein Profilprofundum pro Semester für die hälfte der Schüler
        var ProfilProfundumPflichtige = personen.Where(p => isProfilPflichtig(p, slots.Select(s => s.Quartal)));

        foreach (var profundumPflichtigePerson in ProfilProfundumPflichtige)
        {
            var pBeleg = belegwünsche
                .Where(b => b.BetroffenePerson.Id == profundumPflichtigePerson.Id)
                .Where(b => b.ProfundumInstanz.Profundum.Kategorie.ProfilProfundum);
            model.AddAtLeastOne(pBeleg.Select(b => belegVariables[b])
                .Append(PersonNotEnrolledVariables[profundumPflichtigePerson]));
            modelWithoutLimits.AddAtLeastOne(pBeleg.Select(b => belegVariablesWithoutLimits[b])
                .Append(PersonNotEnrolledVariablesWithoutLimits[profundumPflichtigePerson]));
        }

        // Maximal MaxEinschreibungen Einschreibungen pro ProfundumInstanz
        foreach (var p in angebote)
        {
            var beleg = belegwünsche.Where(b => b.ProfundumInstanz == p).ToArray();
            var beleg_vars = beleg.Select(b => belegVariables[b]);
            if (p.MaxEinschreibungen is int max)
            {
                model.Add(LinearExpr.Sum(beleg_vars) <= max);
            }
        }

        // Maximal eine Instanz eines Profundums pro Schüler
        foreach (var p in personen)
        {
            var profundaDefinitionenIds = belegwünsche
                .Where(b => b.BetroffenePerson.Id == p.Id)
                .Select(b => b.ProfundumInstanz.Profundum.Id)
                .ToHashSet();

            foreach (var defId in profundaDefinitionenIds)
            {
                var psBeleg = belegwünsche
                    .Where(b => b.BetroffenePerson.Id == p.Id)
                    .Where(b => b.ProfundumInstanz.Profundum.Id == defId);
                var psBelegVar = psBeleg.Select(b => belegVariables[b]).ToArray();
                var psBelegVarWithoutLimits = psBeleg.Select(b => belegVariablesWithoutLimits[b]).ToArray();

                model.AddAtMostOne(psBelegVar);
                modelWithoutLimits.AddAtMostOne(psBelegVarWithoutLimits);
            }
        }

        model.Maximize(objective);
        modelWithoutLimits.Maximize(objectiveWithoutLimits);
        var solver = new CpSolver();
        var solverWithoutLimits = new CpSolver();
        var resultStatus = solver.Solve(model);
        var resultStatusWithoutLimits = solverWithoutLimits.Solve(modelWithoutLimits);

        if (resultStatus != CpSolverStatus.Optimal && resultStatus != CpSolverStatus.Feasible)
        {
            if (resultStatusWithoutLimits != CpSolverStatus.Optimal &&
                resultStatusWithoutLimits != CpSolverStatus.Feasible)
            {
                throw new ArgumentException(
                    "No solution found in Matching likely due to errors in non-capacity constraints.");
            }
            else
            {
                throw new ArgumentException("No solution found in Matching due to capacity constraints");
            }
        }


        var matchingResultStatus = (solver.ObjectiveValue, solverWithoutLimits.ObjectiveValue) switch
        {
            (>= 0, >= 0) => MatchingResultStatus.MatchingFound,
            (< 0, >= 0) => MatchingResultStatus.MatchingIncompleteDueToCapacity,
            (< 0, < 0) => MatchingResultStatus.MatchingIncompleteDueToHardConstraints,
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
            ObjectiveValueNoLimits = solverWithoutLimits.ObjectiveValue,
            Optim = solverWithoutLimits.ObjectiveValue == 0
                ? 0
                : solver.ObjectiveValue / solverWithoutLimits.ObjectiveValue,
            Students = personen.ToDictionary(
                p => $"{p.Gruppe}: {p.FirstName} {p.LastName}",
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
                            solverWithoutLimits.Value(belegVariablesWithoutLimits[bw]) * weights[bw.Stufe] *
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
            NotMatchedStudents = personen.Where(p => solver.Value(PersonNotEnrolledVariables[p]) > 0)
                .Select(p => $"{p.Gruppe}: {p.FirstName} {p.LastName}").ToList()
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
    public async Task<Dictionary<string, DTOProfundumDefinition>> GetEnrollment(Models_Person student,
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
