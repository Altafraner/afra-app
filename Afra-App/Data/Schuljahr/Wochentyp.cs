using System.Text.Json.Serialization;

namespace Afra_App.Data.Schuljahr;

public enum Wochentyp
{
    [JsonStringEnumMemberName("H-Woche")]
    H,
    [JsonStringEnumMemberName("N-Woche")]
    N
}