using Altafraner.AfraApp.Dashboard.Contracts.Models;

namespace Altafraner.AfraApp.Dashboard.Contracts.DTO;

/// <summary>
///     A tutors dashboard
/// </summary>
public class TutorDashboard
{
    /// <summary>
    ///     information on the tutors mentees
    /// </summary>
    public required IEnumerable<DashboardMenteeOverview> Mentees { get; init; }

    /// <summary>
    ///     information on upcoming events
    /// </summary>
    public required IEnumerable<ScopedDashboardTutorEventDescriptor> Events { get; init; }
}
