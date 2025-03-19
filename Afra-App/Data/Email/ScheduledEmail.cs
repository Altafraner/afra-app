namespace Afra_App.Data.Email;

using Afra_App.Data.People;

/// <summary>
/// A record representing an Email scheduled for sending
/// </summary>
public class ScheduledEmail
{
    /// <summary>
    /// The unique identifier of the email.
    /// </summary>
    public required Guid Id { get; set; }

    /// <summary>
    /// The recipient
    /// </summary>
    public required Person Recipient { get; set; }

    /// <summary>
    /// The notification subject
    /// </summary>
    /// <remarks> Not the email subject </remarks>
    public required string Subject { get; set; }

    /// <summary>
    /// The notification text
    /// </summary>
    /// <remarks> Not the email subject </remarks>
    public required string Body { get; set; }

    /// <summary>
    /// A point in time by which the email should have been sent
    /// </summary>
    public required DateTime Deadline { get; set; }
}