using System.ComponentModel.DataAnnotations;
using Altafraner.Backbone.Abstractions;
using Altafraner.Backbone.CookieAuthentication.Services;
using Altafraner.Backbone.Defaults;
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
        var section = config.GetSection("CookieAuthentication");
        services.AddOptions<CookieAuthenticationSettings>().Bind(section);

        var settings = section.Exists()
            ? section.Get<CookieAuthenticationSettings>() ??
              throw new ValidationException("Cannot bind CookieAuthenticationSettings")
            : new CookieAuthenticationSettings();

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
