using System.Security.Cryptography;
using Afra_App.Backbone.Authentication;
using Afra_App.Backbone.Configuration;
using Afra_App.Backbone.Services.Email;
using Afra_App.Backbone.Utilities;
using Afra_App.User.Domain.Models;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Quartz;
using Quartz.AspNetCore;

namespace Afra_App.Backbone.Extensions;

/// <summary>
/// A static class that contains extension methods for the <see cref="WebApplicationBuilder"/> to add Backbone services.
/// </summary>
public static class AppBuilderExtension
{
    /// <summary>
    /// Adds the Backbone services to the <see cref="WebApplicationBuilder"/>.
    /// </summary>
    public static void AddBackbone(this WebApplicationBuilder builder)
    {
        builder.Services.AddOptions<EmailConfiguration>()
            .Bind(builder.Configuration.GetSection("SMTP"))
            .Validate(EmailConfiguration.Validate)
            .ValidateOnStart();
        builder.AddAuthentication();
        builder.AddAuthorization();
        builder.AddScheduler();
        builder.AddDatabase();
        builder.ConfigureDataProtection();
        builder.Services.AddTransient<IEmailService, SmtpEmailService>();
        builder.Services.AddTransient<IEmailOutbox, EmailOutbox>();
    }

    private static void AddAuthentication(this WebApplicationBuilder builder)
    {
        builder.Services.AddAuthentication()
            .AddCookie(options =>
            {
                options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
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
                options.Cookie.SameSite = SameSiteMode.Strict;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
            });
    }

    private static void AddAuthorization(this WebApplicationBuilder builder)
    {
        builder.Services.AddAuthorizationBuilder()
            .AddPolicy(AuthorizationPolicies.StudentOnly,
                policy => policy.RequireClaim(AfraAppClaimTypes.Role,
                    nameof(Rolle.Oberstufe), nameof(Rolle.Mittelstufe)))
            .AddPolicy(AuthorizationPolicies.MittelStufeStudentOnly,
                policy => policy.RequireClaim(AfraAppClaimTypes.Role,
                    nameof(Rolle.Mittelstufe)))
            .AddPolicy(AuthorizationPolicies.TutorOnly,
                policy => policy.RequireClaim(AfraAppClaimTypes.Role,
                    nameof(Rolle.Tutor)))
            .AddPolicy(AuthorizationPolicies.Otiumsverantwortlich,
                policy => policy.RequireClaim(AfraAppClaimTypes.GlobalPermission,
                    nameof(GlobalPermission.Otiumsverantwortlich)))
            .AddPolicy(AuthorizationPolicies.Profundumserantwortlich,
                policy => policy.RequireClaim(AfraAppClaimTypes.GlobalPermission,
                    nameof(GlobalPermission.Profundumsverantwortlich)))
            .AddPolicy(AuthorizationPolicies.AdminOnly,
                policy => policy.RequireClaim(AfraAppClaimTypes.GlobalPermission,
                    nameof(GlobalPermission.Admin)))
            .AddPolicy(AuthorizationPolicies.TeacherOrAdmin,
                policy => policy.RequireAssertion(context =>
                    context.User.HasClaim(AfraAppClaimTypes.GlobalPermission, nameof(GlobalPermission.Admin))
                    || context.User.HasClaim(AfraAppClaimTypes.Role, nameof(Rolle.Tutor))));
    }

    private static void ConfigureDataProtection(this WebApplicationBuilder builder)
    {
        try
        {
            var dataProtectionCert =
                CertificateHelper.LoadX509CertificateAndKey(builder.Configuration, "DataProtection");
            builder.Services.AddDataProtection()
                .SetApplicationName("Afra-App")
                .PersistKeysToDbContext<AfraAppContext>()
                .ProtectKeysWithCertificate(dataProtectionCert);
        }
        catch (CryptographicException exception)
        {
            Console.WriteLine($"Could not load certificate for Domain Protection {exception.Message}");
            Environment.Exit(1);
        }
    }

    private static void AddScheduler(this WebApplicationBuilder builder)
    {
        builder.Services.AddQuartz(q =>
            {
                q.UsePersistentStore(storeOptions =>
                    {
                        var conString = builder.Configuration.GetConnectionString("DefaultConnection")!;
                        storeOptions.UsePostgres(pgOptions =>
                            pgOptions.ConnectionString = conString
                        );
                        storeOptions.UseSystemTextJsonSerializer();
                    }
                );
            }
        );
        builder.Services.AddQuartzServer(options => { options.WaitForJobsToComplete = true; });
    }

    private static void AddDatabase(this WebApplicationBuilder builder)
    {
        builder.Services.AddDbContext<AfraAppContext>(options =>
        {
            options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"),
                AfraAppContext.ConfigureNpgsql);
            options.ConfigureWarnings(w => w.Throw(RelationalEventId.MultipleCollectionIncludeWarning));
        });
    }
}
