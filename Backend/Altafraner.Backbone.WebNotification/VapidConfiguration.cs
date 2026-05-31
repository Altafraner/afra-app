using Microsoft.EntityFrameworkCore;

namespace Altafraner.Backbone.WebNotifications;

/// <summary>
///     Configuration for Web Push (VAPID) notifications.
/// </summary>
public class VapidConfiguration<TPerson>
    where TPerson : class, IWebNotificationRecipient
{
    /// <summary>
    ///     The VAPID public key (URL-safe base64 encoded).
    /// </summary>
    public required string PublicKey { get; set; }

    /// <summary>
    ///     The VAPID private key (URL-safe base64 encoded).
    /// </summary>
    public required string PrivateKey { get; set; }

    /// <summary>
    ///     The VAPID subject (e.g. mailto: or URL).
    /// </summary>
    public required string Subject { get; set; }

    internal static bool Validate(VapidConfiguration<TPerson> config)
    {
        return !string.IsNullOrWhiteSpace(config.PublicKey)
               && !string.IsNullOrWhiteSpace(config.PrivateKey)
               && !string.IsNullOrWhiteSpace(config.Subject);
    }

    internal Type? DbContextType { get; set; }

    /// <summary>
    ///     Registers the DbContextStore to the settings
    /// </summary>
    public VapidConfiguration<TPerson> WithDbContextStore<T>()
        where T : DbContext, IWebNotificationContext<TPerson>
    {
        DbContextType = typeof(T);
        return this;
    }
}
