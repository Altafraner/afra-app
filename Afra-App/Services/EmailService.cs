namespace Afra_App.Services;

using System.Net.Mail;
using Quartz;


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
    private readonly SmtpClient _smtpClient;

    /// <summary>
    ///     Constructs the EmailService. Usually called by the DI container.
    /// </summary>
    public EmailService(ISchedulerFactory schedulerFactory, SmtpClient smtpClient)
    {
        _scheduler = schedulerFactory.GetScheduler().GetAwaiter().GetResult();
        _smtpClient = smtpClient;
    }

    /// <summary>
    ///     Schedule an Email for ASAP delivery.
    ///     Might batch multiple messages into a single SMTP session in the future.
    /// </summary>
    public Task SendEmail(string toAddress, string subject, string body)
    {

        string jobName = $"mail-{toAddress}-{Guid.NewGuid()}";
        JobKey key = new JobKey(jobName, "mailjobs");

        IJobDetail job = JobBuilder.Create<MailJob>()
            .WithIdentity(key)
            .UsingJobData("address_to", toAddress)
            .UsingJobData("subject", subject)
            .UsingJobData("body", body)
            .Build();

        ITrigger trigger = TriggerBuilder.Create()
            .ForJob(key)
            .StartAt(DateTimeOffset.UtcNow + TimeSpan.FromSeconds(15))
            .Build();

        return _scheduler.ScheduleJob(job, trigger);
    }
}


class MailJob : IJob
{
    private SmtpClient _smtpClient;

    public MailJob(SmtpClient smtpClient)
    {
        _smtpClient = smtpClient;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            JobDataMap dataMap = context.JobDetail.JobDataMap;
            MailAddress to = new MailAddress(dataMap.GetString("address_to")!);
            string subject = dataMap.GetString("subject")!;
            string body = dataMap.GetString("body")!;

            var message = new MailMessage(new MailAddress("noreply@localhost"), to)
            {
                Subject = subject,
                Body = body,
            };

            await _smtpClient.SendAsync(message);
        }
        catch (Exception e)
        {
            throw new JobExecutionException(e, refireImmediately: false);
        }
    }
}