﻿using Afra_App.Backbone.Services.Email;
using Afra_App.Otium.Configuration;
using Microsoft.Extensions.Options;
using Quartz;

namespace Afra_App.Otium.Services;

/// <summary>
/// A background job that sends reminders to users about their missing enrollments in Otium events.
/// </summary>
public class EnrollmentReminderJob : IJob
{
    private readonly IEmailOutbox _emailOutbox;
    private readonly EnrollmentService _enrollmentService;
    private readonly ILogger<EnrollmentReminderJob> _logger;
    private readonly IOptions<OtiumConfiguration> _otiumConfiguration;

    /// <summary>
    /// Constructor for the EnrollmentReminderJob. Called by the DI container.
    /// </summary>
    public EnrollmentReminderJob(ILogger<EnrollmentReminderJob> logger, EnrollmentService enrollmentService,
        IEmailOutbox emailOutbox, IOptions<OtiumConfiguration> otiumConfiguration)
    {
        _logger = logger;
        _enrollmentService = enrollmentService;
        _emailOutbox = emailOutbox;
        _otiumConfiguration = otiumConfiguration;
    }

    /// <inheritdoc />
    public async Task Execute(IJobExecutionContext context)
    {
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

        try
        {
            var missing = await _enrollmentService.GetNotEnrolledPersonsForDayAsync(tomorrow);
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while executing the enrollment reminder job.");
            throw new JobExecutionException(ex)
            {
                RefireImmediately = false,
                UnscheduleFiringTrigger = false,
                UnscheduleAllTriggers = false
            };
        }
    }
}