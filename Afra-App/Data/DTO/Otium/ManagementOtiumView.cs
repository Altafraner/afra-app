using System.Diagnostics.CodeAnalysis;

namespace Afra_App.Data.DTO.Otium;

/// <summary>
///     A DTO for the view of an Otium in the management ui
/// </summary>
public record ManagementOtiumView
{
    /// Construct an empty ManagementOtiumView
    public ManagementOtiumView() { }

    /// Construct a ManagementOtiumView from a Database Otium
    [SetsRequiredMembers]
    public ManagementOtiumView(Data.Otium.Otium db_otium)
    {
        Id = db_otium.Id;
        Bezeichnung = db_otium.Bezeichnung;
        Beschreibung = db_otium.Beschreibung;
        Kategorie = db_otium.Kategorie.Id;
        Verantwortliche = db_otium.Verantwortliche.Select(v => new Data.DTO.PersonInfoMinimal(v));
        Termine = db_otium.Termine.Select(t => new ManagementTerminView(t));
        Wiederholungen = db_otium.Wiederholungen.Select(r => new ManagementWiederholungView(r));
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