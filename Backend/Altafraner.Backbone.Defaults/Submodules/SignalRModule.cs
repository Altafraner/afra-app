using Altafraner.Backbone.Abstractions;
using Altafraner.Backbone.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Altafraner.Backbone.Defaults;

public class SignalRModule : IModule
{
    public void ConfigureServices(IServiceCollection services, IConfiguration config, IHostEnvironment env)
    {
        services.AddSignalR()
            .AddJsonProtocol(options => JsonOptions.Configure(options.PayloadSerializerOptions));
    }
}