using Afra_App.Data;
using Afra_App.Data.Email;
using Afra_App.Data.People;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace Afra_App.Services;

/// <summary>
///     An interface representing a batching email notification sender
/// </summary>
public interface IBatchingEmailService
{
    /// <summary>
    ///     Schedule a notification for delivery in a batch within the specified timeframe
    /// </summary>
    public Task ScheduleEmailAsync(Person recipient, string subject, string body, TimeSpan deadline);
}

/// <summary>
///     A notification service that sends notifications to the same user in a single email.
///     Deadlines are respected while minimizing the number of E-Mails sent.
/// </summary>
public class BatchingEmailService : IBatchingEmailService
{
    private readonly AfraAppContext _context;
    private readonly IScheduler _scheduler;

    /// <summary>
    ///     Constructs the BatchingEmailService. Usually called by the DI container.
    /// </summary>
    public BatchingEmailService(ISchedulerFactory schedulerFactory, AfraAppContext context)
    {
        _scheduler = schedulerFactory.GetScheduler().GetAwaiter().GetResult();
        _context = context;
    }

    /// <summary>
    ///     Inserts an email into the database and schedules flushing all mails for the respective user when the deadline
    ///     passes.
    /// </summary>
    /// <param name="recipient">The Person to receive the email</param>
    /// <param name="subject">The subject of the notification (Not the Subject of the actual Email)</param>
    /// <param name="body">The body of the notification</param>
    /// <param name="deadline">The TimeSpan within which to send the email containing the notification</param>
    public async Task ScheduleEmailAsync(Person recipient, string subject, string body, TimeSpan deadline)
    {
        var absDeadLine = DateTime.UtcNow + deadline;
        var mailId = Guid.CreateVersion7();
        _context.ScheduledEmails.Add(
            new ScheduledEmail
            {
                Id = mailId,
                Recipient = recipient,
                Subject = subject,
                Body = body,
                Deadline = absDeadLine
            }
        );
        await _context.SaveChangesAsync();

        var key = new JobKey($"mail-flush-{recipient}-{mailId}", "flush-email");

        // Create a job to flush all notifications to this recipient after the deadline passes
        var job = JobBuilder.Create<FlushEmailsJob>()
            .WithIdentity(key)
            .UsingJobData("user_id", recipient.Id)
            .Build();
        var trigger = TriggerBuilder.Create()
            .ForJob(key)
            .StartAt(absDeadLine)
            .Build();

        await _scheduler.ScheduleJob(job, trigger);
    }
}

/// <summary>
///     A Job that sends a batched email with all pending notifications for a single User
/// </summary>
internal class FlushEmailsJob : IJob
{
    private readonly AfraAppContext _dbContext;
    private readonly IEmailService _emailService;
    private readonly ILogger _logger;

    public FlushEmailsJob(AfraAppContext context, IEmailService emailService, ILogger<FlushEmailsJob> logger)
    {
        _dbContext = context;
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
            if (emailsForUser.Count == 0)
            {
                return;
            }

            var user = await _dbContext.Personen.FindAsync(userId);

            // Drop message if user was deleted after scheduling email
            if (user is null)
            {
                return;
            }

            var userEmail = user.Email;

            await Console.Out.WriteLineAsync($"Flush for {userEmail}");

            const string batchSubject = "Neue Benachrichtigungen";
            var batchText = "";


            foreach (var (em, i) in emailsForUser.Select((x, i) => (x, i)))
            {
                var notificationHeading = $"{i + 1,3}. {em.Subject}";
                var notificationText = $"     {em.Body}";
                batchText += $"{notificationHeading}\n{notificationText}\n";
            }

            _logger.LogInformation("Flushing E-Mail: {batchText}", batchText);
            await _emailService.SendEmailAsync(userEmail, batchSubject, batchText);

            _dbContext.RemoveRange(emailsForUser);
            await _dbContext.SaveChangesAsync();
        }
        catch (Exception e)
        {
            throw new JobExecutionException(e, false);
        }
    }
}