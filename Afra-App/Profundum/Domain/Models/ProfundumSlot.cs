namespace Afra_App.Profundum.Domain.Models;

///
public enum ProfundumQuartal
{
    ///
    Q1,
    ///
    Q2,
    ///
    Q3,
    ///
    Q4,
}

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
    public required bool EinwahlMöglich { get; set; }

    ///
    public override string ToString()
    {
        return $"{Jahr}-{Quartal.ToString()}-{Wochentag.ToString()}";
    }
}
