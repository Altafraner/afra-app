namespace Afra_App.Profundum.Domain.DTO;

using Google.OrTools.Sat;

///
public record StudentMatchingStats
{
    ///
    public required double optim { get; set; }
    ///
    public required Dictionary<string, string[]> einschreibungen { get; set; }
    ///
    public required Dictionary<string, string[]> Wuensche { get; set; }
}

///
public record ProfundumMatchingStats
{
    ///
    public required int Einschreibungen { get; set; }
    ///
    public int? MaxEinschreibungen { get; set; }
}

///
public record MatchingStats
{
    ///
    public required double CalculationTime { get; set; }

    ///
    public required CpSolverStatus ResultStatus { get; set; }

    ///
    public required double ObjectiveValue { get; set; }

    ///
    public required double ObjectiveValueNoLimits { get; set; }

    ///
    public required double Optim { get; set; }

    ///
    public Dictionary<string, StudentMatchingStats>? Students { get; set; }

    ///
    public Dictionary<string, ProfundumMatchingStats>? Profunda { get; set; }
}
