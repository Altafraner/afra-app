using Altafraner.AfraApp.Schuljahr.Domain.Models;

namespace Altafraner.AfraApp.Otium.Domain.Models;

/// <summary>
///     A record representing a recurrence rule for an Otium.
/// </summary>
public class OtiumWiederholung : OtiumInstanz
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
    ///     The block the Otium is in.
    /// </summary>
    public required char Block { get; set; }

    /// <summary>
    ///     A list of all instances of the recurrence rule. Useful for bulk operations.
    /// </summary>
    public List<OtiumTermin> Termine { get; init; } = null!;

    /// <summary>
    ///     The date of the first Termin
    /// </summary>
    public DateOnly? StartDate =>
        Termine.Count != 0 ? Termine.Min(t => t.Block.Schultag.Datum) : null;

    /// <summary>
    ///     The date of the Last Termin
    /// </summary>
    public DateOnly? EndDate =>
        Termine.Count != 0 ? Termine.Max(t => t.Block.Schultag.Datum) : null;
}
