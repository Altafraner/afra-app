namespace Altafraner.AfraApp.Profundum.Domain.Models;

/// <summary>
///     a date a profundum happens at
/// </summary>
public class ProfundumTermin
{
    /// <summary>
    ///     The unique identifier of the profundum termin
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    ///     the slot this day is for
    /// </summary>
    public required ProfundumSlot Slot { get; set; }

    /// <summary>
    ///     the day the profundum happens at
    /// </summary>
    public required DateOnly Day { get; set; }

    /// <summary>
    ///     the start time on the day
    /// </summary>
    public required TimeOnly StartTime { get; set; }

    /// <summary>
    ///     the end time on the day
    /// </summary>
    public required TimeOnly EndTime { get; set; }

    /// <summary>
    ///     The lesson number. Mainly used for cevex sync.
    /// </summary>
    public int Lesson { get; set; }
}
