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

    ///
    [JsonIgnore]
    public ICollection<ProfundumSlot> Slots { get; init; } = [];

    ///
    public DateTime EinwahlStart { get; set; }

    ///
    public DateTime EinwahlStop { get; set; }

    ///
    public bool HasBeenMatched { get; set; } = false;
}
