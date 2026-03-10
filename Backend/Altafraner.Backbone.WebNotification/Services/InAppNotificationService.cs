using System.Text.Json;
using Altafraner.Backbone.WebNotifications.API.Hubs;
using Altafraner.Backbone.WebNotifications.Domain.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Altafraner.Backbone.WebNotifications.Services;

/// <summary>
///     Stores in-app notifications in the database, delivers them in real time via SignalR,
///     and sends Web Push messages when the user has active push subscriptions.
/// </summary>
internal class InAppNotificationService<TPerson> : IInAppNotificationService<TPerson> where TPerson : class, IWebNotificationRecipient
{
    private readonly IWebNotificationContext<TPerson> _dbContext;
    private readonly IHubContext<NotificationHub, INotificationHubClient> _hub;
    private readonly ILogger<InAppNotificationService<TPerson>> _logger;
    private readonly WebPushSender<TPerson> _webPushSender;

    /// <summary>
    ///     Constructs a new <see cref="InAppNotificationService{TPerson}" />.
    /// </summary>
    public InAppNotificationService(
        IWebNotificationContext<TPerson> db,
        IHubContext<NotificationHub, INotificationHubClient> hub,
        ILogger<InAppNotificationService<TPerson>> logger,
        WebPushSender<TPerson> webPushSender)
    {
        _dbContext = db;
        _hub = hub;
        _logger = logger;
        _webPushSender = webPushSender;
    }

    /// <inheritdoc />
    public async Task SendInAppNotificationAsync(Guid recipientId, string subject, string body)
    {
        var notification = new InAppNotification<TPerson>
        {
            Id = Guid.CreateVersion7(),
            RecipientId = recipientId,
            Subject = subject,
            Body = body,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.InAppNotifications.Add(notification);

        if (_dbContext is not DbContext contextActions)
            throw new InvalidOperationException("The supplied database does not support this operation.");
        await contextActions.SaveChangesAsync();

        await _hub.Clients.User(recipientId.ToString())
            .ReceiveNotification(new NotificationHubClient.NewNotification(
                notification.Id,
                notification.Subject,
                notification.Body,
                notification.CreatedAt));

        await TrySendPushAsync(recipientId, subject, body);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<InAppNotification<TPerson>>> GetNotificationsAsync(Guid userId)
    {
        return await _dbContext.InAppNotifications
            .Where(n => n.RecipientId == userId && n.DismissedAt == null)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task MarkAsReadAsync(Guid notificationId, Guid userId)
    {
        var notification = await GetOwnedNotificationAsync(notificationId, userId);
        if (notification.ReadAt is null)
        {
            notification.ReadAt = DateTime.UtcNow;
            if (_dbContext is not DbContext contextActions)
                throw new InvalidOperationException("The supplied database does not support this operation.");
            await contextActions.SaveChangesAsync();
        }
    }

    /// <inheritdoc />
    public async Task DismissAsync(Guid notificationId, Guid userId)
    {
        var notification = await GetOwnedNotificationAsync(notificationId, userId);
        notification.DismissedAt = DateTime.UtcNow;
        notification.ReadAt ??= DateTime.UtcNow;
        if (_dbContext is not DbContext contextActions)
            throw new InvalidOperationException("The supplied database does not support this operation.");
        await contextActions.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task SavePushSubscriptionAsync(Guid userId, string endpoint, string p256dh, string auth)
    {
        var existing = await _dbContext.PushSubscriptions.FirstOrDefaultAsync(s => s.Endpoint == endpoint);
        if (existing is not null)
        {
            existing.P256dh = p256dh;
            existing.Auth = auth;
            existing.PersonId = userId;
        }
        else
        {
            _dbContext.PushSubscriptions.Add(new PushSubscription<TPerson>
            {
                Id = Guid.CreateVersion7(),
                PersonId = userId,
                Endpoint = endpoint,
                P256dh = p256dh,
                Auth = auth,
                CreatedAt = DateTime.UtcNow
            });
        }

        if (_dbContext is not DbContext contextActions)
            throw new InvalidOperationException("The supplied database does not support this operation.");
        await contextActions.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task RemovePushSubscriptionAsync(Guid userId, string endpoint)
    {
        var sub = await _dbContext.PushSubscriptions
            .FirstOrDefaultAsync(s => s.PersonId == userId && s.Endpoint == endpoint);
        if (sub is not null)
        {
            _dbContext.PushSubscriptions.Remove(sub);
            if (_dbContext is not DbContext contextActions)
                throw new InvalidOperationException("The supplied database does not support this operation.");
            await contextActions.SaveChangesAsync();
        }
    }

    private async Task TrySendPushAsync(Guid recipientId, string subject, string body)
    {
        if (!_webPushSender.IsEnabled) return;

        var subscriptions = await _dbContext.PushSubscriptions
            .Where(s => s.PersonId == recipientId)
            .ToListAsync();

        if (subscriptions.Count == 0) return;

        var payload = JsonSerializer.Serialize(new { title = subject, body });
        var staleEndpoints = new List<string>();

        foreach (var sub in subscriptions)
        {
            try
            {
                await _webPushSender.SendAsync(new Uri(sub.Endpoint), sub.P256dh, sub.Auth, payload);
            }
            catch (PushSubscriptionGoneException)
            {
                staleEndpoints.Add(sub.Endpoint);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send Web Push notification to endpoint {Endpoint}", sub.Endpoint);
            }
        }

        if (staleEndpoints.Count > 0)
        {
            var toRemove = await _dbContext.PushSubscriptions
                .Where(s => staleEndpoints.Contains(s.Endpoint))
                .ToListAsync();
            _dbContext.PushSubscriptions.RemoveRange(toRemove);
            if (_dbContext is not DbContext contextActions)
                throw new InvalidOperationException("The supplied database does not support this operation.");
            await contextActions.SaveChangesAsync();
        }
    }

    private async Task<InAppNotification<TPerson>> GetOwnedNotificationAsync(Guid notificationId, Guid userId)
    {
        var notification = await _dbContext.InAppNotifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.RecipientId == userId);
        return notification ?? throw new KeyNotFoundException($"Notification {notificationId} not found.");
    }
}
