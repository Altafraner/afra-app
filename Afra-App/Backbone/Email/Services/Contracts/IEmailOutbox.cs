using Afra_App.User.Domain.Models;

namespace Afra_App.Backbone.Email.Services.Contracts;

/// <summary>
///     An interface representing a batching email notification sender
/// </summary>
public interface IEmailOutbox
{
    /// <summary>
    ///     Schedule a notification for delivery in a batch within the specified timeframe
    /// </summary>
    public Task ScheduleNotificationAsync(Person recipient, string subject, string body, TimeSpan deadline) =>
        ScheduleNotificationAsync(recipient.Id, subject, body, deadline);

    /// <summary>
    ///     Schedule a notification for delivery in a batch within the specified timeframe
    /// </summary>
    public Task ScheduleNotificationAsync(Guid recipientId, string subject, string body, TimeSpan deadline);

    /// <summary>
    /// Sends a report via email.
    /// </summary>
    /// <param name="recipient">The recipients E-Mail-Address</param>
    /// <param name="subject">The E-Mails subject</param>
    /// <param name="body">The E-Mail body (plain text)</param>
    public Task SendReportAsync(string recipient, string subject, string body);
}
