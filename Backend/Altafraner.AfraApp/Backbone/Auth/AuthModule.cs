using System.Security.Claims;
using Altafraner.AfraApp.Domain.Configuration;
using Altafraner.AfraApp.User.Domain.Models;
using Altafraner.AfraApp.User.Services;
using Altafraner.AfraApp.User.Services.LDAP;
using Altafraner.Backbone.Abstractions;
using Altafraner.Backbone.Defaults;
using Altafraner.Backbone.Utils;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace Altafraner.AfraApp.Backbone.Auth;

/// <summary>
///     A module for handling simple authorization cases
/// </summary>
[DependsOn<ReverseProxyHandlerModule>]
internal class AuthModule : IModule
{
    public void ConfigureServices(IServiceCollection services, IConfiguration config, IHostEnvironment env)
    {
        var cookieSettings =
            ConfigHelper.GetAndRegisterConfig<CookieAuthenticationSettings>(services, config, "CookieAuthentication");
        var oidcSettings =
            ConfigHelper.GetAndRegisterConfig<OidcConfiguration>(services, config, "Oidc");

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
                        OnRedirectToIdentityProviderForSignOut = context =>
                        {
                            var idTokenClaim = context.HttpContext.User.FindFirst(AfraAppClaimTypes.OidcIdTokenHint);
                            if (idTokenClaim is not null) context.ProtocolMessage.IdTokenHint = idTokenClaim.Value;

                            return Task.CompletedTask;
                        },
                        OnAccessDenied = context =>
                        {
                            var logger = context.HttpContext.RequestServices
                                .GetRequiredService<ILogger<AuthModule>>();
                            logger.LogWarning("OIDC Access Denied");
                            context.Response.Redirect("/error/access-denied");
                            context.HandleResponse();
                            return Task.CompletedTask;
                        },
                        OnRemoteFailure = context =>
                        {
                            var logger = context.HttpContext.RequestServices
                                .GetRequiredService<ILogger<AuthModule>>();
                            logger.LogWarning("OIDC Unexpected Remote Error: {message}", context.Failure?.Message);
                            context.Response.Redirect("/error/remote-failure");
                            context.HandleResponse();
                            return Task.CompletedTask;
                        }
                    };
                });

        services.AddAuthorizationBuilder()
            .AddPolicy(AuthorizationPolicies.StudentOnly,
                policy => policy.RequireClaim(AfraAppClaimTypes.Role,
                    nameof(Rolle.Oberstufe),
                    nameof(Rolle.Mittelstufe)))
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
        services.AddScoped<IAuthenticationLifetimeService, AuthenticationLifetimeService>();
    }

    private static async Task OidcOnTokenValidated(TokenValidatedContext context)
    {
        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<OpenIdConnectEvents>>();
        var oidcSettings = context.HttpContext.RequestServices.GetRequiredService<IOptions<OidcConfiguration>>().Value;

        var oidcUser = context.Principal;
        var userId = oidcUser?.FindFirst(oidcSettings.IdClaim!)?.Value;

        if (userId is null || oidcUser is null || !Guid.TryParse(userId, out var userGuid))
        {
            logger.LogWarning("Received OIDC event without valid ID");
            context.Fail("The authentication provider did not provide a valid user ID");
            return;
        }

        var userService = context.HttpContext.RequestServices.GetRequiredService<UserService>();

        var user = await userService.GetUserByLdapIdAsync(userGuid);
        if (user is null)
        {
            var ldapService = context.HttpContext.RequestServices.GetRequiredService<LdapService>();
            await ldapService.SynchronizeAsync();
            user = await userService.GetUserByLdapIdAsync(userGuid);
            if (user is null)
            {
                context.Fail("User not staged for LDAP sync");
                return;
            }
        }

        var claims = UserSigninService.GenerateClaims(user);

        // Add claims necessary for oidc single logout to work
        var subClaim = oidcUser.FindFirst(ClaimTypes.NameIdentifier);
        var sidClaim = oidcUser.FindFirst("sid");
        var idToken = context.TokenEndpointResponse?.IdToken;
        if (subClaim is not null) claims.Add(subClaim);
        if (sidClaim is not null) claims.Add(sidClaim);
        if (!string.IsNullOrWhiteSpace(idToken))
            claims.Add(new Claim(AfraAppClaimTypes.OidcIdTokenHint, idToken));

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        context.Principal = principal;
        context.Success();
    }

    /// <inheritdoc />
    public void RegisterMiddleware(WebApplication app)
    {
        app.UseAuthentication();
        app.UseAuthorization();
    }

    public void Configure(WebApplication app)
    {
        app.MapGet("/api/oidc/start",
            (IOptions<CookieAuthenticationSettings> cookieSettings, HttpContext context, bool staySignedIn = false,
                string redirectUrl = "/") => TypedResults.Challenge(
                new AuthenticationProperties
                {
                    RedirectUri = ValidateRedirect(redirectUrl, context) ? redirectUrl : "/",
                    IsPersistent = staySignedIn,
                    ExpiresUtc = staySignedIn ? DateTimeOffset.UtcNow.Add(cookieSettings.Value.CookieTimeout) : null
                },
                [OpenIdConnectDefaults.AuthenticationScheme]));
    }

    // Parts of this are from the .NET Source Code
    private bool ValidateRedirect(string url, HttpContext context)
    {
        if (string.IsNullOrEmpty(url)) return false;

        // Allows "/" or "/foo" but not "//" or "/\".
        if (url[0] == '/')
        {
            // url is exactly "/"
            if (url.Length == 1) return true;

            // url doesn't start with "//" or "/\"
            if (url[1] != '/' && url[1] != '\\') return !HasControlCharacter(url.AsSpan(1));

            return false;
        }

        // Allows "~/" or "~/foo" but not "~//" or "~/\".
        if (url[0] == '~' && url.Length > 1 && url[1] == '/')
        {
            // url is exactly "~/"
            if (url.Length == 2) return true;

            // url doesn't start with "~//" or "~/\"
            if (url[2] != '/' && url[2] != '\\') return !HasControlCharacter(url.AsSpan(2));
        }

        if (Uri.TryCreate(url, UriKind.Absolute, out var targetUri))
        {
            var request = context.Request;

            var sameScheme = string.Equals(targetUri.Scheme, request.Scheme, StringComparison.OrdinalIgnoreCase);
            var sameHost = string.Equals(targetUri.Host, request.Host.Host, StringComparison.OrdinalIgnoreCase);

            var expectedPort = request.Host.Port ?? (request.IsHttps ? 443 : 80);
            var samePort = targetUri.Port == expectedPort;

            return sameScheme && sameHost && samePort;
        }

        return false;

        static bool HasControlCharacter(ReadOnlySpan<char> readOnlySpan)
        {
            // URLs may not contain ASCII control characters.
            for (var i = 0; i < readOnlySpan.Length; i++)
                if (char.IsControl(readOnlySpan[i]))
                    return true;

            return false;
        }
    }
}
