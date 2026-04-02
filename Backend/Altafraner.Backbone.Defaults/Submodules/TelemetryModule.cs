using Altafraner.Backbone.Abstractions;
using Altafraner.Backbone.Defaults.Configuration;
using Altafraner.Backbone.Utils;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Altafraner.Backbone.Defaults;

/// <summary>
///     Adds telemetry
/// </summary>
public class TelemetryModule : IModule
{
    /// <inheritdoc />
    public void ConfigureServices(IServiceCollection services, IConfiguration config, IHostEnvironment env)
    {
        var otel = services.AddOpenTelemetry()
            .ConfigureResource(r => r.AddService("afra-app"));

        var traceConfig = ConfigHelper.GetAndRegisterConfig<TelemetryConfiguration>(services, config, "Telemetry")
            .Tracing;

        if (traceConfig is not null)
        {
            var traceEndpoint = traceConfig.TraceEndpoint;
            var traceProto = traceConfig.TraceProtocol;
            otel.WithTracing(tracing => tracing
                .AddOtlpExporter(exporterOptions =>
                    {
                        exporterOptions.Endpoint = traceEndpoint;
                        exporterOptions.Protocol = traceProto;
                    }
                )
                .AddEntityFrameworkCoreInstrumentation()
                .AddQuartzInstrumentation()
                .AddAspNetCoreInstrumentation()
                .AddSource("Altafraner.*")
            );
        }

        otel.WithMetrics(metrics => metrics
            .AddAspNetCoreInstrumentation()
            .AddRuntimeInstrumentation()
            .AddMeter("System.Net.Http")
            .AddMeter("System.Net.NameResolution")
            .AddMeter("Altafraner.*")
            .AddPrometheusExporter()
        );
    }

    /// <inheritdoc />
    public void Configure(WebApplication app)
    {
        app.MapPrometheusScrapingEndpoint();
    }
}
