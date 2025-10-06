using Altafraner.Backbone.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Altafraner.Backbone.Defaults;

public class CachingModule : IModule
{
    public void ConfigureServices(IServiceCollection services, IConfiguration config, IHostEnvironment env)
    {
        services.AddMemoryCache();
        services.AddHybridCache();
    }
}
