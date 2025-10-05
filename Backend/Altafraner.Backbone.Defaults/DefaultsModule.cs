using Altafraner.Backbone.Abstractions;
using Altafraner.Backbone.Defaults.Submodules;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Altafraner.Backbone.Defaults;

[DependsOn<DevelopmentModule>]
[DependsOn<PrometheusModule>]
[DependsOn<SignalRModule>]
[DependsOn<CachingModule>]
[DependsOn<HttpJsonOptionsModule>]
[DependsOn<ReverseProxyHandlerModule>]
[DependsOn<HttpContextAccessorModule>]
public class DefaultsModule : IModule
{
    public void ConfigureServices(IServiceCollection services, IConfiguration config, IHostEnvironment env)
    {
    }
}