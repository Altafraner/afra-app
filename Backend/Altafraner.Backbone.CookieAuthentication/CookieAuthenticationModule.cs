using Altafraner.Backbone.Abstractions;
using Altafraner.Backbone.CookieAuthentication.Services;
using Altafraner.Backbone.Defaults;
using Altafraner.Backbone.Utils;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Altafraner.Backbone.CookieAuthentication;

/// <summary>
///     A module handling cookie authentication
/// </summary>
[DependsOn<HttpContextAccessorModule>]
public class CookieAuthenticationModule : IModule
{
    /// <inheritdoc />
    public void ConfigureServices(IServiceCollection services, IConfiguration config, IHostEnvironment env)
    {
        var settings =
            ConfigHelper.GetAndRegisterConfig<CookieAuthenticationSettings>(services, config, "CookieAuthentication");

        services.AddAuthentication()
            .AddCookie(options =>
            {
                options.ExpireTimeSpan = settings.CookieTimeout;
                options.Cookie.SameSite = settings.SameSiteMode;
                options.Cookie.SecurePolicy = settings.SecurePolicy;
                options.SlidingExpiration = settings.SlidingExpiration;
            });
        services.AddScoped<IAuthenticationLifetimeService, AuthenticationLifetimeService>();
    }

    /// <inheritdoc />
    public void RegisterMiddleware(WebApplication app)
    {
        app.UseAuthentication();
    }
}
