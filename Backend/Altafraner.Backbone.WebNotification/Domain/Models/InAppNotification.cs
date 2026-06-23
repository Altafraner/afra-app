namespace Altafraner.Backbone.WebNotifications.Domain.Models;

/// <summary>
///     An in-app notification stored for a specific user.
/// </summary>
public class InAppNotification<TPerson> where TPerson : class, IWebNotificationRecipient
{
    /// <summary>
    ///     The unique identifier for this notification.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    ///     The id of the recipient.
    /// </summary>
    public Guid RecipientId { get; set; }

    /// <summary>
    ///     The recipient person.
    /// </summary>
    public TPerson Recipient { get; set; } = null!;

    /// <summary>
    ///     A short notification title.
    /// </summary>
    public required string Subject { get; set; }

    /// <summary>
    ///     The notification body text.
    /// </summary>
    public required string Body { get; set; }

    /// <summary>
    ///     When the notification was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    ///     When the user read the notification. Null if unread.
    /// </summary>
    public DateTime? ReadAt { get; set; }

    /// <summary>
    ///     When the user dismissed the notification. Null if not dismissed.
    /// </summary>
    public DateTime? DismissedAt { get; set; }
}
