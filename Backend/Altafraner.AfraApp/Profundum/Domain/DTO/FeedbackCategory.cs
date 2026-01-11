using Altafraner.AfraApp.Profundum.Domain.Models.Bewertung;

namespace Altafraner.AfraApp.Profundum.Domain.DTO;

/// <summary>
///     A DTO representing a feedback category
/// </summary>
public record FeedbackCategory
{
    /// <summary>
    ///     Initializes a dto from the db model
    /// </summary>
    public FeedbackCategory(ProfundumFeedbackKategorie kategorie)
    {
        Id = kategorie.Id;
        Label = kategorie.Label;
        Fachbereiche = kategorie.Fachbereiche.Select(k => new DtoProfundumFachbereich(k));
    }

    /// <summary>
    ///     The categories id
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    ///     The categories Label
    /// </summary>
    public string Label { get; set; }

    /// <summary>
    ///     The profundum fachbereiche this feedback category is for
    /// </summary>
    public IEnumerable<DtoProfundumFachbereich> Fachbereiche { get; set; }
}
