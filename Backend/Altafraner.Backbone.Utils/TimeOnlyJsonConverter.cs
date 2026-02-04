using System.Text.Json;
using System.Text.Json.Serialization;

namespace Altafraner.Backbone.Utils;

/// <summary>
///     Converts a TimeOnly object to and from a string using the "t" format.
/// </summary>
/// <remarks>I hope they somewhen implement native support for this. Until then, this will do.</remarks>
public class TimeOnlyJsonConverter : JsonConverter<TimeOnly>
{
    /// <inheritdoc />
    public override TimeOnly Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        return TimeOnly.Parse(reader.GetString() ?? "0:0:0");
    }

    /// <inheritdoc />
    public override void Write(Utf8JsonWriter writer, TimeOnly value, JsonSerializerOptions options)
    {
        var time = value.ToString("t");
        writer.WriteStringValue(time);
    }
}
