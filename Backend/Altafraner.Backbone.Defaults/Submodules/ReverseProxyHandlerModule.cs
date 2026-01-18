using Altafraner.Backbone.Abstractions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using IPNetwork = System.Net.IPNetwork;

namespace Altafraner.Backbone.Defaults;

/// <summary>
///     Adds middleware to handle local network traffic via reverse proxy
/// </summary>
public class ReverseProxyHandlerModule : IModule
{
    /// <inheritdoc />
    public void ConfigureServices(IServiceCollection services, IConfiguration config, IHostEnvironment env)
    {
    }

    /// <inheritdoc />
    public void RegisterMiddleware(WebApplication app)
    {
        app.UseForwardedHeaders(new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor
                               | ForwardedHeaders.XForwardedProto
                               | ForwardedHeaders.XForwardedHost,
            KnownIPNetworks =
            {
                IPNetwork.Parse("10.0.0.0/8"),
                IPNetwork.Parse("172.16.0.0/12"),
                IPNetwork.Parse("192.168.0.0/16"),
                IPNetwork.Parse("fc00::/7")
            }
        });
    }
}
