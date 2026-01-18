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
}
