using Afra_App.Profundum.Domain.Models;

/// <summary>
/// A db record representing a Profundum Anker.
/// </summary>
public class ProfundumAnker
{
    /// <summary>
    ///  The unique identifier for the Anker
    /// </summary>
    public required Guid Id { get; set; }
    
    /// <summary>
    /// The name of the Anker
    /// </summary>
    public required string Bezeichnung { get; set; }

    /// <summary>
    /// The category of the anchor
    /// </summary>
    public required ProfundumKategorie Kategorie { get; set; }
}
