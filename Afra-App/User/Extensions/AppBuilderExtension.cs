using Afra_App.User.Configuration;
using Afra_App.User.Services;
using Afra_App.User.Services.LDAP;
using Afra_App.User.Services.SAML;

namespace Afra_App.User.Extensions;

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
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<LdapService>();
        if (builder.Configuration.GetValue<bool>("Saml:Enabled")) builder.Services.AddSingleton<SamlService>();

        builder.Services.AddHostedService<LdapAutoSyncScheduler>();
    }
}
