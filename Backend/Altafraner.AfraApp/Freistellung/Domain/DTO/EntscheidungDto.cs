using System.ComponentModel.DataAnnotations;
using Altafraner.AfraApp.Freistellung.Domain.Models;

namespace Altafraner.AfraApp.Freistellung.Domain.DTO;

/// <summary>
///     The data required for a teacher to submit a decision on a <see cref="Models.Freistellungsantrag" />.
/// </summary>
public record EntscheidungDto
{
    /// <summary>
    ///     The teacher's decision.
    /// </summary>
    public required EntscheidungsStatus Status { get; init; }

    /// <summary>
    ///     An optional comment from the teacher.
    /// </summary>
    [MaxLength(500)]
    public string? Kommentar { get; init; }
}
