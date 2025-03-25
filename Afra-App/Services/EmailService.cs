using Afra_App.Data.Configuration;
using Microsoft.Extensions.Options;
using MimeKit;
using MailKit.Net.Smtp;

namespace Afra_App.Services;

/// <summary>
///     An interface representing a email sender
/// </summary>
public interface IEmailService
{
    /// <summary>
    ///     Sends an Email
    /// </summary>
    public Task SendEmailAsync(string to, string subject, string body);
}

/// <summary>
///     An email sending service
/// </summary>
public class EmailService : IEmailService
{
    private readonly EmailConfiguration _emailConfiguration;

    /// <summary>
    ///     Constructs the EmailService. Usually called by the DI container.
    /// </summary>
    public EmailService(IOptions<EmailConfiguration> emailConfiguration)
    {
        _emailConfiguration = emailConfiguration.Value;
    }

    /// <summary>
    ///     Schedule an Email for ASAP delivery.
    ///     Might batch multiple messages into a single SMTP session in the future.
    /// </summary>
    public async Task SendEmailAsync(string toAddress, string subject, string body)
    {
        var email = new MimeMessage();
        email.Sender = MailboxAddress.Parse(_emailConfiguration.SenderEmail);
        email.Sender.Name = _emailConfiguration.SenderName;
        email.From.Add(email.Sender);
        email.To.Add(MailboxAddress.Parse(toAddress));
        email.Subject = subject;
        email.Body = new TextPart(MimeKit.Text.TextFormat.Plain) { Text = body };
        using (var smtp = new SmtpClient())
        {
            smtp.Connect(_emailConfiguration.Host, _emailConfiguration.Port,
                    _emailConfiguration.SecureSocketOptions);
            smtp.Authenticate(_emailConfiguration.Username, _emailConfiguration.Password);
            await smtp.SendAsync(email);
            smtp.Disconnect(true);
        }
    }
}

/// <summary>
///     An email sending service for debugging that logs any emails to the console unsent
/// </summary>
public class MockEmailService : IEmailService
{
    private readonly ILogger _logger;

    /// <summary>
    ///     Constructs the EmailService. Usually called by the DI container.
    /// </summary>
    public MockEmailService(ILogger<MockEmailService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    ///     Schedule an Email for ASAP delivery.
    ///     Might batch multiple messages into a single SMTP session in the future.
    /// </summary>
    public Task SendEmailAsync(string toAddress, string subject, string body)
    {
        _logger.LogInformation("Sending Mail for {}, subject: {}, body:\n{}", toAddress, subject, body);
        return Task.CompletedTask;
    }
}