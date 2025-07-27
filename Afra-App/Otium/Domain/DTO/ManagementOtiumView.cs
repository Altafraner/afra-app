using Afra_App.User.Domain.DTO;

namespace Afra_App.Otium.Domain.DTO;

/// <summary>
///     A DTO for the view of an Otium in the management ui
/// </summary>
public record ManagementOtiumView
{
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
