using Altafraner.Backbone.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Altafraner.Backbone.Defaults;

/// <summary>
///     Adds the HttpContextAccessor
/// </summary>
public class HttpContextAccessorModule : IModule
{
    /// <inheritdoc />
    public void ConfigureServices(IServiceCollection services, IConfiguration config, IHostEnvironment env)
    {
        services.AddHttpContextAccessor();
    }
}
