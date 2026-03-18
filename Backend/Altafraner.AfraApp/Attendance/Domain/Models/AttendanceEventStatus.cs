using Microsoft.EntityFrameworkCore;

namespace Altafraner.AfraApp.Attendance.Domain.Models;

/// <summary>
///     Contains the status for a supervisable event
/// </summary>
[PrimaryKey(nameof(Scope), nameof(SlotId), nameof(EventId))]
public class AttendanceEventStatus
{
    /// <summary>
    ///     The scope of the slot
    /// </summary>
    public required AttendanceScope Scope { get; set; }

    /// <summary>
    ///     The events slot
    /// </summary>
    public required Guid SlotId { get; set; }

    /// <summary>
    ///     the events unique identifier
    /// </summary>
    public required Guid EventId { get; set; }

    /// <summary>
    ///     the status
    /// </summary>
    public required bool Status { get; set; }
}
