namespace Afra_App.Profundum.Domain.Models;

/// <summary>
///     A db record representing a Profundum.
/// </summary>
public record ProfundumSlot
{
    /// <summary>
    ///     A unique identifier for the Profundum Slot
    /// </summary>
    public Guid Id { get; set; }

    ///
    public required int Jahr { get; set; }

    ///
    public required ProfundumQuartal Quartal { get; set; }

    ///
    public required DayOfWeek Wochentag { get; set; }

    ///
    public required ProfundumEinwahlZeitraum EinwahlZeitraum { get; set; }

    ///
    public override string ToString()
    {
        return $"{Jahr}-{Quartal.ToString()}-{Wochentag.ToString()}";
    }
}

///
public class ProfundumSlotComparer : IComparer<ProfundumSlot>
{
    ///
    public int Compare(ProfundumSlot? slot1, ProfundumSlot? slot2)
    => (slot1, slot2) switch
    {
        (null, null) => 0,
        (null, var s2) => 1,
        (var s1, null) => -1,
        (var s1, var s2) =>
        ((s1.Jahr * 10 + (int)s1.Quartal) * 10 + (int)s1.Wochentag)
        .CompareTo((s2.Jahr * 10 + (int)s2.Quartal) * 10 + (int)s2.Wochentag),
    };
}
