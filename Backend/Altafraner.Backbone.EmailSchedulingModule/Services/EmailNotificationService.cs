using Altafraner.Backbone.EmailSchedulingModule.Jobs;
using Altafraner.Backbone.EmailSchedulingModule.Models;
using Microsoft.EntityFrameworkCore;
using Quartz;

namespace Altafraner.Backbone.EmailSchedulingModule.Services;

internal class EmailNotificationService<TPerson> : INotificationService where TPerson : class, IEmailRecipient
{
    private readonly IScheduledEmailContext<TPerson> _dbContext;
    private readonly ISchedulerFactory _schedulerFactory;

    public EmailNotificationService(IScheduledEmailContext<TPerson> dbContext, ISchedulerFactory schedulerFactory)
    {
        _dbContext = dbContext;
        _schedulerFactory = schedulerFactory;
    }

    public async Task ScheduleNotificationAsync(Guid recipientId, string subject, string body, TimeSpan deadline)
    {
        var absDeadLine = DateTime.UtcNow + deadline;
        var mailId = Guid.CreateVersion7();
        _dbContext.ScheduledEmails.Add(
            new ScheduledEmail<TPerson>
            {
                Id = mailId,
                RecipientId = recipientId,
                Subject = subject,
                Body = body,
                Deadline = absDeadLine
            }
        );

        if (_dbContext is not DbContext contextActions)
            throw new InvalidOperationException("The supplied database does not support this operation.");

        await contextActions.SaveChangesAsync();

        var key = new JobKey($"mail-flush-{recipientId}-{mailId}", "flush-email");

        // Create a job to flush all notifications to this recipient after the deadline passes
        var job = JobBuilder.Create<BatchEmailsJob<TPerson>>()
            .WithIdentity(key)
            .UsingJobData("user_id", recipientId)
            .Build();
        var trigger = TriggerBuilder.Create()
            .ForJob(key)
            .StartAt(absDeadLine)
            .Build();

        var scheduler = await _schedulerFactory.GetScheduler();
        await scheduler.ScheduleJob(job, trigger);
    }
}
