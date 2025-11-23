using Altafraner.Backbone.Abstractions;
using Altafraner.Backbone.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Altafraner.Backbone.Defaults;

/// <summary>
///     Configures the json options for minimal apis
/// </summary>
public class HttpJsonOptionsModule : IModule
{
    /// <inheritdoc />
    public void ConfigureServices(IServiceCollection services, IConfiguration config, IHostEnvironment env)
    {
        services.ConfigureHttpJsonOptions(options => JsonOptions.Configure(options.SerializerOptions));
    }
}
