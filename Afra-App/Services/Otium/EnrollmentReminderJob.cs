using Afra_App.Services.Email;
using Quartz;

namespace Afra_App.Services.Otium;

/// <summary>
/// A background job that sends reminders to users about their missing enrollments in Otium events.
/// </summary>
public class EnrollmentReminderJob : IJob
{
    private readonly IBatchingEmailService _batchingEmailService;
    private readonly EnrollmentService _enrollmentService;
    private readonly ILogger<EnrollmentReminderJob> _logger;

    /// <summary>
    /// Constructor for the EnrollmentReminderJob. Called by the DI container.
    /// </summary>
    public EnrollmentReminderJob(ILogger<EnrollmentReminderJob> logger, EnrollmentService enrollmentService,
        IBatchingEmailService batchingEmailService)
    {
        _logger = logger;
        _enrollmentService = enrollmentService;
        _batchingEmailService = batchingEmailService;
    }

    /// <inheritdoc />
    public async Task Execute(IJobExecutionContext context)
    {
        var now = DateTime.Now;
        var tomorrow = DateOnly.FromDateTime(now.AddDays(1));
        var hasRun = context.JobDetail.JobDataMap.TryGetDateTime("last_run", out var lastRun);
        if (!hasRun && TimeOnly.FromDateTime(now) < EnrollmentReminderService.DefaultReminderTime)
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

                await _batchingEmailService.ScheduleEmailAsync(person.Id, subject, body, TimeSpan.FromMinutes(5));
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
