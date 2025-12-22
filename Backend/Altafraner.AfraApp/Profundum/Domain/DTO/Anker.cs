using Altafraner.AfraApp.Profundum.Domain.Models.Bewertung;

namespace Altafraner.AfraApp.Profundum.Domain.DTO;

/// <summary>
///     A DTO representing a feedback anchor
/// </summary>
public record Anker
{
    /// <summary>
    ///     The anchors id
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    ///     The anchors label
    /// </summary>
    public string Label { get; set; }

    /// <summary>
    ///     the anchors associated feedback category
    /// </summary>
    public Guid KategorieId { get; set; }

    /// <summary>
    ///     Initializes a dto from the db model
    /// </summary>
    public Anker(ProfundumFeedbackAnker anker)
    {
        Id = anker.Id;
        Label = anker.Label;
        KategorieId = anker.Kategorie.Id;
    }
}
