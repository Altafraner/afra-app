using Altafraner.Backbone.Abstractions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Metrics;

namespace Altafraner.Backbone.Defaults;

/// <summary>
///     Adds observability
/// </summary>
public class PrometheusModule : IModule
{
    /// <inheritdoc />
    public void ConfigureServices(
        IServiceCollection services,
        IConfiguration config,
        IHostEnvironment env
    )
    {
        var otel = services.AddOpenTelemetry();

        otel.WithMetrics(metrics =>
            metrics
                .AddAspNetCoreInstrumentation()
                .AddRuntimeInstrumentation()
                .AddMeter("System.Net.Http")
                .AddMeter("System.Net.NameResolution")
                .AddMeter("Microsoft.AspNetCore.Hosting")
                .AddMeter("Microsoft.AspNetCore.Server.Kestrel")
                .AddPrometheusExporter()
        );
    }

    /// <inheritdoc />
    public void Configure(WebApplication app)
    {
        app.MapPrometheusScrapingEndpoint();
    }
}
