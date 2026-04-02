using OpenTelemetry.Exporter;

namespace Altafraner.Backbone.Defaults.Configuration;

/// <summary> General Telemetry configuration </summary>
public class TelemetryConfiguration
{
    /// <summary> tracing configuration </summary>
    public TraceConfiguration? Tracing { get; set; }

    /// <summary> Configuration for tracing </summary>
    public class TraceConfiguration
    {
        /// <summary> Endpoint for exporting traces </summary>
        public required Uri TraceEndpoint { get; set; }

        /// <summary> protocol for exporting traces </summary>
        public required OtlpExportProtocol TraceProtocol { get; set; }
    }
}
