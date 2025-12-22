using System.ComponentModel.DataAnnotations;

namespace Altafraner.AfraApp.Profundum.Domain.DTO;

/// <summary>
///     A request to create or change a feedback kategorie
/// </summary>
public record FeedbackKategorieChangeRequest
{
    /// <summary>
    ///     The categories label
    /// </summary>
    [MaxLength(200)]
    public required string Label { get; set; }

    /// <summary>
    ///     The profunda categories this feedback category applies to.
    /// </summary>
    [MinLength(1)]
    public required IEnumerable<Guid> Kategorien { get; set; }
}
