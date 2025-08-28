using System.ComponentModel.DataAnnotations;
using Afra_App.User.Domain.Models;
using Afra_App.Backbone.Database.Contracts;

namespace Afra_App.Otium.Domain.Models;

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
    public ICollection<Person> Verantwortliche { get; set; } = new List<Person>();

    /// <summary>
    ///     All recurrence rules for the Otium
    /// </summary>
    public ICollection<OtiumWiederholung> Wiederholungen { get; set; } = new List<OtiumWiederholung>();

    /// <summary>
    ///     All instances of the Otium
    /// </summary>
    public ICollection<OtiumTermin> Termine { get; set; } = new List<OtiumTermin>();

    /// <inheritdoc/>
    public DateTime CreatedAt { get; set; }

    /// <inheritdoc/>
    public DateTime LastModified { get; set; }
}
