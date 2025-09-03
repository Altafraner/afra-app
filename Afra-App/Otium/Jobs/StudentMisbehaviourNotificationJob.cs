using System.Text;
using Afra_App.Backbone.Email.Services.Contracts;
using Afra_App.Backbone.Scheduler.Templates;
using Afra_App.Backbone.Utilities;
using Afra_App.Otium.Configuration;
using Afra_App.Otium.Domain.Models;
using Afra_App.Otium.Services;
using Afra_App.Schuljahr.Domain.Models;
using Afra_App.Schuljahr.Services;
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
        SchuljahrService schuljahrService, UserService userService, BlockHelper blockHelper,
        IOptions<OtiumConfiguration> otiumConfiguration) : base(logger)
    {
        _logger = logger;
        _emailOutbox = emailOutbox;
        _attendanceService = attendanceService;
        _enrollmentService = enrollmentService;
        _schuljahrService = schuljahrService;
        _userService = userService;
        _blockHelper = blockHelper;
        _otiumConfiguration = otiumConfiguration;
    }

    protected override int MaxRetryCount => 3;

    /// <exception cref="ArgumentOutOfRangeException"></exception>
    /// <inheritdoc />
    protected override async Task ExecuteAsync(IJobExecutionContext context, int _)
    {
        if (!_otiumConfiguration.Value.StudentMisbehaviourNotification.Enabled) return;

        var now = DateTime.Now;
        var hasRun = context.JobDetail.JobDataMap.TryGetDateTime("last_run", out var lastRun);
        if (TimeOnly.FromDateTime(now) < _otiumConfiguration.Value.StudentMisbehaviourNotification.Time.AddMinutes(-5))
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

        await DoWork();
        context.JobDetail.JobDataMap.Put("last_run", now);
        _logger.LogInformation("Student misbehaviour job completed successfully.");
    }

    private async Task DoWork()
    {
        var today = DateOnly.FromDateTime(DateTime.Now);
        var blocks = await _schuljahrService.GetBlocksAsync(today);
        var lastDayWithBlocks = await _schuljahrService.GetLastDayWithBlocksAsync(today);

        if (blocks.Count == 0)
        {
            _logger.LogInformation("No blocks found for today ({Date})", today);
            return;
        }

        var unenrolledProblems = new Dictionary<Person, List<Block>>();
        var missingInTerminProblems = new Dictionary<Person, List<(OtiumTermin termin, Block block)>>();
        var missingInBlockProblems = new Dictionary<Person, List<Block>>();

        foreach (var block in blocks)
        {
            var (termineInBlock, unenrolledForBlock, _) = await _attendanceService.GetAttendanceForBlockAsync(block.Id);
            foreach (var (missingPerson, status) in unenrolledForBlock)
            {
                if (status == OtiumAnwesenheitsStatus.Entschuldigt) continue;

                if (!unenrolledProblems.ContainsKey(missingPerson))
                    unenrolledProblems[missingPerson] = [];
                unenrolledProblems[missingPerson].Add(block);

                if (status != OtiumAnwesenheitsStatus.Fehlend) continue;
                if (!missingInBlockProblems.ContainsKey(missingPerson))
                    missingInBlockProblems[missingPerson] = [];
                missingInBlockProblems[missingPerson].Add(block);
            }

            foreach (var (termin, anwesenheiten) in termineInBlock)
            foreach (var (person, _) in anwesenheiten.Where(a => a.Value == OtiumAnwesenheitsStatus.Fehlend))
            {
                if (!missingInTerminProblems.ContainsKey(person)) missingInTerminProblems[person] = [];
                missingInTerminProblems[person].Add((termin, block));
            }
        }

        var kategorieProblems = today == lastDayWithBlocks
            ? await _enrollmentService.GetStudentsWithMissingCategoriesInWeek(today.GetStartOfWeek())
            : [];

        var studentsWithProblems = unenrolledProblems.Keys
            .Concat(missingInTerminProblems.Keys)
            .Concat(kategorieProblems.Keys)
            .DistinctBy(s => s.Id);

        foreach (var student in studentsWithProblems)
        {
            if (student.Rolle != Rolle.Mittelstufe) continue;
            var subject = $"{student.Vorname} {student.Nachname}: Information zum Otium";
            var contentBuilder = new StringBuilder();
            foreach (var block in unenrolledProblems.GetValueOrDefault(student, []))
            {
                var blockName = _blockHelper.Get(block.SchemaId)!.Bezeichnung;
                contentBuilder.AppendLine($"- War nicht für den {blockName} eingeschrieben");
            }

            foreach (var block in missingInBlockProblems.GetValueOrDefault(student, []))
            {
                var blockName = _blockHelper.Get(block.SchemaId)!.Bezeichnung;
                contentBuilder.AppendLine($"- Fehlte unentschuldigt im {blockName}");
            }

            foreach (var termin in missingInTerminProblems.GetValueOrDefault(student, []))
            {
                var blockName = _blockHelper.Get(termin.block.SchemaId)!.Bezeichnung;
                contentBuilder.AppendLine(
                    $"- Fehlte unentschuldigt im Angebot „{termin.termin.Otium.Bezeichnung}“ im {blockName}");
            }

            foreach (var kategorie in kategorieProblems.GetValueOrDefault(student, []))
                contentBuilder.AppendLine($"- War diese Woche zu keinem Angebot der Kategorie „{kategorie}“");

            var body = contentBuilder.ToString();
            var mentoren = await _userService.GetMentorsAsync(student);

            foreach (var mentor in mentoren)
                await _emailOutbox.ScheduleNotificationAsync(mentor, subject, body, TimeSpan.FromMinutes(10));
        }
    }
}
