using Altafraner.AfraApp.Dashboard.Contracts.Models;
using Altafraner.AfraApp.User.Domain.DTO;

namespace Altafraner.AfraApp.Dashboard.Contracts.DTO;

/// <summary>
///     Contains information on a mentee as displayed on the mentors Dashboard
/// </summary>
public class DashboardMenteeOverview
{
    /// <summary>
    ///     The mentee the information is for
    /// </summary>
    public required PersonInfoMinimal Mentee { get; init; }

    /// <summary>
    ///     The status in the last week
    /// </summary>
    public required DashboardMenteeStatus Last { get; init; }

    /// <summary>
    ///     The status in the current week
    /// </summary>
    public required DashboardMenteeStatus Current { get; init; }

    /// <summary>
    ///     The status in the next week
    /// </summary>
    public required DashboardMenteeStatus Next { get; init; }
}
