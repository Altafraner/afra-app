using Afra_App.Otium.Domain.Models.Schuljahr;

namespace Afra_App.Otium.Domain.Models;

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
    ///     The block the Otium is in.
    /// </summary>
    public required char Block { get; set; }

    /// <summary>
    ///     A list of all instances of the recurrence rule. Useful for bulk operations.
    /// </summary>
    public ICollection<Termin> Termine { get; init; } = new List<Termin>();

    /// <summary>
    ///     The date of the first Termin
    /// </summary>
    public DateOnly? StartDate => Termine.Count != 0 ? Termine.Min(t => t.Block.Schultag.Datum) : null;

    /// <summary>
    ///     The date of the Last Termin
    /// </summary>
    public DateOnly? EndDate => Termine.Count != 0 ? Termine.Max(t => t.Block.Schultag.Datum) : null;
}
