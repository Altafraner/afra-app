using Afra_App.Profundum.Domain.DTO;
using Afra_App.Profundum.Domain.Models;
using Afra_App.User.Services;
using Microsoft.EntityFrameworkCore;
using Person = Afra_App.User.Domain.Models.Person;
using Afra_App.Profundum.Configuration;

using Google.OrTools.Sat;
using LinearExpr = Google.OrTools.Sat.LinearExpr;
using Microsoft.Extensions.Options;

namespace Afra_App.Profundum.Services;

/// <summary>
///     A service for handling enrollments.
/// </summary>
public class EnrollmentService
{
    private readonly AfraAppContext _dbContext;
    private readonly ILogger _logger;
    private readonly UserService _userService;
    private readonly IOptions<ProfundumConfiguration> _profundumConfiguration;

    /// <summary>
    ///     Constructs the EnrollmentService. Usually called by the DI container.
    /// </summary>
    public EnrollmentService(AfraAppContext dbContext,
        ILogger<EnrollmentService> logger, UserService userService, IOptions<ProfundumConfiguration> profundumConfiguration)
    {
        _dbContext = dbContext;
        _logger = logger;
        _userService = userService;
        _profundumConfiguration = profundumConfiguration;
    }

    /// <summary>
    ///     Get all options for slots currently open for enrollment
    /// </summary>
    public ICollection<BlockKatalog> GetKatalog()
    {
        var katalog = new List<BlockKatalog>() { };
        var slots = _dbContext.ProfundaSlots.Where(s => s.EinwahlMöglich).OrderBy(s => (s.Jahr * 4 + (int)s.Quartal) * 7 + s.Wochentag).ToArray();

        foreach (var s in slots)
        {
            var profundumInstanzen = _dbContext.ProfundaInstanzen
                .Where(p => s.Id == p.Slots
                        .OrderBy(s => (s.Jahr * 4 + (int)s.Quartal) * 7 + s.Wochentag)
                        .First().Id
                );
            katalog.Add(new BlockKatalog
            {
                label = s.ToString(),
                id = s.ToString(),
                options = profundumInstanzen.Select(p => new BlockOption
                {
                    label = p.Profundum.Bezeichnung,
                    value = p.Id,
                    alsoIncludes = p.Slots.Where(x => x.Id != s.Id).Select(s => s.ToString()).ToArray(),
                }).ToArray(),
            });
        }

        return katalog;
    }

    private bool isProfundumPflichtig(Person student, IEnumerable<ProfundumQuartal> quartale)
    {
        var klasse = _userService.GetKlassenstufe(student);
        _logger.LogInformation("klasse {}", klasse);
        var profilQuartale = _profundumConfiguration.Value.ProfilPflichtigkeit.GetValueOrDefault(klasse);
        _logger.LogInformation("pflichtige Quartale: {}", System.Text.Json.JsonSerializer.Serialize(profilQuartale));
        _logger.LogInformation("relevante Quartale: {}", System.Text.Json.JsonSerializer.Serialize(quartale));
        if (profilQuartale is null)
        {
            return false;
        }
        var ret = profilQuartale.Intersect(quartale).Any();
        return ret;
    }

    /// <summary>
    ///     Register a set of Profundum Belegwuensche.
    ///     Validates that all currently open slots are filled
    /// </summary>
    /// <param name="student">The student wanting to enroll</param>
    /// <param name="wuensche">A dictionary containing the ordered ids of ProdundumInstanzen given the slot</param>
    public async Task<IResult> RegisterBelegWunschAsync(Person student, Dictionary<String, Guid[]> wuensche)
    {
        var slotsMöglich = _dbContext.ProfundaSlots.Where(s => s.EinwahlMöglich).OrderBy(s => (s.Jahr * 4 + (int)s.Quartal) * 7 + s.Wochentag).ToArray().AsReadOnly();

        var konflikte = _dbContext.ProfundaBelegWuensche
            .Include(bw => bw.ProfundumInstanz)
            .Where(p => p.ProfundumInstanz.Slots.Any(s => s.EinwahlMöglich))
            .Where(p => p.BetroffenePerson.Id == student.Id);
        if (konflikte.Any())
        {
            return Results.Conflict("Bereits abgegeben");
        }

        var ProfilProfundumEnthalten = false;

        foreach (var (str, l) in wuensche)
        {
            var s = slotsMöglich.Where(sm => sm.ToString() == str).FirstOrDefault();
            if (s is null)
            {
                return Results.BadRequest("Kein solcher Slot");
            }
            for (int i = 0; i < l.Length; ++i)
            {
                if (!BelegWunschStufe.IsDefined(typeof(BelegWunschStufe), i + 1))
                {
                    return Results.BadRequest("Belegwunschstufe nicht definiert");
                }

                var stufe = (BelegWunschStufe)(i + 1);
                var profundumInstanz = _dbContext.ProfundaInstanzen.Include(p => p.Profundum).Include(p => p.Slots).Where(p => p.Id == l[i]).First();

                if (profundumInstanz is null)
                {
                    return Results.BadRequest("ProfundumInstanz nicht gefunden");
                }

                if (profundumInstanz.Slots.Except(slotsMöglich).Any())
                {
                    return Results.BadRequest("ProfundumInstanz hat nicht einwählbare slots");

                }

                if (profundumInstanz.Slots.OrderBy(s => (s.Jahr * 4 + (int)s.Quartal) * 7 + s.Wochentag).First().Id == s.Id)
                {
                    var belegWunsch = new BelegWunsch() { ProfundumInstanz = profundumInstanz, Stufe = stufe, BetroffenePerson = student };
                    _dbContext.ProfundaBelegWuensche.Add(belegWunsch);

                    if (profundumInstanz.Profundum.ProfilProfundum)
                    {
                        ProfilProfundumEnthalten = true;
                    }
                }
                else if (!profundumInstanz.Slots.Where(sl => sl.Id == s.Id).Any())
                {
                    return Results.BadRequest("Profunduminstanz enthält nicht diesen Slot");
                }
            }
        }

        if (isProfundumPflichtig(student, slotsMöglich.Select(s => s.Quartal)) && !ProfilProfundumEnthalten)
        {
            return Results.BadRequest("Kein Profilprofundum in Einwahl enthalten");
        }

        await _dbContext.SaveChangesAsync();

        return Results.Ok("Einwahl gespeichert");
    }

    /// <summary>
    ///     Perform a matching for the given slots and return information about the result
    /// </summary>
    /// <param name="slotIds">The Ids of the slots to apply the matching to</param>
    public async Task<IResult> PerformMatching(ICollection<Guid> slotIds)
    {

        var slots = await _dbContext.ProfundaSlots
            .Where(s => slotIds.Contains(s.Id))
            .ToArrayAsync();

        var alteEinschreibungen = _dbContext.ProfundaEinschreibungen
            .Where(e => e.ProfundumInstanz.Slots.Any(s => slots.Contains(s)));
        _logger.LogInformation("delting {numEnrollments} old enrollments", alteEinschreibungen.Count());
        _dbContext.RemoveRange(alteEinschreibungen);
        await _dbContext.SaveChangesAsync();

        var angebote = await _dbContext.ProfundaInstanzen
            .Include(pi => pi.Slots)
            .Include(pi => pi.Profundum)
            .Where(pi => pi.Slots.Any(s => slotIds.Contains(s.Id)))
            .ToArrayAsync();
        var belegwünsche = await _dbContext.ProfundaBelegWuensche
            .Include(b => b.BetroffenePerson)
            .Include(b => b.ProfundumInstanz).ThenInclude(pi => pi.Profundum)
            .Where(b => angebote.Contains(b.ProfundumInstanz))
            .ToArrayAsync();
        var personen = belegwünsche.Select(b => b.BetroffenePerson).ToHashSet();

        var model = new CpModel();
        var modelWithoutLimits = new CpModel();
        var objective = LinearExpr.NewBuilder();
        var objectiveWithoutLimits = LinearExpr.NewBuilder();

        var weights = new Dictionary<BelegWunschStufe, int> {
            { BelegWunschStufe.ErstWunsch, 100 },
            { BelegWunschStufe.ZweitWunsch, 50 },
            { BelegWunschStufe.DrittWunsch, 25 },
        }.AsReadOnly();

        var belegVariables = new Dictionary<BelegWunsch, BoolVar>();
        var belegVariablesWithoutLimits = new Dictionary<BelegWunsch, BoolVar>();
        foreach (var wunsch in belegwünsche)
        {
            belegVariables[wunsch] = model.NewBoolVar($"beleg-{wunsch.BetroffenePerson.Id}-{wunsch.ProfundumInstanz.Id}");
            belegVariablesWithoutLimits[wunsch] = modelWithoutLimits.NewBoolVar($"beleg-{wunsch.BetroffenePerson.Id}-{wunsch.ProfundumInstanz.Id}");
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
                var psBelegVar = psBeleg.Select(b => belegVariables[b]).ToArray();
                var psBelegVarWithoutLimits = psBeleg.Select(b => belegVariablesWithoutLimits[b]).ToArray();
                model.AddExactlyOne(psBelegVar);
                modelWithoutLimits.AddExactlyOne(psBelegVarWithoutLimits);
                for (int i = 0; i < psBeleg.Length; ++i)
                {
                    objective.AddTerm(psBelegVar[i], weights[psBeleg[i].Stufe] * psBeleg[i].ProfundumInstanz.Slots.Count());
                    objectiveWithoutLimits.AddTerm(psBelegVarWithoutLimits[i], weights[psBeleg[i].Stufe] * psBeleg[i].ProfundumInstanz.Slots.Count());
                }
            }
        }

        // Mindestens ein Profilprofundum pro Semester für die hälfte der Schüler
        ProfundumQuartal[] QuartaleWintersemester = [ProfundumQuartal.Q1, ProfundumQuartal.Q2];
        var ProfilProfundumPflichtige = personen.Where(p => isProfundumPflichtig(p, slots.Select(s => s.Quartal)));

        foreach (var profundumPflichtigePerson in ProfilProfundumPflichtige)
        {
            var pBeleg = belegwünsche
                .Where(b => b.BetroffenePerson.Id == profundumPflichtigePerson.Id)
                .Where(b => b.ProfundumInstanz.Profundum.ProfilProfundum);
            model.AddAtLeastOne(pBeleg.Select(b => belegVariables[b]));
            modelWithoutLimits.AddAtLeastOne(pBeleg.Select(b => belegVariablesWithoutLimits[b]));
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

        model.Maximize(objective);
        modelWithoutLimits.Maximize(objectiveWithoutLimits);
        var solver = new CpSolver();
        var solverWithoutLimits = new CpSolver();
        var resultStatus = solver.Solve(model);
        var resultStatusWithoutLimits = solverWithoutLimits.Solve(modelWithoutLimits);

        if (resultStatus != CpSolverStatus.Optimal && resultStatus != CpSolverStatus.Feasible)
        {
            if (resultStatusWithoutLimits != CpSolverStatus.Optimal && resultStatusWithoutLimits != CpSolverStatus.Feasible)
            {
                return Results.Conflict($"No solution found in Matching likely due to errors in non-capacity constraints.");
            }
            else
            {
                return Results.Conflict($"No solution found in Matching due to capacity constraints");
            }
        }

        // Ergebnis rückschreiben
        foreach (var bw in belegwünsche)
        {
            var bw_var = belegVariables[bw];
            if (solver.Value(bw_var) > 0)
            {
                _dbContext.ProfundaEinschreibungen.Add(new Einschreibung()
                {
                    ProfundumInstanz = bw.ProfundumInstanz,
                    BetroffenePerson = bw.BetroffenePerson
                });
            }
        }

        await _dbContext.SaveChangesAsync();

        return Results.Ok(new
        {
            CalculationTime = solver.WallTime(),
            ResultStatus = resultStatus,
            ObjectiveValue = solver.ObjectiveValue,
            Students = personen.ToDictionary(
                    p => $"{p.Gruppe}: {p.Vorname} {p.Nachname}",
                    p =>
                    {
                        double score = belegwünsche
                            .Where(x => x.BetroffenePerson.Id == p.Id)
                            .Select(bw => solver.Value(belegVariables[bw]) * weights[bw.Stufe] * bw.ProfundumInstanz.Slots.Count()).Sum();
                        double scorePossible = belegwünsche
                            .Where(x => x.BetroffenePerson.Id == p.Id)
                            .Select(bw => solverWithoutLimits.Value(belegVariablesWithoutLimits[bw]) * weights[bw.Stufe] * bw.ProfundumInstanz.Slots.Count()).Sum();
                        return new
                        {
                            optim = score / scorePossible,
                            einschreibungen = slots.ToDictionary(s => s.ToString(),
                                s => _dbContext.ProfundaEinschreibungen.
                                Include(e => e.ProfundumInstanz).ThenInclude(p => p.Profundum)
                                .Where(e => e.BetroffenePerson == p)
                                .Where(e => e.ProfundumInstanz.Slots.Contains(s))
                                .Select(e => e.ProfundumInstanz.Profundum.Bezeichnung)
                                ),
                            Wuensche = slots.ToDictionary(s => s.ToString(),
                                s => _dbContext.ProfundaBelegWuensche
                                .Include(e => e.ProfundumInstanz).ThenInclude(p => p.Profundum)
                                .Where(e => e.BetroffenePerson == p)
                                .Where(e => e.ProfundumInstanz.Slots.Contains(s))
                                .ToArray()
                                .OrderBy(e => (int)e.Stufe)
                                .Select(e => e.ProfundumInstanz.Profundum.Bezeichnung)
                                )
                        };
                    }),
            Profunda = angebote
            .Where(a => _dbContext.ProfundaEinschreibungen
                            .Include(e => e.ProfundumInstanz)
                            .Where(e => e.ProfundumInstanz.Id == a.Id)
                            .Count() > 0)
            .ToDictionary(a => $"{a.Slots.First().ToString()} {a.Profundum.Bezeichnung}", a => new
            {
                Einschreibungen = _dbContext.ProfundaEinschreibungen
                            .Include(e => e.ProfundumInstanz)
                            .Where(e => e.ProfundumInstanz.Id == a.Id)
                            .Count(),
                MaxEinschreibungen = a.MaxEinschreibungen,
            }),
        });
    }
}
