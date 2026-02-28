using System.ComponentModel.DataAnnotations;
using Altafraner.AfraApp.User.Domain.Models;

namespace Altafraner.AfraApp.Freistellung.Domain.Models;

/// <summary>
///     A single logged transition in the lifecycle of a <see cref="Freistellungsantrag" />. This is
///     the append-only audit trail for the request — <see cref="Freistellungsantrag.Status" /> is
///     always changed together with adding one of these, so the current status is simply the
///     status of the most recent entry.
/// </summary>
public class FreistellungsVerlaufEintrag
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
    ///     When this transition happened.
    /// </summary>
    public DateTime Zeitpunkt { get; set; } = DateTime.UtcNow;

    /// <summary>
    ///     The person who caused this transition. <c>null</c> for transitions triggered
    ///     automatically by the system (e.g. once every teacher/mentor has responded).
    /// </summary>
    public Person? Person { get; set; }

    /// <summary>
    ///     The foreign key of <see cref="Person" />.
    /// </summary>
    public Guid? PersonId { get; set; }

    /// <summary>
    ///     The status the request transitioned to.
    /// </summary>
    public FreistellungsStatus NeuerStatus { get; set; }

    /// <summary>
    ///     An optional comment attached to this transition (e.g. the Sekretariat's hint about a
    ///     missing Elternbestätigung, or the Schulleiter's rejection reason).
    /// </summary>
    [MaxLength(500)]
    public string? Kommentar { get; set; }
}
