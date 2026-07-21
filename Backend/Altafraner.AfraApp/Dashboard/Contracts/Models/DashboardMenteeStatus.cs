namespace Altafraner.AfraApp.Dashboard.Contracts.Models;

/// <summary>
///     An Enum describing the status of a mentee in a given timeframe
/// </summary>
public enum DashboardMenteeStatus
{
    /// <summary>
    ///     For this mentee no status exists
    /// </summary>
    NotApplicable,

    /// <summary>
    ///     The mentee has broken some rules
    /// </summary>
    Invalid,

    /// <summary>
    ///     There is data missing to calculate a precise answer
    /// </summary>
    Uncertain,

    /// <summary>
    ///     The mentee is following all rules
    /// </summary>
    Valid
}

/// <summary>
///     Extension methods for the <see cref="DashboardMenteeStatus" /> enum
/// </summary>
public static class DashboardMenteeStatusExtensions
{
    /// <summary>
    ///     Merges two stati
    /// </summary>
    public static DashboardMenteeStatus Merge(this DashboardMenteeStatus status1, DashboardMenteeStatus status2)
    {
        if (status1 == DashboardMenteeStatus.NotApplicable) return status2;
        if (status2 == DashboardMenteeStatus.NotApplicable) return status1;
        if (status1 == DashboardMenteeStatus.Invalid || status2 == DashboardMenteeStatus.Invalid)
            return DashboardMenteeStatus.Invalid;
        if (status1 == DashboardMenteeStatus.Uncertain || status2 == DashboardMenteeStatus.Uncertain)
            return DashboardMenteeStatus.Uncertain;
        return DashboardMenteeStatus.Valid;
    }
}
