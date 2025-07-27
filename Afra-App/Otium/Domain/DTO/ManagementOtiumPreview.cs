namespace Afra_App.Otium.Domain.DTO;

/// <summary>
///     Contains a preview of an Otium for the management UI.
/// </summary>
public record ManagementOtiumPreview
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
    ///     A reference to the category of the Otium. Categories are transitive.
    /// </summary>
    public required Guid Kategorie { get; set; }

    /// <summary>
    ///     The number of Termine for the Otium
    /// </summary>
    public required int Termine { get; set; }
}
