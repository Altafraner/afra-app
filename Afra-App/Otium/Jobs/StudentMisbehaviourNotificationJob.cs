using System.Runtime.CompilerServices;
using System.Text;
using Afra_App.Backbone.Email.Services.Contracts;
using Afra_App.Backbone.Scheduler.Templates;
using Afra_App.Otium.Configuration;
using Afra_App.Otium.Domain.Models;
using Afra_App.Otium.Domain.Models.Schuljahr;
using Afra_App.Otium.Services;
using Afra_App.User.Domain.Models;
using Afra_App.User.Services;
using Microsoft.Extensions.Options;
using Quartz;

namespace Afra_App.Otium.Jobs;

/// <summary>
///     A job that notifies mentors about student misbehaviour.
/// </summary>
internal sealed class StudentMisbehaviourNotificationJob : RetryJob
{
    private readonly IAttendanceService _attendanceService;
    private readonly BlockHelper _blockHelper;
    private readonly AfraAppContext _dbContext;
    private readonly IEmailOutbox _emailOutbox;
    private readonly EnrollmentService _enrollmentService;
    private readonly ILogger<StudentMisbehaviourNotificationJob> _logger;
    private readonly IOptions<OtiumConfiguration> _otiumConfiguration;
    private readonly SchuljahrService _schuljahrService;
    private readonly UserService _userService;

    /// <summary>
    ///     Called from DI
    /// </summary>
    public StudentMisbehaviourNotificationJob(ILogger<StudentMisbehaviourNotificationJob> logger,
        IEmailOutbox emailOutbox, IAttendanceService attendanceService, EnrollmentService enrollmentService,
        SchuljahrService schuljahrService, UserService userService, BlockHelper blockHelper, AfraAppContext dbContext,
        IOptions<OtiumConfiguration> otiumConfiguration) : base(logger)
    {
        _logger = logger;
        _emailOutbox = emailOutbox;
        _attendanceService = attendanceService;
        _enrollmentService = enrollmentService;
        _schuljahrService = schuljahrService;
        _userService = userService;
        _blockHelper = blockHelper;
        _dbContext = dbContext;
        _otiumConfiguration = otiumConfiguration;
    }

    protected override int MaxRetryCount => 3;

    /// <exception cref="ArgumentOutOfRangeException"></exception>
    /// <inheritdoc />
    protected override async Task ExecuteAsync(IJobExecutionContext context, int _)
    {
        var now = DateTime.Now;
        var hasRun = context.JobDetail.JobDataMap.TryGetDateTime("last_run", out var lastRun);
        if (!hasRun && TimeOnly.FromDateTime(now) < _otiumConfiguration.Value.EnrollmentReminder.Time)
        {
            _logger.LogWarning(
                "Student Misbehaviour job was scheduled before the default reminder time. Skipping execution.");
            return;
        }

        if (hasRun && lastRun.Date == now.Date)
        {
            _logger.LogInformation("Student Misbehaviour job has already run today. Skipping execution.");
            return;
        }

        _logger.LogInformation("Running student misbehaviour job at {Time}", now);

        await DoWork(context);
        context.JobDetail.JobDataMap.Put("last_run", now);
        _logger.LogInformation("Student misbehaviour job completed successfully.");
    }

    private async Task DoWork(IJobExecutionContext context)
    {
        var today = DateOnly.FromDateTime(DateTime.Now);
        var blocks = await _schuljahrService.GetBlocksAsync(today);
        var lastDayWithBlocks = await _schuljahrService.GetLastDayWithBlocksAsync(today);

        if (blocks.Count == 0)
        {
            _logger.LogInformation("No blocks found for today ({Date})", today);
            return;
        }

        var blockProblems = new Dictionary<Person, List<BlockProblem>>();

        foreach (var block in blocks)
        {
            var attendanceForBlock = await _attendanceService.GetAttendanceForBlockAsync(block.Id);
            foreach (var missingPerson in attendanceForBlock.missingPersons.Keys)
            {
                if (!blockProblems.ContainsKey(missingPerson))
                    blockProblems[missingPerson] = [];

                blockProblems[missingPerson].Add(new BlockProblem(ProblemType.Unenrolled, block));
            }

            var missingButEnrolled = attendanceForBlock.termine
                .ToDictionary(t => t.Key,
                    t => t.Value.Where(e => e.Value == OtiumAnwesenheitsStatus.Fehlend));

            foreach (var (termin, anwesenheiten) in missingButEnrolled)
            foreach (var (person, _) in anwesenheiten)
                await _enrollmentService.UnenrollAsync(termin.Id, person, true, false);
            await _dbContext.SaveChangesAsync(context.CancellationToken);

            var missingStudentsInBlock = attendanceForBlock.missingPersons
                .Where(s => s.Value == OtiumAnwesenheitsStatus.Fehlend)
                .Concat(missingButEnrolled.SelectMany(t => t.Value))
                .DistinctBy(s => s.Key.Id)
                .Select(s => s.Key);

            foreach (var missingStudent in missingStudentsInBlock)
            {
                if (!blockProblems.ContainsKey(missingStudent)) blockProblems[missingStudent] = [];
                blockProblems[missingStudent].Add(new BlockProblem(ProblemType.Missing, block));
            }
        }

        var kategorieProblems = today == lastDayWithBlocks
            ? await _enrollmentService.GetStudentsWithMissingCategoriesInWeek(today.AddDays(-(int)today.DayOfWeek))
            : [];

        var studentsWithProblems = blockProblems.Keys
            .Concat(kategorieProblems.Keys)
            .DistinctBy(s => s.Id);

        foreach (var student in studentsWithProblems)
        {
            var subject = $"{student.Vorname} {student.Nachname}: Nicht erlaubtes Verhalten im Otium";
            var contentBuilder = new StringBuilder();
            contentBuilder.AppendLine(
                $"Die Afra-App hat ein Problem im Bezug auf ihren Mentee {student.Vorname} {student.Nachname} festgestellt:");
            contentBuilder.AppendLine();
            foreach (var problem in blockProblems.GetValueOrDefault(student, []))
            {
                var blockName = _blockHelper.Get(problem.Block.SchemaId)!.Bezeichnung;
                contentBuilder.AppendLine(problem.ProblemType switch
                {
                    ProblemType.Missing => $"- Fehlte im {blockName}",
                    ProblemType.Unenrolled => $"- War nicht für den {blockName} eingeschrieben",
                    _ => throw new SwitchExpressionException("Unbekannter Problemtyp: " + problem.ProblemType)
                });
            }

            foreach (var kategorie in kategorieProblems.GetValueOrDefault(student, []))
                contentBuilder.AppendLine($"- Fehlte in der verpflichtenden Kategorie {kategorie}");

            var body = contentBuilder.ToString();
            var mentoren = await _userService.GetMentorsAsync(student);

            foreach (var mentor in mentoren)
                await _emailOutbox.ScheduleNotificationAsync(mentor, subject, body, TimeSpan.FromMinutes(10));
        }
    }

    private record BlockProblem(ProblemType ProblemType, Block Block);

    private enum ProblemType
    {
        Missing,
        Unenrolled
    }
}
