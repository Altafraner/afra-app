using System.Text.Json.Serialization;

namespace Altafraner.AfraApp.Profundum.Domain.Models;

/// <summary>
///     A db record representing a Profundum Einwahlzeitraum.
/// </summary>
public class ProfundumEinwahlZeitraum
{
    /// <summary>
    ///     A unique identifier for the Profundum Einwahlzeitraum
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    ///     The slots contained in this enrollment timeframe
    /// </summary>
    [JsonIgnore]
    public ICollection<ProfundumSlot> Slots { get; init; } = [];

    /// <summary>
    ///     the date students are alowed to register their wishes from
    /// </summary>
    public DateTime EinwahlStart { get; set; }

    /// <summary>
    ///     the date by which the wishes have to be submitted.
    /// </summary>
    public DateTime EinwahlStop { get; set; }

    /// <summary>
    ///     An admin-settable display name for this Einwahlzeitraum
    /// </summary>
    public string Bezeichnung { get; set; } = "";

    /// <summary>
    ///     The date/time from which students may see their matched Profundum Termine in their personal
    ///     calendar/dashboard. <c>null</c> means not yet published - matched enrollments exist but stay hidden from
    ///     students until staff explicitly set this. Doesn't affect what supervising Tutoren see; they know their
    ///     own teaching schedule regardless of publication state.
    /// </summary>
    public DateTime? Veroeffentlichungsdatum { get; set; }
}
