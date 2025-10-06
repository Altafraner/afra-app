namespace Altafraner.Backbone.EmailOutbox;

/// <summary>
///     An interface representing a batching email notification sender
/// </summary>
public interface IEmailOutbox
{
    /// <summary>
    /// Sends a report via email.
    /// </summary>
    /// <param name="recipient">The recipients E-Mail-Address</param>
    /// <param name="subject">The E-Mails subject</param>
    /// <param name="body">The E-Mail body (plain text)</param>
    public Task SendReportAsync(string recipient, string subject, string body);
}
