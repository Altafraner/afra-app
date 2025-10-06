using Altafraner.Backbone.Abstractions;
using Altafraner.Backbone.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Altafraner.Backbone.Defaults;

public class HttpJsonOptionsModule : IModule
{
    public void ConfigureServices(IServiceCollection services, IConfiguration config, IHostEnvironment env)
    {
        services.ConfigureHttpJsonOptions(options => JsonOptions.Configure(options.SerializerOptions));
    }
}