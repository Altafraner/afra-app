namespace Altafraner.AfraApp.Profundum.Domain.Models;

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
    public ICollection<ProfundumTermin> Termine { get; set; }

    ///
    public override string ToString()
    {
        return $"{Jahr}-{Quartal.ToString()}-{Wochentag.ToString()}";
    }
}

///
public class ProfundumSlotComparer : IComparer<ProfundumSlot>, IEqualityComparer<ProfundumSlot>
{
    ///
    public int Compare(ProfundumSlot? slot1, ProfundumSlot? slot2)
        => (slot1, slot2) switch
        {
            (null, null) => 0,
            (null, _) => 1,
            (_, null) => -1,
            var (s1, s2) =>
                ((s1.Jahr * 10 + (int)s1.Quartal) * 10 + (int)s1.Wochentag)
                .CompareTo((s2.Jahr * 10 + (int)s2.Quartal) * 10 + (int)s2.Wochentag),
        };

    ///
    public bool Equals(ProfundumSlot? x, ProfundumSlot? y)
    {
        return Compare(x, y) == 0;
    }

    ///
    public int GetHashCode(ProfundumSlot obj)
    {
        return HashCode.Combine(obj.Jahr, obj.Quartal, obj.Wochentag);
    }
}
