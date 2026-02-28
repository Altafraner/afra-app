using System.ComponentModel.DataAnnotations;

namespace Altafraner.AfraApp.Freistellung.Domain.DTO;

/// <summary>
///     The data for a single lesson entry submitted as part of a
///     <see cref="CreateFreistellungsantragDto" />.
/// </summary>
public record CreateBetroffeneStundeDto
{
    /// <summary>
    ///     The date of the lesson (must fall within the leave period).
    /// </summary>
    public required DateOnly Datum { get; init; }

    /// <summary>
    ///     The lesson block number (Unterrichtsblock), e.g. 1–8.
    /// </summary>
    [Range(1, 12)]
    public required int Block { get; init; }

    /// <summary>
    ///     The subject (Unterrichtsfach) of the lesson.
    /// </summary>
    [MaxLength(200)]
    public required string Fach { get; init; }

    /// <summary>
    ///     The ID of the teacher who teaches this lesson.
    /// </summary>
    public required Guid LehrerId { get; init; }
}
