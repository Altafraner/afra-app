namespace Altafraner.AfraApp.Backbone.Email.Services.Contracts;

/// <summary>
///     An interface representing an email sender
/// </summary>
public interface IEmailService
{
    /// <summary>
    ///     Sends an Email
    /// </summary>
    public Task SendEmailAsync(string to, string subject, string body);
}
