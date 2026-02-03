using Altafraner.Backbone.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Altafraner.Backbone.Defaults;

/// <summary>
///     A module for adding caching services
/// </summary>
public class CachingModule : IModule
{
    /// <inheritdoc />
    public void ConfigureServices(
        IServiceCollection services,
        IConfiguration config,
        IHostEnvironment env
    )
    {
        services.AddMemoryCache();
        services.AddHybridCache();
    }
}
