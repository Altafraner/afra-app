using Altafraner.AfraApp.Attendance.Domain.Models;

namespace Altafraner.AfraApp.Attendance.Domain.Dto;

/// <summary>
///     Contains information on a specific attendance
/// </summary>
public struct AttendanceInformation
{
    /// <summary>
    ///     The attendance state
    /// </summary>
    public AttendanceState State { get; init; }

    /// <summary>
    ///     The attendances type
    /// </summary>
    public AttendanceEntryType Type { get; init; }
}
