using Quartz;

namespace Altafraner.Backbone.EmailOutbox.Jobs;

/// <summary>
/// A job that sends a report via email.
/// </summary>
internal sealed class FlushEmailJob : IJob
{
    private readonly IEmailService _emailService;

    public FlushEmailJob(IEmailService emailService)
    {
        _emailService = emailService;
    }

    public async ValueTask Execute(IJobExecutionContext context,
        CancellationToken cancellationToken)
    {
        var subject = context.MergedJobDataMap.GetString("subject");
        var body = context.MergedJobDataMap.GetString("body");
        var recipient = context.MergedJobDataMap.GetString("recipient");
        if (string.IsNullOrEmpty(subject) || string.IsNullOrEmpty(body) || string.IsNullOrEmpty(recipient))
            throw new JobExecutionException("Subject, body, and recipient must be provided for the report job.")
            {
                RefireImmediately = false,
                UnscheduleFiringTrigger = true
            };

        await _emailService.SendEmailAsync(recipient, subject, body);
    }
}
