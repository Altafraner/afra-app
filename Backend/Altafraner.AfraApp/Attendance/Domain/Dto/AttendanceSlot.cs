using Altafraner.AfraApp.Attendance.Domain.Models;

namespace Altafraner.AfraApp.Attendance.Domain.Dto;

/// <summary>
///     A slot in which attendances can be supervised
/// </summary>
public class AttendanceSlot
{
    /// <summary>
    ///     The slots scope
    /// </summary>
    public required AttendanceScope Scope { get; set; }

    /// <summary>
    ///     The slot the attendance is for
    /// </summary>
    public required Guid SlotId { get; set; }

    /// <summary>
    ///     The name of the slot
    /// </summary>
    public required string Label { get; set; }
}
