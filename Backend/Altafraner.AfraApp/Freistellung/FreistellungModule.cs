using Altafraner.AfraApp.Freistellung.API.Endpoints;
using Altafraner.AfraApp.Freistellung.Services;
using Altafraner.AfraApp.User;
using Altafraner.Backbone.Abstractions;

namespace Altafraner.AfraApp.Freistellung;

/// <summary>
///     A Module for handling leave requests (Freistellungsanträge).
/// </summary>
[DependsOn<UserModule>]
[DependsOn<DatabaseModule>]
public class FreistellungModule : IModule
{
    /// <inheritdoc />
    public void ConfigureServices(IServiceCollection services, IConfiguration config, IHostEnvironment env)
    {
        services.AddScoped<FreistellungsService>();
    }

    /// <inheritdoc />
    public void Configure(WebApplication app)
    {
        var group = app.MapGroup("/api/freistellung")
            .RequireAuthorization();
        group.MapFreistellungsEndpoints();
    }
}
