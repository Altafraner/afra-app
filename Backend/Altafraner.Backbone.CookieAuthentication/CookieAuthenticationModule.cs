using Altafraner.Backbone.Abstractions;
using Altafraner.Backbone.CookieAuthentication.Services;
using Altafraner.Backbone.Defaults;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Altafraner.Backbone.CookieAuthentication;

[DependsOn<HttpContextAccessorModule>]
public class CookieAuthenticationModule : IModule<CookieAuthenticationSettings>
{
    public void ConfigureServices(IServiceCollection services, IConfiguration config, IHostEnvironment env)
    {
        var settings = config.Get<CookieAuthenticationSettings>() ?? new CookieAuthenticationSettings();
        services.AddAuthentication()
            .AddCookie(options =>
            {
                options.ExpireTimeSpan = settings.CookieTimeout;
                options.Cookie.SameSite = settings.SameSiteMode;
                options.Cookie.SecurePolicy = settings.SecurePolicy;
                options.SlidingExpiration = settings.SlidingExpiration;
                options.Events.OnRedirectToAccessDenied = context =>
                {
                    var authenticated = context.HttpContext.User.Identity?.IsAuthenticated ?? false;
                    if (authenticated)
                    {
                        context.Response.Clear();
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        context.Response.WriteAsJsonAsync(new ProblemDetails
                        {
                            Title = "Access Denied",
                            Detail =
                                "You are not allowed to access this resource. You do not seem to have the right roles.",
                            Status = StatusCodes.Status403Forbidden,
                            Type = "https://datatracker.ietf.org/doc/html/rfc7231#section-6.5.3"
                        });
                        return Task.CompletedTask;
                    }

                    context.Response.Redirect(context.RedirectUri);
                    return Task.CompletedTask;
                };
            });
        services.AddScoped<IAuthenticationLifetimeService, AuthenticationLifetimeService>();
    }

    public void RegisterMiddleware(WebApplication app)
    {
        app.UseAuthentication();
    }
}
