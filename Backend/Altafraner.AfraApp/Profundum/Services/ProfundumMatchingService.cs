using System.Diagnostics;
using System.Text;
using Altafraner.AfraApp.Profundum.Configuration;
using Altafraner.AfraApp.Profundum.Domain.Contracts.Services;
using Altafraner.AfraApp.Profundum.Domain.DTO;
using Altafraner.AfraApp.Profundum.Domain.Models;
using Altafraner.AfraApp.User.Domain.DTO;
using Altafraner.AfraApp.User.Domain.Models;
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

    public ProfundumMatchingService(AfraAppContext dbContext,
        ILogger<ProfundumEnrollmentService> logger,
        IOptions<ProfundumConfiguration> profundumConfiguration,
        IRulesFactory rulesFactory)
    {
        _dbContext = dbContext;
        _logger = logger;
        _profundumConfiguration = profundumConfiguration;
        _rulesFactory = rulesFactory;
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

        var slots = _dbContext.ProfundaSlots.ToArray();
        var fixEinschreibungen = _dbContext.ProfundaEinschreibungen
            .Where(e => e.IsFixed).ToArray();
        var angebote = (await _dbContext.ProfundaInstanzen
                .Include(pi => pi.Slots).ThenInclude(s => s.EinwahlZeitraum)
                .Include(pi => pi.Profundum)
                .ToArrayAsync())
            .ToArray();
        var angeboteList = angebote.ToList();
        var belegwuensche = await _dbContext.ProfundaBelegWuensche
            .Include(b => b.BetroffenePerson)
            .Include(b => b.ProfundumInstanz).ThenInclude(b => b.Slots).ThenInclude(s => s.EinwahlZeitraum)
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

        var weights = new Dictionary<ProfundumBelegWunschStufe, int>
        {
            { ProfundumBelegWunschStufe.ErstWunsch, 128 },
            { ProfundumBelegWunschStufe.ZweitWunsch, 64 },
            { ProfundumBelegWunschStufe.DrittWunsch, 32 }
        }.AsReadOnly();
        var weightsVerschoben = new Dictionary<ProfundumBelegWunschStufe, int>
        {
            { ProfundumBelegWunschStufe.ErstWunsch, 16 },
            { ProfundumBelegWunschStufe.ZweitWunsch, 8 },
            { ProfundumBelegWunschStufe.DrittWunsch, 4 }
        }.AsReadOnly();

        // Set-Up Objective
        foreach (var currentSlot in slots)
        {
            var angeboteInSlot = angebote.Where(a => a.Slots.Contains(currentSlot)).ToArray();
            foreach (var currentStudent in students)
            {
                // Not enrolled var
                var nev = personNotEnrolledVariables[(currentStudent, currentSlot)];
                objective.AddTerm(nev, 1); // Not matched is slightly better than stupid solutions.

                var wuensche = belegwuensche.Where(b => b.BetroffenePerson == currentStudent).ToArray();
                var wuenscheInSlot = wuensche.Where(w => w.ProfundumInstanz.Slots.Contains(currentSlot)
                        && w.EinwahlZeitraum.Slots.Contains(currentSlot)).ToArray();

                // angebote vars
                foreach (var currentInstanzInSlot in angeboteInSlot)
                {
                    var currentVar = belegVars[(currentStudent, currentSlot, currentInstanzInSlot)];

                    // gewichtung
                    var wunsch = wuenscheInSlot.FirstOrDefault(w => w.ProfundumInstanz == currentInstanzInSlot);
                    if (wunsch is not null)
                    {
                        objective.AddTerm(currentVar, weights[wunsch.Stufe]);
                    }
                }

                // Wünsche from different slots
                var wunschVerschobenVars = belegVars
                    .Where(b => b.Key.p == currentStudent)
                    .Select(b => (
                            stufe: wuensche
                                .Where(w => w.ProfundumInstanz.Profundum == b.Key.i.Profundum
                                            && w.ProfundumInstanz != b.Key.i
                                            && w.EinwahlZeitraum.Slots.Contains(b.Key.s)
                                )
                                .Select(w => w.Stufe),
                            var: b.Value
                        )
                    )
                    .Where(x => x.stufe.Any())
                    .Select(x => (x.stufe.Max(), x.var));
                foreach (var (stufe, v) in wunschVerschobenVars)
                {
                    objective.AddTerm(v, weightsVerschoben[stufe]);
                }
            }
        }


        foreach (var student in students)
        {
            var sBelegWuensche = belegwuensche.Where(w => w.BetroffenePerson == student).ToArray();
            var sBelegVars = belegVars.Where(k => k.Key.p == student)
                .ToDictionary(x => (x.Key.s, x.Key.i), x => x.Value);
            var sNotEnrolledVars = personNotEnrolledVariables.Where(k => k.Key.p == student)
                .ToDictionary(x => x.Key.s, x => x.Value);

            foreach (var r in _rulesFactory.GetIndividualRules())
            {
                r.AddConstraints(student,
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
            r.AddConstraints(slots, students, belegwuensche, belegVars, model);

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
        };
    }

    ///
    public Task FinalizeMatching()
    {
        return _dbContext.ProfundaEinschreibungen
            .Where(e => e.ProfundumInstanz != null)
            .ExecuteUpdateAsync(e => e.SetProperty(ei => ei.IsFixed, true));
    }

    private IEnumerable<MatchingWarning> GetStudentWarnings(Person student,
        ProfundumSlot[] slots,
        ProfundumEinschreibung[] enrollments)
    {
        return _rulesFactory.GetIndividualRules().SelectMany(r => r.GetWarnings(student, slots, enrollments));
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
                .Select(e => new DTOWunsch(e.ProfundumInstanz.Id,
                    e.ProfundumInstanz.Slots.Select(s => s.Id),
                    (int)e.Stufe));
            var warnings = GetStudentWarnings(person,
                slots,
                person.ProfundaEinschreibungen
                    .Where(e => e.ProfundumInstanz is not null)
                    .ToArray());

            yield return new DTOProfundumEnrollmentSet
            {
                Person = new PersonInfoMinimal(person),
                Enrollments = personsEnrollments,
                Wuensche = personsWishes,
                Warnings = warnings
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
