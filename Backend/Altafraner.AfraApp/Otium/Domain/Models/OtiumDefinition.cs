using System.ComponentModel.DataAnnotations;
using Altafraner.AfraApp.User.Domain.Models;
using Altafraner.Backbone.Utils;

namespace Altafraner.AfraApp.Otium.Domain.Models;

/// <summary>
///     A db record representing an Otium.
/// </summary>
public class OtiumDefinition : IHasTimestamps
{
    /// <summary>
    ///     A unique identifier for the Otium
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    ///     A name for the Otium
    /// </summary>
    [MaxLength(70)]
    public required string Bezeichnung { get; set; }

    /// <summary>
    ///     A description for the Otium. May contain multiple lines.
    /// </summary>
    [MaxLength(500)]
    public required string Beschreibung { get; set; }

    /// <summary>
    ///     A reference to the category of the Otium. Categories are transitive.
    /// </summary>
    public required OtiumKategorie Kategorie { get; set; }

    /// <summary>
    ///     A list of all people responsible for the Otium.
    /// </summary>
    public ICollection<Person> Verantwortliche { get; set; } = null!;

    /// <summary>
    ///     All recurrence rules for the Otium
    /// </summary>
    public ICollection<OtiumWiederholung> Wiederholungen { get; set; } = null!;

    /// <summary>
    ///     All instances of the Otium
    /// </summary>
    public ICollection<OtiumTermin> Termine { get; set; } = null!;

    /// <inheritdoc/>
    public DateTime CreatedAt { get; set; }

    /// <inheritdoc/>
    public DateTime LastModified { get; set; }

    /// <summary>
    ///     The youngest grade allowed enrollment to this Otium. Null for no lower limit
    /// </summary>
    public int? MinKlasse { get; set; } = null;

    /// <summary>
    ///     The oldest grade allowed enrollment to this Otium. Null for no upper limit
    /// </summary>
    public int? MaxKlasse { get; set; } = null;
}
