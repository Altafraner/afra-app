using System.ComponentModel.DataAnnotations;
using Afra_App.Data.People;

namespace Afra_App.Data.Otium;

/// <summary>
/// A db record representing an Otium.
/// </summary>
public class Otium
{
    /// <summary>
    /// A unique identifier for the Otium
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// A name for the Otium
    /// </summary>
    [MaxLength(50)]
    public required string Bezeichnung { get; set; }

    /// <summary>
    /// A description for the Otium. May contain multiple lines.
    /// </summary>
    [MaxLength(500)]
    public required string Beschreibung { get; set; }

    /// <summary>
    /// A reference to the category of the Otium. Categories are transitive.
    /// </summary>
    public required Kategorie Kategorie { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public ICollection<Person> Verantwortliche { get; set; } = new List<Person>();

    /// <summary>
    /// All recurrence rules for the Otium
    /// </summary>
    public ICollection<Wiederholung> Wiederholungen { get; set; } = new List<Wiederholung>();

    /// <summary>
    /// All instances of the Otium
    /// </summary>
    public ICollection<Termin> Termine { get; set; } = new List<Termin>();
}