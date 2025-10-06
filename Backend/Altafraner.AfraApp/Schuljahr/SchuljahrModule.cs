using Altafraner.AfraApp.Schuljahr.API.Endpoints;
using Altafraner.AfraApp.Schuljahr.Services;
using Altafraner.Backbone.Abstractions;

namespace Altafraner.AfraApp.Schuljahr;

/// <summary>
///     A module for handling the school-year
/// </summary>
public class SchuljahrModule : IModule
{
    /// <inheritdoc />
    public void ConfigureServices(IServiceCollection services, IConfiguration config, IHostEnvironment env)
    {
        services.AddScoped<SchuljahrService>();
    }

    /// <inheritdoc />
    public void Configure(WebApplication app)
    {
        app.MapSchuljahrEndpoints();
    }
}
