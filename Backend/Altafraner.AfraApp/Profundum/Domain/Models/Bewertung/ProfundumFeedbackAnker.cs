using System.ComponentModel.DataAnnotations;

namespace Altafraner.AfraApp.Profundum.Domain.Models.Bewertung;

/// <summary>
///     A db record representing a Profundum Anker.
/// </summary>
public class ProfundumFeedbackAnker
{
    /// <summary>
    ///     The unique identifier for the Anker
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    ///     The name of the Anker
    /// </summary>
    [MaxLength(200)]
    public required string Label { get; set; }

    /// <summary>
    ///     The category of the anchor
    /// </summary>
    public required ProfundumFeedbackKategorie Kategorie { get; set; }
}
