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
    ///     A short summary of the reason for the leave.
    /// </summary>
    [MaxLength(200)]
    public required string Grund { get; set; }

    /// <summary>
    ///     The long reason for the leave.
    /// </summary>
    [MaxLength(1000)]
    public required string Beschreibung { get; set; }

    /// <summary>
    ///     The start of the requested leave period (date and time).
    /// </summary>
    public DateTime Von { get; set; }

    /// <summary>
    ///     The end of the requested leave period (date and time, inclusive).
    /// </summary>
    public DateTime Bis { get; set; }

    /// <summary>
    ///     The current status of this request.
    /// </summary>
    public FreistellungsStatus Status { get; set; } = FreistellungsStatus.Gestellt;

    /// <summary>
    ///     The timestamp when this request was created.
    /// </summary>
    public DateTime ErstelltAm { get; set; } = DateTime.UtcNow;

    /// <summary>
    ///     The individual lessons the student will miss during the leave period.
    /// </summary>
    public List<BetroffeneStunde> BetroffeneStunden { get; set; } = [];

    /// <summary>
    ///     The decisions associated with this request (both teachers and mentors).
    /// </summary>
    public List<LehrerEntscheidung> Entscheidungen { get; set; } = [];

    /// <summary>
    ///     An optional comment from the Sekretariat when rejecting the request.
    /// </summary>
    [MaxLength(500)]
    public string? SekretariatKommentar { get; set; }

    /// <summary>
    ///     An optional comment from the Schulleiter when rejecting the request.
    /// </summary>
    [MaxLength(500)]
    public string? SchulleiterKommentar { get; set; }
}
