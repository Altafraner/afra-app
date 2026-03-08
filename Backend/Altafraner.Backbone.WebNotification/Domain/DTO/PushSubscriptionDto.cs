namespace Altafraner.Backbone.WebNotifications.Domain.DTO;

/// <summary>
///     A DTO for registering a Web Push subscription.
/// </summary>
public record PushSubscriptionDto
{
    /// <summary>The push service endpoint URL.</summary>
    public required string Endpoint { get; init; }

    /// <summary>The P256DH key.</summary>
    public required string P256dh { get; init; }

    /// <summary>The auth secret.</summary>
    public required string Auth { get; init; }
}
