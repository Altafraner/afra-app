using Altafraner.Backbone.Abstractions;
using Altafraner.Backbone.WebNotifications.API.Hubs;
using Altafraner.Backbone.WebNotifications.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Altafraner.Backbone.WebNotifications;

/// <summary>
///     Module that adds in-app notifications, Web Push support and a notification settings API.
/// </summary>
/// <typeparam name="TPerson">The data type representing a user in the current context</typeparam>
public class WebNotificationsModule<TPerson> : IModule<VapidConfiguration<TPerson>>
    where TPerson : class, IWebNotificationRecipient
{
    /// <inheritdoc />
    public void ConfigureServices(IServiceCollection services, IConfiguration config, IHostEnvironment env)
    {
        services.AddOptions<VapidConfiguration<TPerson>>()
            .Bind(config.GetSection("VAPID"));

        services.AddHttpClient("WebPush");

        services.AddScoped<WebPushSender<TPerson>>();

        services.AddScoped<IWebNotificationContext<TPerson>>(sp =>
        {
            var settings = sp.GetRequiredService<IOptions<VapidConfiguration<TPerson>>>();
            var contextType = settings.Value.DbContextType ?? throw new InvalidOperationException("Cannot find VaapidConfiguration");
            return sp.GetRequiredService(contextType) as IWebNotificationContext<TPerson> ??
                   throw new InvalidOperationException("Module not configured");
        }
        );

        services.AddScoped<IInAppNotificationService<TPerson>, InAppNotificationService<TPerson>>();
    }
    /// <inheritdoc />
    public void Configure(WebApplication app)
    {
        app.MapHub<NotificationHub>("/api/notifications/hub")
            .RequireAuthorization();
    }
}
