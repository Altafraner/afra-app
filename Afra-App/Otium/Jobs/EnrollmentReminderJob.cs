using Afra_App.Backbone.Email.Services.Contracts;
using Afra_App.Backbone.Scheduler.Templates;
using Afra_App.Otium.Configuration;
using Afra_App.Otium.Services;
using Afra_App.User.Domain.Models;
using Microsoft.Extensions.Options;
using Quartz;

namespace Afra_App.Otium.Jobs;

/// <summary>
/// A background job that sends reminders to users about their missing enrollments in Otium events.
/// </summary>
internal sealed class EnrollmentReminderJob : RetryJob
{
    private readonly IEmailOutbox _emailOutbox;
    private readonly EnrollmentService _enrollmentService;
    private readonly ILogger<EnrollmentReminderJob> _logger;
    private readonly IOptions<OtiumConfiguration> _otiumConfiguration;

    /// <summary>
    /// Constructor for the EnrollmentReminderJob. Called by the DI container.
    /// </summary>
    public EnrollmentReminderJob(ILogger<EnrollmentReminderJob> logger, EnrollmentService enrollmentService,
        IEmailOutbox emailOutbox, IOptions<OtiumConfiguration> otiumConfiguration) : base(logger)
    {
        _logger = logger;
        _enrollmentService = enrollmentService;
        _emailOutbox = emailOutbox;
        _otiumConfiguration = otiumConfiguration;
    }

    protected override int MaxRetryCount => 3;

    protected override async Task ExecuteAsync(IJobExecutionContext context, int _)
    {
        if (!_otiumConfiguration.Value.EnrollmentReminder.Enabled) return;

        var now = DateTime.Now;
        var tomorrow = DateOnly.FromDateTime(now.AddDays(1));
        var hasRun = context.JobDetail.JobDataMap.TryGetDateTime("last_run", out var lastRun);
        if (!hasRun && TimeOnly.FromDateTime(now) < _otiumConfiguration.Value.EnrollmentReminder.Time)
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

            await _emailOutbox.ScheduleNotificationAsync(person.Id, subject, body, TimeSpan.FromMinutes(5));
        }

        context.JobDetail.JobDataMap.Put("last_run", now);
        _logger.LogInformation("Enrollment reminder job completed successfully.");
    }
}
