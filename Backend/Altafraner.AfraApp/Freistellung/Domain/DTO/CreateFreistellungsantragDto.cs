using System.ComponentModel.DataAnnotations;

namespace Altafraner.AfraApp.Freistellung.Domain.DTO;

/// <summary>
///     The data required to create a new <see cref="Models.Freistellungsantrag" />.
/// </summary>
public record CreateFreistellungsantragDto
{
    /// <summary>
    ///     The date for which the leave is requested.
    /// </summary>
    public required DateOnly Datum { get; init; }

    /// <summary>
    ///     The reason for the leave.
    /// </summary>
    [MaxLength(1000)]
    public required string Grund { get; init; }

    /// <summary>
    ///     The IDs of the teachers whose lessons the student attends on the requested day.
    /// </summary>
    public required List<Guid> LehrerIds { get; init; }
}
