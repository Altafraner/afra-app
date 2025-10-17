using Altafraner.AfraApp.User.Domain.Models;
using Altafraner.Backbone.Abstractions;
using Altafraner.Backbone.CookieAuthentication;
using Altafraner.Backbone.Defaults;

namespace Altafraner.AfraApp.Backbone.Authorization;

/// <summary>
/// A module for handling simple authorization cases
/// </summary>
[DependsOn<ReverseProxyHandlerModule>]
[DependsOn<CookieAuthenticationModule>]
public class AuthorizationModule : IModule
{
    /// <inheritdoc />
    public void ConfigureServices(IServiceCollection services, IConfiguration config, IHostEnvironment env)
    {
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

    /// <inheritdoc />
    public void RegisterMiddleware(WebApplication app)
    {
        app.UseAuthorization();
    }
}
