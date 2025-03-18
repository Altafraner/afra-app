using System.Text.Json.Serialization;

namespace Afra_App.Data.Schuljahr;

/// <summary>
///     The type of week the recurrence rule applies to.
/// </summary>
public enum Wochentyp
{
    /// <summary>
    ///     A week usually containing the start or end of a holiday
    /// </summary>
    [JsonStringEnumMemberName("H-Woche")] H,

    /// <summary>
    ///     A normal week
    /// </summary>
    [JsonStringEnumMemberName("N-Woche")] N
}