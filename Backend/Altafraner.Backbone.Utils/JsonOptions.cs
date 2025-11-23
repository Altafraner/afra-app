using System.Text.Json;
using System.Text.Json.Serialization;

namespace Altafraner.Backbone.Utils;

/// <summary>
///     Utilities for configuring the JSON Serializer
/// </summary>
public static class JsonOptions
{
    /// <summary>
    ///     Configures the json serializer
    /// </summary>
    public static void Configure(JsonSerializerOptions options)
    {
        options.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.Converters.Add(new JsonStringEnumConverter());
        options.Converters.Add(new TimeOnlyJsonConverter());
    }
}
