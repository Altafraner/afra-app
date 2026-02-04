using System.Text;
using Altafraner.Backbone.EmailOutbox;
using Altafraner.Backbone.Scheduling;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Quartz;

namespace Altafraner.Backbone.EmailSchedulingModule.Jobs;

/// <summary>
///     A Job that sends a batched email with all pending notifications for a single User
/// </summary>
internal sealed class BatchEmailsJob<TPerson> : RetryJob
    where TPerson : class, IEmailRecipient
{
    private readonly IScheduledEmailContext<TPerson> _dbContext;
    private readonly IEmailOutbox _emailOutbox;
    private readonly ILogger _logger;

    public BatchEmailsJob(
        IScheduledEmailContext<TPerson> dbContext,
        IEmailOutbox emailOutbox,
        ILogger<BatchEmailsJob<TPerson>> logger
    )
        : base(logger)
    {
        _dbContext = dbContext;
        _emailOutbox = emailOutbox;
        _logger = logger;
    }

    protected override int MaxRetryCount => 1;

    protected override TimeSpan GetRetryDelay(int _) => TimeSpan.FromMinutes(1);

    protected override async Task ExecuteAsync(IJobExecutionContext jobContext, int _)
    {
        var dataMap = jobContext.JobDetail.JobDataMap;
        var userId = dataMap.GetGuidValueFromString("user_id");

        var emailsForUser = await _dbContext
            .ScheduledEmails.Include(x => x.Recipient)
            .Where(x => x.RecipientId == userId)
            .OrderBy(x => x.Deadline) // Short notices are probably more important
            .ToListAsync();

        // Flush jobs might be pending that were set by already sent notifications.
        // Do not send empty batches caused by this condition
        if (emailsForUser.Count == 0)
            return;

        var user = emailsForUser.FirstOrDefault()?.Recipient;

        // Drop message if user was deleted after scheduling email
        if (user is null)
            return;

        var userEmail = user.Email;

        var batchSubject =
            emailsForUser.Count == 1 ? emailsForUser.First().Subject : "Neue Benachrichtigungen";
        var batchText = new StringBuilder(
            $"""
            Hallo {user.FirstName} {user.LastName},

            Es gibt neue Benachrichtigungen in der Afra-App. Diese sind im folgenden aufgelistet.

            """
        );

        foreach (var (e, i) in emailsForUser.Select((e, i) => (e, i)))
        {
            // Notification format:
            //  1. Title
            //     Notification text over
            //     multiple lines
            //  2. Other Title
            //     Notification text
            //  ...
            const string spacing = "    ";
            var notificationHeading = $"{i + 1, 2}. {e.Subject}";
            var notificationText =
                spacing + e.Body.ReplaceLineEndings(Environment.NewLine + spacing);
            batchText.AppendLine(notificationHeading + Environment.NewLine + notificationText);
        }

        _logger.LogInformation("Flushing E-Mail Notifications for {email}", userEmail);
        await _emailOutbox.SendReportAsync(userEmail, batchSubject, batchText.ToString());

        if (_dbContext is not DbContext contextActions)
            throw new InvalidOperationException("The given email store is no dbContext");

        contextActions.RemoveRange(emailsForUser);
        await contextActions.SaveChangesAsync();
    }
}
