using System.ComponentModel.DataAnnotations;
using Afra_App.User.Domain.Models;

namespace Afra_App.Backbone.Email.Domain.Models;

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
    ///     The foreign key to the recipient
    /// </summary>
    public Guid RecipientId { get; set; }

    /// <summary>
    /// The recipient
    /// </summary>
    public Person Recipient { get; set; } = null!;

    /// <summary>
    /// The notification subject
    /// </summary>
    /// <remarks> Not the email subject </remarks>
    [MaxLength(100)]
    public required string Subject { get; set; }

    /// <summary>
    /// The notification text
    /// </summary>
    /// <remarks> Not the email subject </remarks>
    [MaxLength(10000)]
    public required string Body { get; set; }

    /// <summary>
    /// A point in time by which the email should have been sent
    /// </summary>
    public required DateTime Deadline { get; set; }
}
