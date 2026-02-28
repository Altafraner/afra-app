using System.ComponentModel.DataAnnotations;

namespace Altafraner.AfraApp.Freistellung.Domain.DTO;

/// <summary>
///     The data required to create a new <see cref="Models.Freistellungsantrag" />.
/// </summary>
public record CreateFreistellungsantragDto
{
    /// <summary>
    ///     A short title summarising the reason for the leave.
    /// </summary>
    [MaxLength(200)]
    public required string Grund { get; init; }

    /// <summary>
    ///     The reason for the leave.
    /// </summary>
    [MaxLength(1000)]
    public required string Beschreibung { get; init; }

    /// <summary>
    ///     The start of the requested leave period (date and time).
    /// </summary>
    public required DateTime Von { get; init; }

    /// <summary>
    ///     The end of the requested leave period (date and time, inclusive).
    /// </summary>
    public required DateTime Bis { get; init; }

    /// <summary>
    ///     The individual lessons the student will miss during the leave period.
    ///     Each entry specifies the date, block number, subject, and teacher.
    /// </summary>
    public required List<CreateBetroffeneStundeDto> Stunden { get; init; }
}
