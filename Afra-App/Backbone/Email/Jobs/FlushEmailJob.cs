using Afra_App.Backbone.Email.Services.Contracts;
using Afra_App.Backbone.Scheduler.Templates;
using Quartz;

namespace Afra_App.Backbone.Email.Jobs;

/// <summary>
/// A job that sends a report via email.
/// </summary>
internal sealed class FlushEmailJob : RetryJob
{
    private readonly IEmailService _emailService;

    public FlushEmailJob(IEmailService emailService, ILogger<FlushEmailJob> logger) : base(logger)
    {
        _emailService = emailService;
    }

    protected override int MaxRetryCount => 3;

    protected override async Task ExecuteAsync(IJobExecutionContext context, int _)
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
}
