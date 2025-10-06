using Altafraner.AfraApp.User.API.Endpoints;
using Altafraner.AfraApp.User.Configuration.LDAP;
using Altafraner.AfraApp.User.Services;
using Altafraner.AfraApp.User.Services.LDAP;
using Altafraner.Backbone.Abstractions;

namespace Altafraner.AfraApp.User;

/// <summary>
///     A module for handling users
/// </summary>
public class UserModule : IModule
{
    /// <inheritdoc />
    public void ConfigureServices(IServiceCollection services, IConfiguration config, IHostEnvironment env)
    {
        services.AddOptions<LdapConfiguration>()
            .Bind(config.GetSection("LDAP"))
            .Validate(LdapConfiguration.Validate)
            .ValidateOnStart();

        services.AddScoped<UserSigninService>();
        services.AddScoped<UserAccessor>();
        services.AddScoped<UserService>();
        services.AddScoped<UserAuthorizationHelper>();
        services.AddHttpContextAccessor();
        services.AddScoped<LdapService>();

        services.AddHostedService<LdapAutoSyncScheduler>();
    }

    /// <inheritdoc />
    public void Configure(WebApplication app)
    {
        app.MapUserEndpoints();
        app.MapPeopleEndpoints();
    }
}
