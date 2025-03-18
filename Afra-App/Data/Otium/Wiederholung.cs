using Afra_App.Data.Schuljahr;

namespace Afra_App.Data.Otium;

/// <summary>
///     A record representing a recurrence rule for an Otium.
/// </summary>
public class Wiederholung : OtiumsInstanz
{
    /// <summary>
    ///     The unique identifier of the recurrence rule.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    ///     The day of week the recurrence rule applies to.
    /// </summary>
    public required DayOfWeek Wochentag { get; set; }

    /// <summary>
    ///     The type of week the recurrence rule applies to.
    /// </summary>
    public required Wochentyp Wochentyp { get; set; }

    /// <summary>
    ///     A list of all instances of the recurrence rule. Useful for bulk operations.
    /// </summary>
    public ICollection<Termin> Termine { get; init; } = new List<Termin>();
}