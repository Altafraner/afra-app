using System.Text.Json;
using System.Text.Json.Serialization;

namespace Altafraner.Backbone.Utils;

public class JsonOptions
{
    public static void Configure(JsonSerializerOptions options)
    {
        options.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.Converters.Add(new JsonStringEnumConverter());
        options.Converters.Add(new TimeOnlyJsonConverter());
    }
}
