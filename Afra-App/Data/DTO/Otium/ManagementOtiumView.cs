using System.Diagnostics.CodeAnalysis;

namespace Afra_App.Data.DTO.Otium;

/// <summary>
///     A DTO for the view of an Otium in the management ui
/// </summary>
public record ManagementOtiumView
{
    /// Construct an empty ManagementOtiumView
    public ManagementOtiumView()
    {
    }

    /// Construct a ManagementOtiumView from a Database Otium
    [SetsRequiredMembers]
    public ManagementOtiumView(Data.Otium.Otium dbOtium)
    {
        Id = dbOtium.Id;
        Bezeichnung = dbOtium.Bezeichnung;
        Beschreibung = dbOtium.Beschreibung;
        Kategorie = dbOtium.Kategorie.Id;
        Verantwortliche = dbOtium.Verantwortliche.Select(v => new PersonInfoMinimal(v));
        Termine = dbOtium.Termine.Select(t => new ManagementTerminView(t));
        Wiederholungen = dbOtium.Wiederholungen.Select(r => new ManagementWiederholungView(r));
    }

    /// <summary>
    ///     A unique identifier for the Otium
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    ///     A name for the Otium
    /// </summary>
    public required string Bezeichnung { get; set; }

    /// <summary>
    ///     A description for the Otium. May contain multiple lines.
    /// </summary>
    public required string Beschreibung { get; set; }

    /// <summary>
    ///     A reference to the category of the Otium. Categories are transitive.
    /// </summary>
    public required Guid Kategorie { get; set; }

    /// <summary>
    ///     A list of all people responsible for the Otium.
    /// </summary>
    public required IEnumerable<PersonInfoMinimal> Verantwortliche { get; set; }

    /// <summary>
    ///     All instances of the Otium
    /// </summary>
    public IEnumerable<ManagementTerminView>? Termine { get; set; }

    /// <summary>
    ///     All repetition rules of the Otium
    /// </summary>
    public IEnumerable<ManagementWiederholungView>? Wiederholungen { get; set; }
}