using System.ComponentModel.DataAnnotations;

namespace Altafraner.AfraApp.Profundum.Domain.Models.Bewertung;

/// <summary>
///     A db record representing a Profundum Bewertung.
/// </summary>
public class ProfundumFeedbackEntry
{
    /// <summary>
    ///     The unique identifier for the Bewertung
    /// </summary>
    public Guid AnkerId { get; set; }

    /// <summary>
    ///     The actual Kriterium
    /// </summary>
    public ProfundumFeedbackAnker Anker { get; set; } = null!;

    /// <summary>
    ///     The actual Profundum enrollment.
    /// </summary>
    public ProfundumEinschreibung Einschreibung { get; set; } = null!;

    /// <summary>
    ///     The ID of the slot for the enrollment (part of composite key)
    /// </summary>
    protected internal Guid SlotId { get; set; }

    /// <summary>
    ///     The ID of the person affected by the enrollment (part of composite key)
    /// </summary>
    protected internal Guid BetroffenePersonId { get; set; }

    /// <summary>
    ///     The grade given for the Kriterium
    /// </summary>
    [Range(1, 5)]
    public int Grad { get; set; }
}
