using Afra_App.Profundum.Domain.Models;
using Afra_App.User.Domain.Models;

/// <summary>
/// A db record representing ProfundumAnkerBewertungen
/// </summary>
public class ProfundumAnkerBewertung
{

    /// <summary>
    /// Unique Identifier
    /// </summary>
    public required Guid Id { get; set; }
    
    /// <summary>
    /// The rated person ID
    /// </summary>
    public required Guid BewertetePersonId { get; set; }
    
    /// <summary>
    /// The rated person
    /// </summary>
    public required Person BewertetePerson { get; set; } = null!;
    
    /// <summary>
    ///  The rated Profundum instance
    /// </summary>
    public required ProfundumInstanz Profundum { get; set; }
    
    /// <summary>
    ///  The rated Anker
    /// </summary>
    public required ProfundumAnker Anker { get; set; }

    /// <summary>
    ///  The Kriterium for the Anker
    /// </summary>
    public required ProfundumsBewertungKriterium Kriterium { get; set; }

    /// <summary>
    ///  Feedback for the Anker in the correct Profundum (1-5)
    /// </summary>
    public required int Bewertung { get; set; }
}
