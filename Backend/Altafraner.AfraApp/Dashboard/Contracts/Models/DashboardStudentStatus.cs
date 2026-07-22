using System.Diagnostics.CodeAnalysis;
using Altafraner.AfraApp.Attendance.Domain.Models;

namespace Altafraner.AfraApp.Dashboard.Contracts.Models;

/// <summary>
///     All information required for a student dashboard
/// </summary>
public class DashboardStudentOverview
{
    /// <summary>
    ///     Contains messages concerning a specific day
    /// </summary>
    public required Dictionary<DateOnly, List<string>> DailyWarnings { get; init; }

    /// <summary>
    ///     Contains messages concerning a specific week
    /// </summary>
    public required Dictionary<DateOnly, List<string>> WeeklyWarnings { get; init; }

    /// <summary>
    ///     The students events
    /// </summary>
    public required IEnumerable<DashboardStudentEventDescriptor> Events { get; init; }
}

/// <summary>
///     Describes an event for a student
/// </summary>
public class DashboardStudentEventDescriptor
{
    /// <summary>
    ///     The events label
    /// </summary>
    public required string? Label { get; init; }

    /// <summary>
    ///     The events start time
    /// </summary>
    public required DateTime Start { get; init; }

    /// <summary>
    ///     The events slot label
    /// </summary>
    public required string SlotLabel { get; init; }

    /// <summary>
    ///     A scope specific payload
    /// </summary>
    public required object Payload { get; init; }

    /// <summary>
    ///     The attendance state
    /// </summary>
    public required AttendanceState? Attendance { get; init; }

    /// <summary>
    ///     The events location
    /// </summary>
    public required string? Location { get; init; }
}

/// <summary>
///     A student event from a specific scope
/// </summary>
public class ScopedDashboardStudentEventDescriptor : DashboardStudentEventDescriptor
{
    /// <summary>
    ///     The events module scope
    /// </summary>
    public string Scope { get; }

    /// <summary>
    ///     Initializes a new <see cref="ScopedDashboardStudentEventDescriptor" /> from an unscoped instance
    /// </summary>
    [SetsRequiredMembers]
    public ScopedDashboardStudentEventDescriptor(DashboardStudentEventDescriptor descriptor, string scope)
    {
        Scope = scope;
        Label = descriptor.Label;
        Start = descriptor.Start;
        SlotLabel = descriptor.SlotLabel;
        Payload = descriptor.Payload;
        Attendance = descriptor.Attendance;
        Location = descriptor.Location;
    }
}
