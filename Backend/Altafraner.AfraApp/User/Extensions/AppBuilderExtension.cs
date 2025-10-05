using Altafraner.AfraApp.User.Configuration.LDAP;
using Altafraner.AfraApp.User.Services;
using Altafraner.AfraApp.User.Services.LDAP;

namespace Altafraner.AfraApp.User.Extensions;

/// <summary>
/// A static class that provides extension methods for <see cref="WebApplicationBuilder"/> to add user-related services.
/// </summary>
public static class AppBuilderExtension
{
    /// <summary>
    /// Adds user-related services to the <see cref="WebApplicationBuilder"/>.
    /// </summary>
    public static void AddUser(this WebApplicationBuilder builder)
    {
        builder.Services.AddOptions<LdapConfiguration>()
            .Bind(builder.Configuration.GetSection("LDAP"))
            .Validate(LdapConfiguration.Validate)
            .ValidateOnStart();

        builder.Services.AddScoped<UserSigninService>();
        builder.Services.AddScoped<UserAccessor>();
        builder.Services.AddScoped<UserService>();
        builder.Services.AddScoped<UserAuthorizationHelper>();
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<LdapService>();

        builder.Services.AddHostedService<LdapAutoSyncScheduler>();
    }
}
