namespace Altafraner.AfraApp.User.Domain.Models;

/// <summary>
///     An append-only log entry recording that a <see cref="Person" />'s <see cref="Person.Gruppe" /> became a given
///     value as of a given point in time. Used to answer "what was this person's grade at some point in the past,"
///     since <see cref="Person.Gruppe" /> itself only ever reflects the current value.
/// </summary>
public class PersonGruppenHistorie
{
    /// <summary>
    ///     The unique identifier of this history entry.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    ///     The person this entry is about.
    /// </summary>
    public required Person Person { get; set; }

    /// <summary>
    ///     The id of <see cref="Person" />.
    /// </summary>
    protected internal Guid PersonId { get; set; }

    /// <summary>
    ///     The value <see cref="Person.Gruppe" /> took on as of <see cref="GueltigAb" />.
    /// </summary>
    public string? Gruppe { get; set; }

    /// <summary>
    ///     The point in time this Gruppe value took effect.
    /// </summary>
    public required DateTime GueltigAb { get; set; }
}
