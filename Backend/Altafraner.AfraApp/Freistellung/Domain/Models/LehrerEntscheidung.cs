using System.ComponentModel.DataAnnotations;
using Altafraner.AfraApp.User.Domain.Models;

namespace Altafraner.AfraApp.Freistellung.Domain.Models;

/// <summary>
///     A teacher's decision on a <see cref="Freistellungsantrag" />.
/// </summary>
public class LehrerEntscheidung
{
    /// <summary>
    ///     The unique identifier of this decision.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    ///     The leave request this decision belongs to.
    /// </summary>
    public required Freistellungsantrag Freistellungsantrag { get; set; }

    /// <summary>
    ///     The foreign key of the leave request.
    /// </summary>
    public Guid FreistellungsantragId { get; set; }

    /// <summary>
    ///     The teacher who made this decision.
    /// </summary>
    public required Person Lehrer { get; set; }

    /// <summary>
    ///     The foreign key of the teacher.
    /// </summary>
    public Guid LehrerId { get; set; }

    /// <summary>
    ///     The current status of this decision.
    /// </summary>
    public EntscheidungsStatus Status { get; set; } = EntscheidungsStatus.Ausstehend;

    /// <summary>
    ///     An optional comment from the teacher.
    /// </summary>
    [MaxLength(500)]
    public string? Kommentar { get; set; }

    /// <summary>
    ///     The timestamp when the decision was made.
    /// </summary>
    public DateTime? EntschiedenAm { get; set; }
}
