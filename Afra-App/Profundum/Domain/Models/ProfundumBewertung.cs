using Afra_App.Profundum.Domain.Models;
using Afra_App.User.Domain.Models;

/// <summary>
///  A db record representing a Profundum Bewertung.
/// </summary>
public class ProfundumBewertung
{
    /// <summary>
    ///  The unique identifier for the Bewertung
    /// </summary>
    public required Guid Id { get; set; }
    /// <summary>
    ///   The unique identifier for the Bewertung
    /// </summary>
    public Guid KriteriumId { get; set; }
    /// <summary>
    ///   The actual Kriterium 
    /// </summary>
    public required ProfundumsBewertungKriterium Kriterium { get; set; }
    /// <summary> 
    ///   The profundum instanz the Bewertung is for
    /// </summary>
    public Guid InstanzId { get; set; }
    /// <summary>
    ///   The actual Profundum Instanz
    /// </summary>
    public required ProfundumInstanz Instanz { get; set; }
    /// <summary>
    ///   The person that received the Bewertung
    /// </summary>
    public Guid BetroffenePersonId { get; set; }
    /// <summary>
    ///  The actual person that received the Bewertung
    /// </summary>
    public required Person BetroffenePerson { get; set; }
    /// <summary>
    ///   The grade given for the Kriterium
    /// </summary>
    public int Grad { get; set; }
}

