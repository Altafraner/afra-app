using System.ComponentModel.DataAnnotations;
using Altafraner.AfraApp.User.Domain.Models;

namespace Altafraner.AfraApp.Freistellung.Domain.Models;

/// <summary>
///     A leave request submitted by a student.
/// </summary>
public class Freistellungsantrag
{
    /// <summary>
    ///     The unique identifier of this request.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    ///     The student who submitted this request.
    /// </summary>
    public required Person Student { get; set; }

    /// <summary>
    ///     The foreign key of the student.
    /// </summary>
    public Guid StudentId { get; set; }

    /// <summary>
    ///     The first day of the requested leave period.
    /// </summary>
    public DateOnly DatumVon { get; set; }

    /// <summary>
    ///     The last day of the requested leave period (inclusive).
    ///     Equal to <see cref="DatumVon" /> for single-day requests.
    /// </summary>
    public DateOnly DatumBis { get; set; }

    /// <summary>
    ///     The reason for the leave.
    /// </summary>
    [MaxLength(1000)]
    public required string Grund { get; set; }

    /// <summary>
    ///     The current status of this request.
    /// </summary>
    public FreistellungsStatus Status { get; set; } = FreistellungsStatus.Gestellt;

    /// <summary>
    ///     The timestamp when this request was created.
    /// </summary>
    public DateTime ErstelltAm { get; set; } = DateTime.UtcNow;

    /// <summary>
    ///     The teacher decisions associated with this request.
    /// </summary>
    public List<LehrerEntscheidung> Entscheidungen { get; set; } = [];
}
