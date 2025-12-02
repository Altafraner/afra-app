using Altafraner.AfraApp.Otium.Configuration;
using Altafraner.AfraApp.Otium.Services;
using Altafraner.AfraApp.User.Domain.Models;
using Altafraner.Backbone.EmailSchedulingModule;
using Altafraner.Backbone.Scheduling;
using Microsoft.Extensions.Options;
using Quartz;

namespace Altafraner.AfraApp.Otium.Jobs;

/// <summary>
/// A background job that sends reminders to users about their missing enrollments in Otium events.
/// </summary>
internal sealed class EnrollmentReminderJob : RetryJob
{
    private readonly INotificationService _notificationService;
    private readonly EnrollmentService _enrollmentService;
    private readonly ILogger<EnrollmentReminderJob> _logger;
    private readonly IOptions<OtiumConfiguration> _otiumConfiguration;

    /// <summary>
    /// Constructor for the EnrollmentReminderJob. Called by the DI container.
    /// </summary>
    public EnrollmentReminderJob(ILogger<EnrollmentReminderJob> logger, EnrollmentService enrollmentService,
        IOptions<OtiumConfiguration> otiumConfiguration, INotificationService notificationService) : base(logger)
    {
        _logger = logger;
        _enrollmentService = enrollmentService;
        _otiumConfiguration = otiumConfiguration;
        _notificationService = notificationService;
    }

    protected override int MaxRetryCount => 3;

    protected override async Task ExecuteAsync(IJobExecutionContext context, int _)
    {
        if (!_otiumConfiguration.Value.EnrollmentReminder.Enabled) return;

        var now = DateTime.Now;
        var tomorrow = DateOnly.FromDateTime(now.AddDays(1));
        var hasRun = context.JobDetail.JobDataMap.TryGetDateTime("last_run", out var lastRun);
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

        var missing = (await _enrollmentService.GetNotEnrolledPersonsForDayAsync(tomorrow))
            .Where(p => p.Rolle == Rolle.Mittelstufe)
            .ToList();
        _logger.LogInformation("Found {Count} persons without enrollments for tomorrow.", missing.Count);

        foreach (var person in missing)
        {
            const string subject = "Fehlende Anmeldungen zum Otium";
            const string body =
                "Du hast dich für morgen noch nicht für alle Otiums-Blöcke eingeschrieben. Bitte hole das schnellstmöglich nach.";

            await _notificationService.ScheduleNotificationAsync(person.Id, subject, body, TimeSpan.FromMinutes(5));
        }

        context.JobDetail.JobDataMap.Put("last_run", now);
        _logger.LogInformation("Enrollment reminder job completed successfully.");
    }
}
