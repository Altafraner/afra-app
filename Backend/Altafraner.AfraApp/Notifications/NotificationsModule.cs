using Altafraner.AfraApp.Notifications.API.Endpoints;
using Altafraner.AfraApp.Notifications.Services;
using Altafraner.AfraApp.User;
using Altafraner.Backbone.Abstractions;

namespace Altafraner.AfraApp.Notifications;

/// <summary>
///     Module that adds in-app notifications, Web Push support and a notification settings API.
/// </summary>
[DependsOn<UserModule>]
[DependsOn<DatabaseModule>]
public class NotificationsModule : IModule
{
    /// <inheritdoc />
    public void ConfigureServices(IServiceCollection services, IConfiguration config, IHostEnvironment env)
    {
        services.AddScoped<INotificationService, CompositeNotificationService>();
    }

    /// <inheritdoc />
    public void Configure(WebApplication app)
    {
        app.MapNotificationEndpoints();
    }
}
