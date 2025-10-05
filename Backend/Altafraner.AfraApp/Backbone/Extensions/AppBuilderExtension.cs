using Altafraner.AfraApp.Backbone.Authentication;
using Altafraner.AfraApp.Backbone.EmergencyBackup.Extensions;
using Altafraner.AfraApp.User.Domain.Models;
using Altafraner.Backbone.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Altafraner.AfraApp.Backbone.Extensions;

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
        builder.AddAuthorization();
        builder.AddDatabase();
        builder.AddEmergencyPostBackup();
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

    private static void AddDatabase(this WebApplicationBuilder builder)
    {
        builder.Services.AddDbContext<AfraAppContext>(options =>
        {
            options.AddInterceptors(new TimestampInterceptor());
            options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"),
                AfraAppContext.ConfigureNpgsql);
            options.ConfigureWarnings(w => w.Throw(RelationalEventId.MultipleCollectionIncludeWarning));
        });
    }
}
