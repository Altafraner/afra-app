using Altafraner.AfraApp.Dashboard.Contracts.Models;

namespace Altafraner.AfraApp.Dashboard.Contracts.DTO;

/// <summary>
///     A students dashboard information
/// </summary>
public class StudentDashboard
{
    /// <summary>
    ///     The weeks the dashboard is for
    /// </summary>
    public required List<StudentWeek> Weeks { get; init; }
}

/// <summary>
///     A week as shown in the student dashboard
/// </summary>
public class StudentWeek
{
    /// <summary>
    ///     The first day of the week
    /// </summary>
    public required DateOnly Monday { get; init; }

    /// <summary>
    ///     Warning Messages for the whole week
    /// </summary>
    public required IEnumerable<string> Warnings { get; init; }

    /// <summary>
    ///     Warning messages for days of the week
    /// </summary>
    public required Dictionary<DateOnly, List<string>> DailyWarnings { get; init; }

    /// <summary>
    ///     The students events during that week
    /// </summary>
    public required IEnumerable<ScopedDashboardStudentEventDescriptor> Events { get; init; }
}
