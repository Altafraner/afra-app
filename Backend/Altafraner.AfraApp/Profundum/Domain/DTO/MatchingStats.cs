namespace Altafraner.AfraApp.Profundum.Domain.DTO;

///
public enum MatchingResultStatus
{
    ///
    MatchingFound,

    ///
    MatchingIncompleteDueToCapacity,

    ///
    MatchingIncompleteDueToHardConstraints,
}

///
public record MatchingStats
{
    ///
    public required double CalculationTime { get; set; }

    ///
    public MatchingResultStatus Result { get; set; }

    ///
    public required double ObjectiveValue { get; set; }

    ///
    public required double ObjectiveValueNoLimits { get; set; }

    ///
    public required double Optim { get; set; }
}
