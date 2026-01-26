using Altafraner.AfraApp.Domain.Configuration;
using Altafraner.Backbone.Abstractions;

namespace Altafraner.AfraApp;

internal class GeneralConfigurationModule : IModule
{
    public void ConfigureServices(IServiceCollection services, IConfiguration config, IHostEnvironment env)
    {
        services.AddOptions<GeneralConfiguration>()
            .Bind(config.GetSection("General"));
    }
}
