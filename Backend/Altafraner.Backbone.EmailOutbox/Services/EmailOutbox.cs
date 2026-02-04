using Altafraner.Backbone.EmailOutbox.Jobs;
using Quartz;

namespace Altafraner.Backbone.EmailOutbox.Services;

/// <summary>
///     A notification service that sends notifications to the same user in a single email.
///     Deadlines are respected while minimizing the number of E-Mails sent.
/// </summary>
internal class EmailOutbox : IEmailOutbox
{
    private readonly IScheduler _scheduler;

    /// <summary>
    ///     Constructs the EmailOutbox. Usually called by the DI container.
    /// </summary>
    public EmailOutbox(ISchedulerFactory schedulerFactory)
    {
        _scheduler = schedulerFactory.GetScheduler().GetAwaiter().GetResult();
    }

    /// <inheritdoc />
    public Task SendReportAsync(string recipient, string subject, string body)
    {
        var jobDataMap = new JobDataMap
        {
            { "subject", subject },
            { "body", body },
            { "recipient", recipient },
            { "retryCount", 0 },
        };
        var job = JobBuilder
            .Create<FlushEmailJob>()
            .WithIdentity($"report-{Guid.CreateVersion7()}", "flush-report")
            .UsingJobData(jobDataMap)
            .Build();

        var trigger = TriggerBuilder.Create().StartNow().Build();

        return _scheduler.ScheduleJob(job, trigger);
    }
}
