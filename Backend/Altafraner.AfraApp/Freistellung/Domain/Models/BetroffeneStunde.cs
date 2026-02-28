using System.ComponentModel.DataAnnotations;
using Altafraner.AfraApp.User.Domain.Models;

namespace Altafraner.AfraApp.Freistellung.Domain.Models;

/// <summary>
///     A single lesson that a student would miss during a leave period.
/// </summary>
public class BetroffeneStunde
{
    /// <summary>
    ///     The unique identifier of this entry.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    ///     The leave request this entry belongs to.
    /// </summary>
    public required Freistellungsantrag Freistellungsantrag { get; set; }

    /// <summary>
    ///     The foreign key of the leave request.
    /// </summary>
    public Guid FreistellungsantragId { get; set; }

    /// <summary>
    ///     The date of this lesson (must fall within the leave period).
    /// </summary>
    public DateOnly Datum { get; set; }

    /// <summary>
    ///     The lesson block number (Unterrichtsblock)
    /// </summary>
    public int Block { get; set; }

    /// <summary>
    ///     The subject (Unterrichtsfach) of the lesson.
    /// </summary>
    [MaxLength(200)]
    public required string Fach { get; set; }

    /// <summary>
    ///     The teacher who teaches this lesson.
    /// </summary>
    public required Person Lehrer { get; set; }

    /// <summary>
    ///     The foreign key of the teacher.
    /// </summary>
    public Guid LehrerId { get; set; }
}
