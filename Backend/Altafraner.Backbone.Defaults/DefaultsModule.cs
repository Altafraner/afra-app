using Altafraner.Backbone.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Altafraner.Backbone.Defaults;

/// <summary>
///     A pseudo-module for including some commonly used modules
/// </summary>
[DependsOn<DevelopmentModule>]
[DependsOn<PrometheusModule>]
[DependsOn<SignalRModule>]
[DependsOn<CachingModule>]
[DependsOn<HttpJsonOptionsModule>]
[DependsOn<ReverseProxyHandlerModule>]
[DependsOn<HttpContextAccessorModule>]
public class DefaultsModule : IModule
{
    /// <inheritdoc />
    public void ConfigureServices(IServiceCollection services, IConfiguration config, IHostEnvironment env)
    {
    }
}
