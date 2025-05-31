using Afra_App.Data;
using Afra_App.Data.People;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace Afra_App.Services.Email;

/// <summary>
///     A Job that sends a batched email with all pending notifications for a single User
/// </summary>
internal class FlushEmailsJob : IJob
{
    private readonly AfraAppContext _dbContext;
    private readonly IEmailService _emailService;
    private readonly ILogger _logger;

    public FlushEmailsJob(AfraAppContext dbContext, IEmailService emailService, ILogger<FlushEmailsJob> logger)
    {
        _dbContext = dbContext;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext jobContext)
    {
        try
        {
            var dataMap = jobContext.JobDetail.JobDataMap;
            var userId = dataMap.GetGuidValueFromString("user_id");

            var emailsForUser = _dbContext.ScheduledEmails
                .Include(x => x.Recipient)
                .Where(x => x.Recipient.Id == userId)
                .OrderBy(x => x.Deadline) // Short notices are probably more important
                .ToList();

            // Flush jobs might be pending that were set by already sent notifications.
            // Do not send empty batches caused by this condition
            if (emailsForUser.Count == 0) return;

            var user = await _dbContext.Personen.FindAsync(userId);

            // Drop message if user was deleted after scheduling email
            if (user is null) return;

            var userEmail = user.Email;

            const string batchSubject = "Neue Benachrichtigungen";
            var begrüßung = user.Rolle == Rolle.Tutor ? "Sehr geehrter" : "Liebe";
            var anrede = user.Rolle == Rolle.Tutor ? "Sie haben" : "Du hast";
            var batchText = $"""
                             {begrüßung} {user.Vorname} {user.Nachname},

                             {anrede} neue Benachrichtigungen in der Afra-App. Diese sind im folgenden aufgelistet.

                             """;

            for (var i = 0; i < emailsForUser.Count; i++)
            {
                // Notification format:
                //  1. Title
                //     Notification text over
                //     multiple lines
                //  2. Other Title
                //     Notification text
                //  ...
                const string spacing = "    ";
                var email = emailsForUser[i];
                var notificationHeading = $"{i + 1,2}. {email.Subject}";
                var notificationText = spacing + email.Body.ReplaceLineEndings(Environment.NewLine + spacing);
                batchText += notificationHeading + Environment.NewLine + notificationText + Environment.NewLine;
            }

            _logger.LogInformation("Flushing E-Mail Notifications for {email}", userEmail);
            await _emailService.SendEmailAsync(userEmail, batchSubject, batchText);

            _dbContext.RemoveRange(emailsForUser);
            await _dbContext.SaveChangesAsync();
        }
        catch (Exception e)
        {
            _logger.LogWarning("Failed to flush emails for user {userId}", jobContext.JobDetail.JobDataMap["user_id"]);

            // Reschedule the job to retry in 2 minutes
            var oldTrigger = jobContext.Trigger;
            var newTrigger = oldTrigger.GetTriggerBuilder()
                .ForJob(jobContext.JobDetail)
                .WithIdentity($"{oldTrigger.Key.Name}-retry", oldTrigger.Key.Group)
                .StartAt(DateTimeOffset.UtcNow.AddMinutes(2))
                .Build();
            await jobContext.Scheduler.ScheduleJob(newTrigger);

            // Still throw the exception to mark the job as failed
            throw new JobExecutionException(e, false)
            {
                UnscheduleFiringTrigger = true,
                UnscheduleAllTriggers = false
            };
        }
    }
}
