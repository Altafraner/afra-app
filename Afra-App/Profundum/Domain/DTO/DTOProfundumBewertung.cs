/// <summary>
/// A data transfer object representing a Profundum Bewertung.
/// </summary>
public class DTOProfundumBewertung
{
    /// <summary>
    /// The unique identifier for the Bewertung
    /// </summary>
    public Guid KriteriumId { get; set; }
    /// <summary>
    /// The unique identifier for the Bewertung Kriterium
    /// </summary>
    public Guid InstanzId { get; set; }
    /// <summary>
    /// An optional comment for the Bewertung
    /// </summary>
    public required int Grad { get; set; }
}
