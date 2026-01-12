namespace Altafraner.AfraApp.Profundum.Domain.DTO;

/// <summary>
///     A dto containing an overview of all feedback anchors
/// </summary>
public record AnkerOverview
{
    /// <summary>
    ///     all anchors, sorted by kategorie
    /// </summary>
    public required Dictionary<Guid, IEnumerable<Anker>> AnkerByKategorie { get; set; }

    /// <summary>
    ///     all categories
    /// </summary>
    public required IEnumerable<FeedbackCategory> Kategorien { get; set; }
}
