using Altafraner.AfraApp.Dashboard.Contracts.Models;
using Altafraner.AfraApp.User.Domain.Models;

namespace Altafraner.AfraApp.Dashboard.Contracts;

/// <summary>
///     A provider for dashboard data
/// </summary>
public interface IDashboardProvider
{
    /// <summary>
    ///     The scope name of the provider
    /// </summary>
    string Scope { get; }

    /// <summary>
    ///     Get Events for a tutors dashboard
    /// </summary>
    /// <param name="tutor">The tutor the events are for</param>
    /// <param name="start">The first day events are requested for (inclusive)</param>
    /// <param name="end">The last day events are requested for (exclusive)</param>
    /// <returns>events in the given timeframe</returns>
    Task<IEnumerable<DashboardTutorEventDescriptor>> GetTutorEvents(Person tutor, DateOnly start, DateOnly end);

    /// <summary>
    ///     Gets the status of mentees in weeks
    /// </summary>
    /// <param name="mentees">The mentees to get the status for</param>
    /// <param name="weeks">The mondays of weeks to get the status for</param>
    Task<Dictionary<Guid, Dictionary<DateOnly, DashboardMenteeStatus>>> GetMenteeStatus(Person[] mentees,
        DateOnly[] weeks);
}
