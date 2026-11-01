using Altafraner.AfraApp.Otium.Configuration;
using Altafraner.AfraApp.Otium.Services;
using Altafraner.AfraApp.Schuljahr.Domain.Models;
using Altafraner.AfraApp.Schuljahr.Services;
using Altafraner.AfraApp.User.Domain.Models;
using Altafraner.Backbone.EmailSchedulingModule;
using Microsoft.Extensions.Options;
using NodaTime;
using NodaTime.Extensions;
using Quartz;

namespace Altafraner.AfraApp.Otium.Jobs;

/// <summary>
/// A background job that sends reminders to users about their missing enrollments in Otium events.
/// </summary>
internal sealed class EnrollmentReminderJob : IJob
{
    private readonly INotificationService _notificationService;
    private readonly EnrollmentService _enrollmentService;
    private readonly ILogger<EnrollmentReminderJob> _logger;
    private readonly IOptions<OtiumConfiguration> _otiumConfiguration;
    private readonly BlockHelper _blockHelper;
    private readonly SchuljahrService _schuljahrService;

    /// <summary>
    /// Constructor for the EnrollmentReminderJob. Called by the DI container.
    /// </summary>
    public EnrollmentReminderJob(ILogger<EnrollmentReminderJob> logger, EnrollmentService enrollmentService,
        IOptions<OtiumConfiguration> otiumConfiguration,
        INotificationService notificationService,
        BlockHelper blockHelper,
        SchuljahrService schuljahrService)
    {
        _logger = logger;
        _enrollmentService = enrollmentService;
        _otiumConfiguration = otiumConfiguration;
        _notificationService = notificationService;
        _blockHelper = blockHelper;
        _schuljahrService = schuljahrService;
    }

    private static readonly Duration ReminderLeadTime = Duration.FromHours(12);

    public async ValueTask Execute(IJobExecutionContext context,
        CancellationToken cancellationToken)
    {
        if (!_otiumConfiguration.Value.EnrollmentReminder.Enabled) return;

        var now = DateTime.Now;
        var hasRun = context.JobDetail.JobDataMap.TryGet("last_run", out DateTime lastRun);
        if (TimeOnly.FromDateTime(now) < _otiumConfiguration.Value.EnrollmentReminder.Time.AddMinutes(-5))
        {
            _logger.LogWarning(
                "Enrollment reminder job was scheduled before the default reminder time. Skipping execution.");
            return;
        }

        if (hasRun && lastRun.Date == now.Date)
        {
            _logger.LogInformation("Enrollment reminder job has already run today. Skipping execution.");
            return;
        }

        _logger.LogInformation("Running enrollment reminder job at {Time}", now);

        var blockSchemas = _blockHelper.GetAll();

        var reminderTime = _otiumConfiguration.Value.EnrollmentReminder.Time.ToLocalTime();
        var today = DateOnly.FromDateTime(DateTime.Today).ToLocalDate();

        var timezone = DateTimeZoneProviders.Bcl.GetSystemDefault();

        var todayFence = (today + reminderTime).InZoneLeniently(timezone);
        var tomorrowFence = (today.PlusDays(1) + reminderTime).InZoneLeniently(timezone);

        var blocksToCheck = new List<Block>();

        foreach (var schema in blockSchemas)
        {
            var lowerBound = todayFence + ReminderLeadTime + schema.EinschreibeVorlauf.ToDuration();
            var upperBound = tomorrowFence + ReminderLeadTime + schema.EinschreibeVorlauf.ToDuration();

            var candidateDates = CandidateDates(lowerBound, upperBound);

            foreach (var candidateDate in candidateDates)
            {
                var candidateDateBcl = candidateDate.ToDateOnly();
                var block = await _schuljahrService.GetBlockAsync(candidateDateBcl, schema.Id);
                if (block == null) continue;
                if (IsDueToday(block, schema))
                    blocksToCheck.Add(block);
            }
        }

        var missing = new List<(Block block, Person person)>();

        foreach (var block in blocksToCheck)
        {
            var schema = blockSchemas.First(s => s.Id == block.SchemaId);
            var missingPersons = await _enrollmentService.GetNotEnrolledPersonsForBlockAsync(block, schema);
            missing.AddRange(missingPersons.Select(p => (block, p)));
        }

        foreach (var (block, person) in missing)
        {
            var schema = blockSchemas.First(s => s.Id == block.SchemaId);
            const string subject = "Fehlende Einschreibung";
            var body =
                $"Du hast dich für den Block „{schema.Bezeichnung}“ am {block.SchultagKey:dd.MM.yyyy} noch nicht eingeschrieben. Bitte hole das schnellstmöglich nach.";

            await _notificationService.ScheduleNotificationAsync(person.Id, subject, body, TimeSpan.FromMinutes(5));
        }

        context.JobDetail.JobDataMap["last_run"] = now;
        _logger.LogInformation("Enrollment reminder job completed successfully.");
        return;

        IEnumerable<LocalDate> CandidateDates(
            ZonedDateTime lower,
            ZonedDateTime upper)
        {
            var start = lower.Date;
            var end = upper.Date;

            for (var d = start; d <= end; d = d.PlusDays(1))
                yield return d;
        }

        bool IsDueToday(
            Block block,
            BlockMetadata schema)
        {
            var occurrence = (block.SchultagKey.ToLocalDate() + schema.Interval.Start.ToLocalTime())
                .InZoneLeniently(timezone);

            var deadline = occurrence - schema.EinschreibeVorlauf.ToDuration();

            var latestReminder = deadline - ReminderLeadTime;

            var probeDate = latestReminder.Date;
            var probeZdt = (probeDate + reminderTime).InZoneLeniently(timezone);

            var dMax = probeZdt.ToInstant() <= latestReminder.ToInstant()
                ? probeDate
                : probeDate.PlusDays(-1);

            return dMax == today;
        }
    }
}
