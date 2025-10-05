using Altafraner.Backbone.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Altafraner.Backbone.Defaults.Submodules;

public class HttpContextAccessorModule : IModule
{
    public void ConfigureServices(IServiceCollection services, IConfiguration config, IHostEnvironment env)
    {
        services.AddHttpContextAccessor();
    }
}