using Afra_App.Data;
using Afra_App.Data.People;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace Afra_App.Services.Email;

/// <summary>
///     A Job that sends a batched email with all pending notifications for a single User
/// </summary>
internal class BatchEmailsJob : IJob
{
    private readonly AfraAppContext _dbContext;
    private readonly IEmailOutbox _emailOutbox;
    private readonly ILogger _logger;

    public BatchEmailsJob(
        AfraAppContext dbContext,
        IEmailOutbox emailOutbox,
        ILogger<BatchEmailsJob> logger)
    {
        _dbContext = dbContext;
        _emailOutbox = emailOutbox;
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

            var batchSubject = emailsForUser.Count == 1 ? emailsForUser.First().Subject : "Neue Benachrichtigungen";
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
            await _emailOutbox.SendReportAsync(userEmail, batchSubject, batchText);

            _dbContext.RemoveRange(emailsForUser);
            await _dbContext.SaveChangesAsync();
        }
        catch (Exception e)
        {
            _logger.LogWarning("Failed to flush emails for user {userId}", jobContext.JobDetail.JobDataMap["user_id"]);

            // Do not retry if this is a retry job
            if (jobContext.Trigger.Key.Name.EndsWith("-retry"))
            {
                _logger.LogError("A retry job failed to batch emails: {Message}", e.Message);
                throw new JobExecutionException("Failed to flush emails after retrying.", e)
                {
                    RefireImmediately = false,
                    UnscheduleFiringTrigger = true,
                    UnscheduleAllTriggers = false
                };
            }

            // Reschedule the job to retry in 1 minute
            var oldTrigger = jobContext.Trigger;
            var newTrigger = oldTrigger.GetTriggerBuilder()
                .ForJob(jobContext.JobDetail)
                .WithIdentity($"{oldTrigger.Key.Name}-retry", oldTrigger.Key.Group)
                .StartAt(DateTimeOffset.UtcNow.AddMinutes(1))
                .Build();
            await jobContext.Scheduler.RescheduleJob(jobContext.Trigger.Key, newTrigger);

            // Still throw the exception to mark the job as failed
            throw new JobExecutionException(e, false)
            {
                UnscheduleFiringTrigger = true,
                UnscheduleAllTriggers = false
            };
        }
    }
}
