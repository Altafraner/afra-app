using System.ComponentModel.DataAnnotations;

namespace Altafraner.AfraApp.Freistellung.Domain.DTO;

/// <summary>
///     The data required to create a new <see cref="Models.Freistellungsantrag" />.
/// </summary>
public record CreateFreistellungsantragDto
{
    /// <summary>
    ///     The first day of the requested leave period.
    /// </summary>
    public required DateOnly DatumVon { get; init; }

    /// <summary>
    ///     The last day of the requested leave period (inclusive).
    ///     Equal to <see cref="DatumVon" /> for single-day requests.
    /// </summary>
    public required DateOnly DatumBis { get; init; }

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
