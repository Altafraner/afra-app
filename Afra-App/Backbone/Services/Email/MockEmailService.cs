namespace Afra_App.Backbone.Services.Email;

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