using Altafraner.Backbone.WebNotifications.Domain.Models;

namespace Altafraner.Backbone.WebNotifications.Services;

/// <summary>
///     Service for managing in-app notifications.
/// </summary>
public interface IInAppNotificationService<TPerson> where TPerson : class, IWebNotificationRecipient

{
    /// <summary>
    ///     Creates and delivers an in-app notification to the specified recipient.
    /// </summary>
    Task SendInAppNotificationAsync(Guid recipientId, string subject, string body);

    /// <summary>
    ///     Returns all non-dismissed notifications for the specified user, newest first.
    /// </summary>
    Task<IReadOnlyList<InAppNotification<TPerson>>> GetNotificationsAsync(Guid userId);

    /// <summary>
    ///     Marks a notification as read.
    /// </summary>
    Task MarkAsReadAsync(Guid notificationId, Guid userId);

    /// <summary>
    ///     Dismisses (soft-deletes) a notification so it no longer appears in the center.
    /// </summary>
    Task DismissAsync(Guid notificationId, Guid userId);

    /// <summary>
    ///     Saves or updates a Web Push subscription for a user.
    /// </summary>
    Task SavePushSubscriptionAsync(Guid userId, string endpoint, string p256dh, string auth);

    /// <summary>
    ///     Removes the Web Push subscription with the given endpoint for a user.
    /// </summary>
    Task RemovePushSubscriptionAsync(Guid userId, string endpoint);
}
