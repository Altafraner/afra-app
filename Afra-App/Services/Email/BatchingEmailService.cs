using Afra_App.Data;
using Afra_App.Data.Email;
using Quartz;

namespace Afra_App.Services.Email;

/// <summary>
///     A notification service that sends notifications to the same user in a single email.
///     Deadlines are respected while minimizing the number of E-Mails sent.
/// </summary>
public class BatchingEmailService : IBatchingEmailService
{
    private readonly AfraAppContext _dbContext;
    private readonly IScheduler _scheduler;

    /// <summary>
    ///     Constructs the BatchingEmailService. Usually called by the DI container.
    /// </summary>
    public BatchingEmailService(ISchedulerFactory schedulerFactory, AfraAppContext dbContext)
    {
        _scheduler = schedulerFactory.GetScheduler().GetAwaiter().GetResult();
        _dbContext = dbContext;
    }

    /// <summary>
    ///     Inserts an email into the database and schedules flushing all mails for the respective user when the deadline
    ///     passes.
    /// </summary>
    /// <param name="recipientId">The ID of the person to receive the email</param>
    /// <param name="subject">The subject of the notification (Not the Subject of the actual Email)</param>
    /// <param name="body">The body of the notification</param>
    /// <param name="deadline">The TimeSpan within which to send the email containing the notification</param>
    public async Task ScheduleEmailAsync(Guid recipientId, string subject, string body, TimeSpan deadline)
    {
        var absDeadLine = DateTime.UtcNow + deadline;
        var mailId = Guid.CreateVersion7();
        _dbContext.ScheduledEmails.Add(
            new ScheduledEmail
            {
                Id = mailId,
                RecipientId = recipientId,
                Subject = subject,
                Body = body,
                Deadline = absDeadLine
            }
        );
        await _dbContext.SaveChangesAsync();

        var key = new JobKey($"mail-flush-{recipientId}-{mailId}", "flush-email");

        // Create a job to flush all notifications to this recipient after the deadline passes
        var job = JobBuilder.Create<FlushEmailsJob>()
            .WithIdentity(key)
            .UsingJobData("user_id", recipientId)
            .Build();
        var trigger = TriggerBuilder.Create()
            .ForJob(key)
            .StartAt(absDeadLine)
            .Build();

        await _scheduler.ScheduleJob(job, trigger);
    }
}
