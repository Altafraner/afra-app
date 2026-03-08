namespace Altafraner.Backbone.WebNotifications.Domain.DTO;

/// <summary>
///     A DTO for the user's notification settings.
/// </summary>
public record NotificationSettingsDto
{
    /// <summary>Whether the user wants to receive notifications via email.</summary>
    public required bool ReceiveEmailNotifications { get; init; }
}
