using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Altafraner.AfraApp.Domain.Configuration;
using Altafraner.AfraApp.User.Domain.Models;
using Altafraner.AfraApp.User.Services;
using Altafraner.AfraApp.User.Services.LDAP;
using Altafraner.Backbone.Abstractions;
using Altafraner.Backbone.Defaults;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace Altafraner.AfraApp.Backbone.Auth;

/// <summary>
/// A module for handling simple authorization cases
/// </summary>
[DependsOn<ReverseProxyHandlerModule>]
internal class AuthModule : IModule
{
    public void ConfigureServices(IServiceCollection services, IConfiguration config, IHostEnvironment env)
    {
        var cookieSection = config.GetSection("CookieAuthentication");
        services.AddOptions<CookieAuthenticationSettings>().Bind(cookieSection);

        var cookieSettings = cookieSection.Exists()
            ? cookieSection.Get<CookieAuthenticationSettings>() ??
              throw new ValidationException("Cannot bind CookieAuthenticationSettings")
            : new CookieAuthenticationSettings();

        var oidcSection = config.GetSection("Oidc");
        services.AddOptions<OidcConfiguration>()
            .Validate(OidcConfiguration.Validate)
            .ValidateOnStart()
            .Bind(oidcSection);


        var oidcSettings = oidcSection.Exists()
            ? oidcSection.Get<OidcConfiguration>() ??
              throw new ValidationException("Cannot bind OidcConfiguration")
            : new OidcConfiguration();

        var authBuilder = services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddCookie(options =>
            {
                options.ExpireTimeSpan = cookieSettings.CookieTimeout;
                options.Cookie.HttpOnly = true;
                options.Cookie.SameSite = cookieSettings.SameSiteMode;
                options.Cookie.SecurePolicy = cookieSettings.SecurePolicy;
                options.SlidingExpiration = cookieSettings.SlidingExpiration;
            });

        if (oidcSettings.Enabled)
            authBuilder.AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme,
                options =>
                {
                    options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.ResponseType = OpenIdConnectResponseType.Code;

                    options.Authority = oidcSettings.Authority;
                    options.ClientId = oidcSettings.ClientId;
                    options.ClientSecret = oidcSettings.ClientSecret;

                    options.CallbackPath = new PathString("/api/oidc/signin");
                    options.SignedOutCallbackPath = new PathString("/api/oidc/signout");

                    options.Scope.Add("openid");
                    options.Scope.Add("profile");
                    options.SaveTokens = false;

                    options.Events = new OpenIdConnectEvents
                    {
                        OnTokenValidated = OidcOnTokenValidated,
                        OnAccessDenied = context =>
                        {
                            var logger = context.HttpContext.RequestServices
                                .GetRequiredService<ILogger<AuthModule>>();
                            logger.LogWarning("OIDC Access Denied");
                            context.Response.Redirect("/oidc/access-denied");
                            context.HandleResponse();
                            return Task.CompletedTask;
                        },
                        OnRemoteFailure = context =>
                        {
                            var logger = context.HttpContext.RequestServices
                                .GetRequiredService<ILogger<AuthModule>>();
                            logger.LogWarning("OIDC Unexpected Remote Error: {message}", context.Failure?.Message);
                            context.Response.Redirect("/oidc/remote-error");
                            context.HandleResponse();
                            return Task.CompletedTask;
                        }
                    };
                });

        services.AddAuthorizationBuilder()
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
            .AddPolicy(AuthorizationPolicies.ProfundumsVerantwortlich,
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

    private static async Task OidcOnTokenValidated(TokenValidatedContext context)
    {
        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<OpenIdConnectEvents>>();
        var oidcSettings = context.HttpContext.RequestServices.GetRequiredService<IOptions<OidcConfiguration>>().Value;

        var oidcUser = context.Principal;
        var userId = oidcUser?.FindFirst(oidcSettings.IdClaim!)?.Value;
        logger.LogWarning("UserId: {userId}", userId);

        if (userId is null)
        {
            logger.LogWarning("Received OIDC event without ID");
            context.Fail("The authentication provider did not provide a user ID");
            return;
        }

        var userService = context.HttpContext.RequestServices.GetRequiredService<UserService>();

        var user = await userService.GetUserByLdapIdAsync(new Guid(userId));
        if (user is null)
        {
            var ldapService = context.HttpContext.RequestServices.GetRequiredService<LdapService>();
            await ldapService.SynchronizeAsync();
            user = await userService.GetUserByIdAsync(new Guid(userId));
            if (user is null)
            {
                context.Fail("User not staged for synchronization");
                return;
            }
        }

        var claims = UserSigninService.GenerateClaims(user);
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        context.Principal = principal;
        context.Success();
    }

    public void RegisterMiddleware(WebApplication app)
    {
        app.UseAuthentication();
        app.UseAuthorization();
    }

    public void Configure(WebApplication app)
    {
        app.MapGet("/api/oidc/start",
            () => TypedResults.Challenge(new AuthenticationProperties
                {
                    RedirectUri = "/"
                },
                [OpenIdConnectDefaults.AuthenticationScheme]));
    }
}
