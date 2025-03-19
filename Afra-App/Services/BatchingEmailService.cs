namespace Afra_App.Services;

using Afra_App.Data;
using Afra_App.Data.Email;
using Afra_App.Data.People;
using Microsoft.EntityFrameworkCore;
using Quartz;

/// <summary>
///     An interface representing a batching email notification sender
/// </summary>
public interface IBatchingEmailService
{
    /// <summary>
    ///     Schedule a notification for delivery in a batch within the specified timeframe
    /// </summary>
    public Task ScheduleEmail(Person recipient, string subject, string body, TimeSpan deadline);
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
    ///     Inserts an email into the database and schedules flushing all mails for the respective user when the deadline passes.
    /// </summary>
    /// <param name="recipient">The Person to receive the email</param>
    /// <param name="subject">The subject of the notification (Not the Subject of the actual Email)</param>
    /// <param name="body">The body of the notification</param>
    /// <param name="deadline">The TimeSpan within which to send the email containing the notification</param>
    public async Task ScheduleEmail(Person recipient, string subject, string body, TimeSpan deadline)
    {

        DateTimeOffset absDeadLine = DateTimeOffset.UtcNow + deadline;
        _context.ScheduledEmails.Add(
                new ScheduledEmail
                {
                    Id = new Guid(),
                    Recipient = recipient,
                    Subject = subject,
                    Body = body,
                    Deadline = absDeadLine,
                }
        );
        await _context.SaveChangesAsync();

        JobKey key = new JobKey($"mail-flush-{recipient}-{Guid.NewGuid()}", "flush-email");

        // Create a job to flush all notifications to this recipient after the deadline passes
        IJobDetail job = JobBuilder.Create<FlushEmailsJob>()
            .WithIdentity(key)
            .UsingJobData("user_id", recipient.Id)
            .Build();
        ITrigger trigger = TriggerBuilder.Create()
            .ForJob(key)
            .StartAt(absDeadLine)
            .Build();

        await _scheduler.ScheduleJob(job, trigger);
    }
}

/// <summary>
///     A Job that sends a batched email with all pending notifications for a single User
/// </summary>
class FlushEmailsJob : IJob
{
    private readonly AfraAppContext _dbContext;
    private readonly IEmailService _emailService;

    public FlushEmailsJob(AfraAppContext context, IEmailService emailService)
    {
        _dbContext = context;
        _emailService = emailService;
    }

    public async Task Execute(IJobExecutionContext jobContext)
    {
        try
        {
            JobDataMap dataMap = jobContext.JobDetail.JobDataMap;
            Guid user_id = dataMap.GetGuidValueFromString("user_id");

            var emailsForUser = _dbContext.ScheduledEmails
                .Include(x => x.Recipient)
                .Where(x => x.Recipient.Id == user_id)
                .OrderBy(x => x.Deadline) // Short notices are probably more important
                .ToList();

            // Flush jobs might be pending that were set by already sent notifications.
            // Do not send empty batches caused by this condition
            if (!emailsForUser.Any())
                return;

            var user_email = _dbContext.Personen.Find(user_id)!.Email;

            await Console.Out.WriteLineAsync($"Flush for {user_email}");

            const string batchSubject = "Neue Benachrichtigungen";
            string batchText = "";


            foreach (var (em, i) in emailsForUser.Select((x, i) => (x, i)))
            {
                string notificationHeading = $"{(i + 1),3}. {em.Subject}";
                string notificationText = $"     {em.Body}";
                batchText += $"{notificationHeading}\n{notificationText}\n";
            }

            await Console.Out.WriteLineAsync(batchText);

            await _emailService.SendEmail(user_email, batchSubject, batchText);

            _dbContext.RemoveRange(emailsForUser);

            await _dbContext.SaveChangesAsync();

        }
        catch (Exception e)
        {
            throw new JobExecutionException(e, refireImmediately: false);
        }
    }
}