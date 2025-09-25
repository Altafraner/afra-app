/// <summary>
///  A db record representing a Profundum Bewertung Kriterium.
/// </summary>
public class ProfundumsBewertungKriterium
{
    /// <summary>
    ///  The unique identifier for the Bewertung Kriterium
    /// </summary>
    public Guid Id { get; set; }
    /// <summary>
    ///  A name for the Bewertung Kriterium
    /// </summary>
    public required string Bezeichnung { get; set; }
}    
