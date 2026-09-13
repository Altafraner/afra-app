namespace Altafraner.AfraApp.Profundum.Domain.DTO;

/// <summary>
///     encodes the successfullness of the matching
/// </summary>
public enum MatchingResultStatus
{
    /// <summary>
    ///     the matching was successful
    /// </summary>
    MatchingComplete,

    /// <summary>
    ///     the matching was not successful
    /// </summary>
    MatchingIncomplete,
}

///
public record MatchingStats
{
    /// <summary>
    ///     The time needed to perform the matching calculations
    /// </summary>
    public required double CalculationTime { get; set; }

    /// <summary>
    ///     The result status
    /// </summary>
    public MatchingResultStatus Result { get; set; }

    /// <summary>
    ///     The number of students who ended up unenrolled in at least one non-fixed slot despite having submitted a
    ///     valid set of Belegwuensche. A high number here usually indicates a capacity shortage.
    /// </summary>
    public int NichtEingeschriebenTrotzWunsch { get; set; }

    /// <summary>
    ///     A histogram of the achieved wish rank across all new (non-fixed) assignments: key = rank, value = number of
    ///     (student, slot) assignments that satisfied a wish of that rank.
    /// </summary>
    public Dictionary<int, int> RangVerteilung { get; set; } = new();
}
