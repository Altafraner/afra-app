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
}
