using System.Net.Mail;
using Quartz;

namespace Afra_App.Services;

/// <summary>
///     An interface representing a email sender
/// </summary>
public interface IEmailService
{
    /// <summary>
    ///     Sends an Email
    /// </summary>
    public Task SendEmail(string to, string subject, string body);
}

/// <summary>
///     An email sending service
/// </summary>
public class EmailService : IEmailService
{
    private readonly IScheduler _scheduler;

    /// <summary>
    ///     Constructs the EmailService. Usually called by the DI container.
    /// </summary>
    public EmailService(ISchedulerFactory schedulerFactory)
    {
        _scheduler = schedulerFactory.GetScheduler().GetAwaiter().GetResult();
    }

    /// <summary>
    ///     Schedule an Email for ASAP delivery.
    ///     Might batch multiple messages into a single SMTP session in the future.
    /// </summary>
    public Task SendEmail(string toAddress, string subject, string body)
    {
        var jobName = $"mail-{toAddress}-{Guid.NewGuid()}";
        var key = new JobKey(jobName, "mailjobs");

        var job = JobBuilder.Create<MailJob>()
            .WithIdentity(key)
            .UsingJobData("address_to", toAddress)
            .UsingJobData("subject", subject)
            .UsingJobData("body", body)
            .Build();

        var trigger = TriggerBuilder.Create()
            .ForJob(key)
            .StartAt(DateTimeOffset.UtcNow + TimeSpan.FromSeconds(15))
            .Build();

        return _scheduler.ScheduleJob(job, trigger);
    }
}

internal class MailJob : IJob
{
    private readonly SmtpClient _smtpClient;

    public MailJob(SmtpClient smtpClient)
    {
        _smtpClient = smtpClient;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            var dataMap = context.JobDetail.JobDataMap;
            var to = new MailAddress(dataMap.GetString("address_to")!);
            var subject = dataMap.GetString("subject")!;
            var body = dataMap.GetString("body")!;

            var message = new MailMessage(new MailAddress("noreply@localhost"), to)
            {
                Subject = subject,
                Body = body
            };

            await _smtpClient.SendMailAsync(message);
        }
        catch (Exception e)
        {
            throw new JobExecutionException(e, false);
        }
    }
}