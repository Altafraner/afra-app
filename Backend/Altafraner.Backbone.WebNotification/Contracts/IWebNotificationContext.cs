using Altafraner.Backbone.WebNotifications.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Altafraner.Backbone.WebNotifications;

/// <summary>
///     Describes how a db context must be designed to include relevant fields for web notifications
/// </summary>
public interface IWebNotificationContext<TPerson> where TPerson : class, IWebNotificationRecipient
{
    /// <summary>
    ///     In-app notifications
    /// </summary>
    public DbSet<InAppNotification<TPerson>> InAppNotifications { get; set; }

    /// <summary>
    ///     Web Push subscriptions
    /// </summary>
    public DbSet<PushSubscription<TPerson>> PushSubscriptions { get; set; }
}
