using Altafraner.Backbone.Abstractions;
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
        var telemetryConfig = config
            .GetSection("Telemetry")
            .Get<TelemetryConfiguration>();
        var traceConfig = telemetryConfig?.Tracing;

        var otel = services.AddOpenTelemetry()
            .ConfigureResource(r => r.AddService("afra-app"));

        if (traceConfig is not null)
        {
            var traceEndpoint = traceConfig.TraceEndpoint;
            var traceProto = traceConfig.TraceProtocol;
            otel.WithTracing(tracing => tracing
                    .AddOtlpExporter(config =>
                    {
                        config.Endpoint = traceEndpoint;
                        config.Protocol = traceProto;
                    }
                )
                .AddEntityFrameworkCoreInstrumentation()
                .AddQuartzInstrumentation()
                .AddAspNetCoreInstrumentation()
            );
        }

        otel.WithMetrics(metrics => metrics
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

/// <summary> General Telemetry configuration </summary>
public class TelemetryConfiguration
{
    /// <summary> tracing configuration </summary>
    public TraceConfiguration? Tracing { get; set; }
}

/// <summary> Configuration for tracing </summary>
public class TraceConfiguration
{
    /// <summary> Endpoint for exporting traces </summary>
    public required Uri TraceEndpoint { get; set; }

    /// <summary> protocol for exporting traces </summary>
    public required OpenTelemetry.Exporter.OtlpExportProtocol TraceProtocol { get; set; }
}
