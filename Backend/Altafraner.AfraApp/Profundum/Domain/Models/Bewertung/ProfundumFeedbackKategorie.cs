using System.ComponentModel.DataAnnotations;

namespace Altafraner.AfraApp.Profundum.Domain.Models.Bewertung;

/// <summary>
///     A kategorie for
/// </summary>
public class ProfundumFeedbackKategorie
{
    /// <summary>
    ///     The unique id of the kategorie
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    ///     The categories label
    /// </summary>
    [MaxLength(200)]
    public required string Label { get; set; }

    /// <summary>
    ///     The profundum categories this feedback category applies for
    /// </summary>
    public List<ProfundumFachbereich> Fachbereiche { get; set; } = [];
}
