namespace Afra_App.Otium.Domain.DTO;

/// <summary>
///     A DTO for the creation of an Otium
/// </summary>
public record ManagementOtiumCreation
{
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
}