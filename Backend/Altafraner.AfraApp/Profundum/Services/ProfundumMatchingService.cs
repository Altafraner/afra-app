using System.Diagnostics;
using Altafraner.AfraApp.Domain;
using Altafraner.AfraApp.Profundum.Configuration;
using Altafraner.AfraApp.Profundum.Domain.Contracts.Services;
using Altafraner.AfraApp.Profundum.Domain.DTO;
using Altafraner.AfraApp.Profundum.Domain.Models;
using Altafraner.AfraApp.Profundum.Jobs;
using Altafraner.AfraApp.User.Services;
using Altafraner.Backbone.EmailSchedulingModule;
using Google.OrTools.Sat;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Quartz;
using Models_Person = Altafraner.AfraApp.User.Domain.Models.Person;

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
    private readonly IScheduler _scheduler;

    ///
    public ProfundumMatchingService(AfraAppContext dbContext,
        ILogger<ProfundumEnrollmentService> logger,
        UserService userService,
        IOptions<ProfundumConfiguration> profundumConfiguration,
        INotificationService notificationService,
        IRulesFactory rulesFactory,
        ISchedulerFactory schedulerFactory
        )
    {
        _dbContext = dbContext;
        _logger = logger;
        _userService = userService;
        _profundumConfiguration = profundumConfiguration;
        _notificationService = notificationService;
        _rulesFactory = rulesFactory;
        _scheduler = schedulerFactory.GetScheduler().GetAwaiter().GetResult();
    }

    ///
    public async Task<Guid> StartMatching()
    {
        var matchingId = Guid.CreateVersion7();
        var jobDataMap = new JobDataMap
        {
            { "matchingid", matchingId },
        };
        var job = JobBuilder.Create<MatchingJob>()
            .WithIdentity($"matching-{matchingId}", "profundum-matching")
            .UsingJobData(jobDataMap)
            .Build();

        var trigger = TriggerBuilder.Create()
            .StartNow()
            .Build();

        await _scheduler.ScheduleJob(job, trigger);
        return matchingId;
    }

    ///
    public MatchingStats? QueryMatching(Guid matchingId)
    {
        // var job = _scheduler.GetJobDetail(new JobKey($"matching-{matchingId}", "profundum-matching"));
        // if (job.IsCompleted)
        // {
        var res = matchingResults.GetValueOrDefault(matchingId);
        Console.WriteLine("num of results " + matchingResults.Count());
        if (res is null)
        {
            throw new NotFoundException("job not yet finished.");
        }
        return res;
        // }
    }

    public void RegisterMatching(Guid id, MatchingStats stats)
    {
        matchingResults[id] = stats;
    }

    private static Dictionary<Guid, MatchingStats> matchingResults = new Dictionary<Guid, MatchingStats>();

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
            { ProfundumBelegWunschStufe.DrittWunsch, 25 }
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

        long notMatchedPenalty = slots.Count() * weights[ProfundumBelegWunschStufe.ErstWunsch] * students.Length;
        var personNotEnrolledVariables = new Dictionary<(Models_Person, ProfundumSlot), BoolVar>();
        var personNotEnrolledVariablesOnlyIndividualRules = new Dictionary<(Models_Person, ProfundumSlot), BoolVar>();
        foreach (var student in students)
        {
            foreach (var s in slots)
            {
                var nev = model.NewBoolVar($"beleg-{student.Id}-not-enrolled-in-{s.Id}");
                personNotEnrolledVariables[(student, s)] = nev;
                objective.AddTerm(nev, -notMatchedPenalty);
                var nevI = modelOnlyIndividualRules.NewBoolVar($"beleg-{student.Id}-not-enrolled");
                personNotEnrolledVariablesOnlyIndividualRules[(student, s)] = nevI;
                objectiveOnlyIndividualRules.AddTerm(nevI, -notMatchedPenalty);
            }
        }

        // Exact eine Einschreibung pro Slot und Person
        // Gewichtung nach Einwahlstufe
        foreach (var p in students)
        {
            foreach (var s in slots)
            {
                if (fixEinschreibungen.Any(e => e.BetroffenePersonId == p.Id && e.SlotId == s.Id))
                {
                    model.Add(personNotEnrolledVariables[(p, s)] == 1);
                }

                var psBeleg = belegwuensche
                    .Where(b => b.BetroffenePerson.Id == p.Id)
                    .Where(b => b.ProfundumInstanz.Slots.Contains(s))
                    .ToArray();
                var psBelegVar = psBeleg
                    .Select(b => belegVariables[b])
                    .Append(personNotEnrolledVariables[(p, s)])
                    .ToArray();
                var psBelegVarOnlyIndividualRules = psBeleg
                    .Select(b => belegVariablesOnlyIndividualRules[b])
                    .Append(personNotEnrolledVariablesOnlyIndividualRules[(p, s)])
                    .ToArray();
                model.AddExactlyOne(psBelegVar);
                modelOnlyIndividualRules.AddExactlyOne(psBelegVarOnlyIndividualRules);
                for (var i = 0; i < psBeleg.Length; ++i)
                {
                    objective.AddTerm(psBelegVar[i], weights[psBeleg[i].Stufe]);
                    objectiveOnlyIndividualRules.AddTerm(psBelegVarOnlyIndividualRules[i], weights[psBeleg[i].Stufe]);
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
                    belegVariables,
                    personNotEnrolledVariables.Where(k => k.Key.Item1.Id == s.Id).Select(s => s.Value).ToArray(),
                    model);
                r.AddConstraints(s,
                        slots,
                    sBelegWuensche,
                    belegVariablesOnlyIndividualRules,
                    personNotEnrolledVariablesOnlyIndividualRules.Where(k => k.Key.Item1.Id == s.Id).Select(s => s.Value).ToArray(),
                    modelOnlyIndividualRules);
            }

        foreach (var r in _rulesFactory.GetAggregateRules())
            r.AddConstraints(slots, students, belegwuensche, belegVariables, model);

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
                throw new ArgumentException(
                    "No solution found in Matching likely due to errors in non-capacity constraints.");

            throw new ArgumentException("No solution found in Matching due to capacity constraints");
        }


        var matchingResultStatus = (solver.ObjectiveValue, solverOnlyIndividualRules.ObjectiveValue) switch
        {
            ( >= 0, >= 0) => MatchingResultStatus.MatchingFound,
            ( < 0, >= 0) => MatchingResultStatus.MatchingIncompleteDueToCapacity,
            ( < 0, < 0) => MatchingResultStatus.MatchingIncompleteDueToHardConstraints,
            _ => throw new UnreachableException()
        };


        // if (matchingResultStatus == MatchingResultStatus.MatchingFound)
        // {
        // Ergebnis rückschreiben
        foreach (var bw in belegwuensche)
        {
            var bwVar = belegVariables[bw];
            if (solver.Value(bwVar) > 0)
            {
                foreach (var s in bw.ProfundumInstanz.Slots)
                {
                    _dbContext.ProfundaEinschreibungen.Add(new ProfundumEinschreibung
                    {
                        ProfundumInstanz = bw.ProfundumInstanz,
                        BetroffenePerson = bw.BetroffenePerson,
                        Slot = s,
                    });
                }
            }
        }

        await _dbContext.SaveChangesAsync();
        // }

        return new MatchingStats
        {
            CalculationTime = solver.WallTime(),
            Result = matchingResultStatus,
            ObjectiveValue = solver.ObjectiveValue,
            ObjectiveValueNoLimits = solverOnlyIndividualRules.ObjectiveValue,
            Optim = solverOnlyIndividualRules.ObjectiveValue == 0
                ? 0
                : solver.ObjectiveValue / solverOnlyIndividualRules.ObjectiveValue,
        };
    }

    ///
    public Task Finalize()
    {
        return _dbContext.ProfundaEinschreibungen.ExecuteUpdateAsync(e => e.SetProperty(e => e.IsFixed, true));
    }
}
