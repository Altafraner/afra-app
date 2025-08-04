using Quartz;

namespace Afra_App.Backbone.Services.Email;

/// <summary>
/// A job that sends a report via email.
/// </summary>
[PersistJobDataAfterExecution]
public class FlushEmailJob : IJob
{
    private readonly IEmailService _emailService;

    /// <summary>
    /// Initializes a new instance of the <see cref="FlushEmailJob"/> class. Called by DI.
    /// </summary>
    public FlushEmailJob(IEmailService emailService)
    {
        _emailService = emailService;
    }

    /// <inheritdoc />
    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            var subject = context.MergedJobDataMap.GetString("subject");
            var body = context.MergedJobDataMap.GetString("body");
            var recipient = context.MergedJobDataMap.GetString("recipient");
            if (string.IsNullOrEmpty(subject) || string.IsNullOrEmpty(body) || string.IsNullOrEmpty(recipient))
                throw new JobExecutionException("Subject, body, and recipient must be provided for the report job.")
                {
                    RefireImmediately = false
                };

            await _emailService.SendEmailAsync(recipient, subject, body);
        }
        catch (Exception e) when (e is not JobExecutionException)
        {
            var hasRetryCount = context.MergedJobDataMap.TryGetIntValue("retryCount", out var retryCount);
            if (!hasRetryCount) retryCount = 0;
            if (retryCount < 3)
            {
                context.JobDetail.JobDataMap.Put("retryCount", retryCount + 1);
                var trigger = TriggerBuilder.Create()
                    .ForJob(context.JobDetail.Key)
                    .UsingJobData("retryCount", retryCount + 1)
                    .StartAt(DateTimeOffset.Now.AddMinutes(3 * (retryCount + 1)));
                await context.Scheduler.RescheduleJob(context.Trigger.Key, trigger.Build());
            }

            throw new JobExecutionException(e, false);
        }
    }
}
