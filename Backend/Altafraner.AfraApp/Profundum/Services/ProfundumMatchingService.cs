using System.Diagnostics;
using Altafraner.AfraApp.Profundum.Configuration;
using Altafraner.AfraApp.Profundum.Domain.Contracts.Services;
using Altafraner.AfraApp.Profundum.Domain.DTO;
using Altafraner.AfraApp.Profundum.Domain.Models;
using Altafraner.AfraApp.User.Domain.Models;
using Altafraner.AfraApp.User.Services;
using Altafraner.Backbone.EmailSchedulingModule;
using Google.OrTools.Sat;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Altafraner.AfraApp.Profundum.Services;

///
internal class ProfundumMatchingService
{
    private readonly AfraAppContext _dbContext;
    private readonly ILogger _logger;
    private readonly INotificationService _notificationService;
    private readonly IOptions<ProfundumConfiguration> _profundumConfiguration;
    private readonly IRulesFactory _rulesFactory;
    private readonly UserService _userService;

    ///
    public ProfundumMatchingService(AfraAppContext dbContext,
        ILogger<ProfundumEnrollmentService> logger,
        UserService userService,
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


    /// <summary>
    ///     Perform a matching for the given slots and return information about the result
    /// </summary>
    public async Task<MatchingStats> PerformMatching()
    {
        var slots = _dbContext.ProfundaSlots.ToArray();

        var automatischeEinschreibungen = _dbContext.ProfundaEinschreibungen
            .Where(e => !e.IsFixed);
        _logger.LogInformation("delting {numEnrollments} old enrollments", automatischeEinschreibungen.Count());
        _dbContext.RemoveRange(automatischeEinschreibungen);
        await _dbContext.SaveChangesAsync();

        var fixEinschreibungen = _dbContext.ProfundaEinschreibungen
            .Where(e => e.IsFixed).ToArray();

        var angebote = (await _dbContext.ProfundaInstanzen
                .Include(pi => pi.Slots)
                .Include(pi => pi.Profundum)
                .ToArrayAsync())
            .Where(pi => pi.Slots.Any(s => slots.Any(sl => sl.Id == s.Id)))
            .ToArray();
        var angeboteList = angebote.ToList();
        var belegwuensche = await _dbContext.ProfundaBelegWuensche
            .Include(b => b.BetroffenePerson)
            .Include(b => b.ProfundumInstanz).ThenInclude(b => b.Slots)
            .Include(b => b.ProfundumInstanz).ThenInclude(pi => pi.Profundum).ThenInclude(p => p.Kategorie)
            .Where(b => angeboteList.Contains(b.ProfundumInstanz))
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

        var weights = new Dictionary<ProfundumBelegWunschStufe, int>
        {
            { ProfundumBelegWunschStufe.ErstWunsch, 100 },
            { ProfundumBelegWunschStufe.ZweitWunsch, 50 },
            { ProfundumBelegWunschStufe.DrittWunsch, 25 }
        }.AsReadOnly();

        var belegVars = new Dictionary<(Person, ProfundumSlot, ProfundumInstanz), BoolVar>();


        long notMatchedPenalty = 1000;
        var personNotEnrolledVariables = new Dictionary<(Person, ProfundumSlot), BoolVar>();
        foreach (var student in students)
        {
            foreach (var s in slots)
            {
                var nev = model.NewBoolVar($"beleg-{student.Id}-not-enrolled-in-{s.Id}");
                personNotEnrolledVariables[(student, s)] = nev;
                objective.AddTerm(nev, -notMatchedPenalty);
            }
        }

        foreach (var p in students)
        {
            foreach (var s in slots)
            {
                var psVars = new List<BoolVar>() { personNotEnrolledVariables[(p, s)] };

                foreach (var i in angebote.Where(a => a.Slots.Contains(s)))
                {
                    belegVars[(p, s, i)] =
                        model.NewBoolVar($"beleg-{p.Id}-{s.Id}-{i.Id}");
                    psVars.Add(belegVars[(p, s, i)]);
                }

                model.AddExactlyOne(psVars);
            }
        }

        // Gewichtung nach Einwahlstufe
        foreach (var p in students)
        {
            foreach (var s in slots)
            {
                var fixE = fixEinschreibungen.Where(e => e.BetroffenePersonId == p.Id && e.SlotId == s.Id);
                if (fixE.Any())
                {
                    var e = fixE.First();
                    if (e.ProfundumInstanz is null)
                    {
                        model.Add(personNotEnrolledVariables[(p, s)] == 1);
                    }
                    else
                    {
                        model.Add(belegVars[(p, s, e.ProfundumInstanz!)] == 1);
                    }
                }

                var psBeleg = belegwuensche
                    .Where(b => b.BetroffenePerson.Id == p.Id)
                    .Where(b => b.ProfundumInstanz.Slots.Contains(s))
                    .ToArray();
                var psBelegVar = psBeleg
                    .SelectMany(b => b.ProfundumInstanz.Slots.Select(s => (b.Stufe, belegVars[(b.BetroffenePerson, s, b.ProfundumInstanz)])))
                    .ToArray();
                foreach (var (stufe, v) in psBelegVar)
                {
                    objective.AddTerm(v, weights[stufe]);
                }
            }
        }


        foreach (var r in _rulesFactory.GetIndividualRules())
            foreach (var s in students)
            {
                var sBelegWuensche = belegwuensche.Where(w => w.BetroffenePerson.Id == s.Id).ToArray();
                r.AddConstraints(s,
                        slots,
                    sBelegWuensche,
                    belegVars.Where(k => k.Key.Item1 == s).ToDictionary(x => (x.Key.Item2, x.Key.Item3), x => x.Value),
                    personNotEnrolledVariables.Where(k => k.Key.Item1 == s).ToDictionary(x => x.Key.Item2, x => x.Value),
                    model,
                    objective
                    );
            }

        foreach (var r in _rulesFactory.GetAggregateRules())
            r.AddConstraints(slots, students, belegwuensche, belegVars, model);

        model.Maximize(objective);

        _logger.LogInformation($"Model stats: {model.ModelStats()}");

        var solver = new CpSolver();
        solver.StringParameters = "max_time_in_seconds:30.0";
        var resultStatus = solver.Solve(model, new SolutionCallBack());

        if (resultStatus != CpSolverStatus.Optimal && resultStatus != CpSolverStatus.Feasible)
        {
            throw new ArgumentException("No solution found in Matching.");
        }

        var newEinschreibungen = new List<ProfundumEinschreibung>();
        foreach (var p in students)
        {
            foreach (var i in angebote)
            {
                foreach (var s in i.Slots)
                {
                    if (fixEinschreibungen.Any(e => e.BetroffenePersonId == p.Id && e.SlotId == s.Id))
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
            }
        }
        await _dbContext.ProfundaEinschreibungen.AddRangeAsync(newEinschreibungen);
        await _dbContext.SaveChangesAsync();

        return new MatchingStats
        {
            CalculationTime = solver.WallTime(),
            Result = MatchingResultStatus.MatchingComplete,
        };
    }

    ///
    public Task Finalize()
    {
        return _dbContext.ProfundaEinschreibungen.ExecuteUpdateAsync(e => e.SetProperty(e => e.IsFixed, true));
    }

    ///
    internal IEnumerable<string> GetStudentWarnings(Person student,
        ProfundumSlot[] slots,
        ProfundumEinschreibung[] enrollments)
    {
        var warnings = new List<string>();

        foreach (var r in _rulesFactory.GetIndividualRules())
        {
            warnings.AddRange(r.GetWarnings(student, slots, enrollments));
        }

        return warnings;
    }

    private bool IsProfilPflichtig(Person student, ProfundumQuartal quartal)
    {
        var klasse = _userService.GetKlassenstufe(student);
        var profilQuartale = _profundumConfiguration.Value.ProfilPflichtigkeit.GetValueOrDefault(klasse);
        return profilQuartale is not null && profilQuartale.Contains(quartal);
    }

    public async IAsyncEnumerable<DTOProfundumEnrollmentSet> GetAllEnrollmentsAsync()
    {
        var slots = await _dbContext.ProfundaSlots.ToArrayAsync();

        var personenWithData = _dbContext.Personen
            .AsSplitQuery()
            .Where(p => p.Rolle == Rolle.Mittelstufe)
            .OrderBy(p => p.Gruppe)
            .ThenBy(p => p.LastName)
            .ThenBy(p => p.FirstName)
            .Include(p => p.ProfundaBelegwuensche)
            .ThenInclude(p => p.ProfundumInstanz)
            .ThenInclude(p => p.Profundum)
            .Include(p => p.ProfundaBelegwuensche)
            .ThenInclude(p => p.ProfundumInstanz)
            .ThenInclude(p => p.Slots)
            .Include(p => p.ProfundaEinschreibungen)
            .ThenInclude(p => p.ProfundumInstanz)
            .ThenInclude(p => p!.Profundum)
            .ThenInclude(p => p.Kategorie)
            .Include(p => p.ProfundaEinschreibungen)
            .ThenInclude(p => p.ProfundumInstanz)
            .ThenInclude(p => p!.Slots)
            .AsAsyncEnumerable();


        await foreach (var person in personenWithData)
        {
            var personsEnrollments = slots.Select(slot => (slotId: slot.Id,
                    enrollment: person.ProfundaEinschreibungen.FirstOrDefault(e => e.SlotId == slot.Id)))
                .Select(e =>
                    e.enrollment is not null
                        ? new DTOProfundumEnrollment(e.enrollment)
                        : new DTOProfundumEnrollment
                        { ProfundumSlotId = e.slotId, ProfundumInstanzId = null, IsFixed = false });

            var personsWishes = person.ProfundaBelegwuensche
                .Select(e => new DTOWunsch(e.ProfundumInstanz.Profundum.Id,
                    e.ProfundumInstanz.Slots.Select(s => s.Id),
                    (int)e.Stufe));
            var warnings = GetStudentWarnings(person,
                slots,
                person.ProfundaEinschreibungen
                    .Where(e => e.ProfundumInstanz is not null)
                    .ToArray());

            yield return new DTOProfundumEnrollmentSet
            {
                Person = new User.Domain.DTO.PersonInfoMinimal(person),
                Enrollments = personsEnrollments,
                Wuensche = personsWishes,
                Warnings = warnings
            };
        }
    }
}


class SolutionCallBack : CpSolverSolutionCallback
{
    private int solution_count;
    public override void OnSolutionCallback()
    {
        Console.WriteLine(String.Format("Solution #{0}: time = {1:F2} s", solution_count, WallTime()));
        Console.WriteLine(String.Format("  objective value = {0}", ObjectiveValue()));
        solution_count++;
    }

}
