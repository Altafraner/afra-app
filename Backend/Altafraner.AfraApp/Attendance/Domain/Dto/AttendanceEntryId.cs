using Altafraner.AfraApp.Attendance.Domain.Models;

namespace Altafraner.AfraApp.Attendance.Domain.Dto;

/// <summary>
///     Identifies a specific attendance of a student in a slot
/// </summary>
public record struct AttendanceEntryId
{
    /// <summary>
    ///     The scope of the attendance
    /// </summary>
    public required AttendanceScope Scope { get; init; }

    /// <summary>
    ///     The slot of the attendance
    /// </summary>
    public required Guid SlotId { get; init; }

    /// <summary>
    ///     The student whose attendance is described
    /// </summary>
    public required Guid StudentId { get; init; }
}
