using Afra_App.Data.Configuration;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;

namespace Afra_App.Services.Email;

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
        var realbody = $"""
                        {body}

                        --
                        Dies ist eine automatisch generierte Nachricht der Afra-App. Bitte antworten Sie nicht auf diese E-Mail.
                        """;
        email.Body = new TextPart(TextFormat.Plain) { Text = realbody };

        using var smtp = new SmtpClient();
        await smtp.ConnectAsync(_emailConfiguration.Host, _emailConfiguration.Port,
            _emailConfiguration.SecureSocketOptions);

        if (_emailConfiguration.Username is not null)
            await smtp.AuthenticateAsync(_emailConfiguration.Username, _emailConfiguration.Password);

        await smtp.SendAsync(email);
        await smtp.DisconnectAsync(true);
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
        _logger.LogInformation("Sending Mail for {to}, subject: {subject}, body:\n{body}", toAddress, subject, body);
        return Task.CompletedTask;
    }
}
