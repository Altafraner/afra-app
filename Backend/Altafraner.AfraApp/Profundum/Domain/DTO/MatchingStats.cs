namespace Altafraner.AfraApp.Profundum.Domain.DTO;

///
public enum MatchingResultStatus
{
    ///
    MatchingComplete,

    ///
    MatchingIncomplete,
}

///
public record MatchingStats
{
    ///
    public required double CalculationTime { get; set; }

    ///
    public MatchingResultStatus Result { get; set; }
}
