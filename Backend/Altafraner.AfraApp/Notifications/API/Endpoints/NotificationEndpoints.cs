using Altafraner.AfraApp.Notifications.Services;
using Altafraner.AfraApp.User.Domain.Models;
using Altafraner.AfraApp.User.Services;
using Altafraner.Backbone.WebNotifications.Domain.DTO;
using Altafraner.Backbone.WebNotifications.Services;
using Microsoft.EntityFrameworkCore;

namespace Altafraner.AfraApp.Notifications.API.Endpoints;

/// <summary>
///     Extension methods for mapping notification-related endpoints.
/// </summary>
public static class NotificationEndpoints
{
    /// <summary>
    ///     Maps all notification endpoints onto the provided <see cref="IEndpointRouteBuilder" />.
    /// </summary>
    public static void MapNotificationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/notifications").RequireAuthorization();

        group.MapGet("", async (IInAppNotificationService<Person> svc, UserAccessor userAccessor) =>
        {
            var userId = userAccessor.GetUserId();
            var notifications = await svc.GetNotificationsAsync(userId);
            return Results.Ok(notifications.Select(n => new NotificationDto
            {
                Id = n.Id,
                Subject = n.Subject,
                Body = n.Body,
                CreatedAt = n.CreatedAt,
                IsRead = n.ReadAt.HasValue
            }));
        });

        group.MapPut("{id:guid}/read", async (Guid id, IInAppNotificationService<Person> svc, UserAccessor userAccessor) =>
        {
            try
            {
                await svc.MarkAsReadAsync(id, userAccessor.GetUserId());
                return Results.NoContent();
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
        });

        group.MapDelete("{id:guid}", async (Guid id, IInAppNotificationService<Person> svc, UserAccessor userAccessor) =>
        {
            try
            {
                await svc.DismissAsync(id, userAccessor.GetUserId());
                return Results.NoContent();
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
        });

        group.MapGet("settings",
            async (UserAccessor userAccessor, AfraAppContext db) =>
            {
                var userId = userAccessor.GetUserId();
                var settings = await db.Personen
                    .Where(p => p.Id == userId)
                    .Select(p => new NotificationSettingsDto { ReceiveEmailNotifications = p.ReceiveEmailNotifications })
                    .FirstOrDefaultAsync();
                return settings is null ? Results.Unauthorized() : Results.Ok(settings);
            });

        group.MapPut("settings",
            async (NotificationSettingsDto dto, UserAccessor userAccessor, AfraAppContext db) =>
            {
                var userId = userAccessor.GetUserId();
                var person = await db.Personen.FindAsync(userId);
                if (person is null) return Results.Unauthorized();
                person.ReceiveEmailNotifications = dto.ReceiveEmailNotifications;
                await db.SaveChangesAsync();
                return Results.NoContent();
            });

        group.MapGet("vapid-public-key",
            (IConfiguration config) =>
            {
                var key = config.GetSection("VAPID")["PublicKey"];
                return string.IsNullOrEmpty(key) ? Results.NotFound() : Results.Ok(new { publicKey = key });
            });

        group.MapPost("push-subscription",
            async (PushSubscriptionDto dto, IInAppNotificationService<Person> svc, UserAccessor userAccessor) =>
            {
                await svc.SavePushSubscriptionAsync(userAccessor.GetUserId(), dto.Endpoint, dto.P256dh, dto.Auth);
                return Results.NoContent();
            });

        group.MapPost("push-subscription/unsubscribe",
            async (PushSubscriptionDto dto, IInAppNotificationService<Person> svc, UserAccessor userAccessor) =>
            {
                await svc.RemovePushSubscriptionAsync(userAccessor.GetUserId(), dto.Endpoint);
                return Results.NoContent();
            });

        group.MapPost("test",
            async (INotificationService svc, UserAccessor userAccessor) =>
            {
                await svc.ScheduleNotificationAsync(
                    userAccessor.GetUserId(),
                    "Testbenachrichtigung",
                    "Das Benachrichtigungssystem funktioniert korrekt.",
                    TimeSpan.Zero);
                return Results.NoContent();
            });
    }
}
