using Afra_App.Otium.Domain.Models.Schuljahr;

namespace Afra_App.Otium.Domain.Models;

/// <summary>
///     A record representing a one-time instance of an Otium.
/// </summary>
public class Termin : OtiumsInstanz
{
    /// <summary>
    ///     A unique identifier for the Termin
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    ///     A reference to the recurrence rule for the Termin. May be null.
    /// </summary>
    public Wiederholung? Wiederholung { get; set; }

    /// <summary>
    ///     A reference to the Block the Termin is on.
    /// </summary>
    public required Block Block { get; set; }

    /// <summary>
    ///     A reference to all enrollments for the Termin.
    /// </summary>
    public ICollection<Einschreibung> Enrollments { get; set; } = new List<Einschreibung>();

    /// <summary>
    ///     True, if the Termin is cancelled.
    /// </summary>
    public bool IstAbgesagt { get; set; }

    /// <summary>
    ///     True, iff a supervisor has checked the attendance for this Termin.
    /// </summary>
    public bool SindAnwesenheitenKontrolliert { get; set; } = false;
}