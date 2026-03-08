namespace Altafraner.Backbone.WebNotifications.Domain.DTO;

/// <summary>
///     A DTO for communicating an in-app notification to the client.
/// </summary>
public record NotificationDto
{
    /// <summary>The unique notification id.</summary>
    public required Guid Id { get; init; }

    /// <summary>A short notification title.</summary>
    public required string Subject { get; init; }

    /// <summary>The notification body text.</summary>
    public required string Body { get; init; }

    /// <summary>When the notification was created (UTC).</summary>
    public required DateTime CreatedAt { get; init; }

    /// <summary>Whether the notification has been read.</summary>
    public required bool IsRead { get; init; }
}
