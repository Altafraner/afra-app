using Altafraner.Backbone.Abstractions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Altafraner.Backbone.Defaults.Submodules;

public class ReverseProxyHandlerModule : IModule
{
    public void ConfigureServices(IServiceCollection services, IConfiguration config, IHostEnvironment env)
    {
    }

    public void BeforeConfigure(WebApplication app)
    {
        app.UseForwardedHeaders(new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor
                               | ForwardedHeaders.XForwardedProto
                               | ForwardedHeaders.XForwardedHost,
            KnownNetworks =
            {
                IPNetwork.Parse("10.0.0.0/8"),
                IPNetwork.Parse("172.16.0.0/12"),
                IPNetwork.Parse("192.168.0.0/16"),
                IPNetwork.Parse("fc00::/7")
            }
        });
    }
}