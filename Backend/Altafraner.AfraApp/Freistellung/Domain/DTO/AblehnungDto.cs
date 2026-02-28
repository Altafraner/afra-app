using System.ComponentModel.DataAnnotations;

namespace Altafraner.AfraApp.Freistellung.Domain.DTO;

/// <summary>
///     The data required when the Sekretariat or Schulleiter rejects a leave request.
/// </summary>
public record AblehnungDto
{
    /// <summary>
    ///     A comment explaining what needs to be fixed or why the request was rejected.
    /// </summary>
    [Required]
    [MaxLength(500)]
    public required string Kommentar { get; init; }
}
