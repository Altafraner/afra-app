using System.ComponentModel.DataAnnotations;
using Altafraner.AfraApp.User.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Altafraner.AfraApp.Profundum.Domain.Models.Bewertung;

/// <summary>
///     A db record representing a Profundum Bewertung.
/// </summary>
[PrimaryKey(nameof(AnkerId), nameof(InstanzId), nameof(BetroffenePersonId))]
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
    ///     The profundum instanz the Bewertung is for
    /// </summary>
    public Guid InstanzId { get; set; }

    /// <summary>
    ///     The actual Profundum Instanz
    /// </summary>
    public ProfundumInstanz Instanz { get; set; } = null!;

    /// <summary>
    ///     The person that received the Bewertung
    /// </summary>
    public Guid BetroffenePersonId { get; set; }

    /// <summary>
    ///     The actual person that received the Bewertung
    /// </summary>
    public Person BetroffenePerson { get; set; } = null!;

    /// <summary>
    ///     The grade given for the Kriterium
    /// </summary>
    [Range(1, 5)]
    public int Grad { get; set; }
}
