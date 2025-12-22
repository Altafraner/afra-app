using System.ComponentModel.DataAnnotations;

namespace Altafraner.AfraApp.Profundum.Domain.DTO;

/// <summary>
///     A request to change or create a profunda feedback anchor.
/// </summary>
public record AnkerChangeRequest
{
    /// <summary>
    ///     The anchors new label
    /// </summary>
    [MaxLength(200)]
    public required string Label { get; set; }

    /// <summary>
    ///     The anchors new kategorie
    /// </summary>
    public required Guid KategorieId { get; set; }
}
