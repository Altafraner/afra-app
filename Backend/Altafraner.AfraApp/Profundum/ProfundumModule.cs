using Altafraner.AfraApp.Profundum.API.Endpoints;
using Altafraner.AfraApp.Profundum.Configuration;
using Altafraner.AfraApp.Profundum.Extensions;
using Altafraner.AfraApp.Profundum.Services;
using Altafraner.AfraApp.Typst;
using Altafraner.AfraApp.User;
using Altafraner.Backbone.Abstractions;

namespace Altafraner.AfraApp.Profundum;

/// <summary>
///     A Module for handling the Profundum
/// </summary>
[DependsOn<UserModule>]
[DependsOn<DatabaseModule>]
[DependsOn<TypstModule>]
public class ProfundumModule : IModule
{
    /// <inheritdoc />
    public void ConfigureServices(IServiceCollection services, IConfiguration config, IHostEnvironment env)
    {
        services.AddOptions<ProfundumConfiguration>()
            .Bind(config.GetSection("Profundum"))
            .Validate(ProfundumConfiguration.Validate)
            .ValidateOnStart();

        services.AddScoped<ProfundumEnrollmentService>();
        services.AddScoped<ProfundumManagementService>();
        services.AddScoped<ProfundumMatchingService>();

        services.AddRules();
    }

    /// <inheritdoc />
    public void Configure(WebApplication app)
    {
        var group = app.MapGroup("/api/profundum");
        group.MapEnrollmentEndpoints();
        group.MapManagementEndpoints();
    }
}
