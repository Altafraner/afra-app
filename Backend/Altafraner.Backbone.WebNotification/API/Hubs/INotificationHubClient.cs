namespace Altafraner.Backbone.WebNotifications.API.Hubs;

/// <summary>
///     The client-side interface for the <see cref="NotificationHub" />.
/// </summary>
public interface INotificationHubClient
{
    /// <summary>
    ///     Delivers a new in-app notification to the connected client.
    /// </summary>
    Task ReceiveNotification(NotificationHubClient.NewNotification notification);
}

/// <summary>
///     Records used by <see cref="INotificationHubClient" />.
/// </summary>
public static class NotificationHubClient
{
    /// <summary>
    ///     A new notification payload sent to the client in real time.
    /// </summary>
    /// <param name="Id">The notification id.</param>
    /// <param name="Subject">The notification subject.</param>
    /// <param name="Body">The notification body.</param>
    /// <param name="CreatedAt">When the notification was created.</param>
    public record NewNotification(Guid Id, string Subject, string Body, DateTime CreatedAt);
}
