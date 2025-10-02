using Altafraner.AfraApp.Backbone.Email.Configuration;
using Altafraner.AfraApp.Backbone.Email.Services.Contracts;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;

namespace Altafraner.AfraApp.Backbone.Email.Services.Implementations;

/// <summary>
///     An email sending service
/// </summary>
public class SmtpEmailService : IEmailService
{
    private readonly EmailConfiguration _emailConfiguration;

    /// <summary>
    ///     Constructs the SmtpEmailService. Usually called by the DI container.
    /// </summary>
    public SmtpEmailService(IOptions<EmailConfiguration> emailConfiguration)
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
        email.Subject = "[Afra-App] " + subject;
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
