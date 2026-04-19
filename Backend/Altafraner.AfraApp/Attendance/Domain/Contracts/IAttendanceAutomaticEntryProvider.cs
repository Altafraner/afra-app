using Altafraner.AfraApp.Attendance.Domain.Dto.Enrollments;
using Altafraner.AfraApp.Attendance.Domain.Models;

namespace Altafraner.AfraApp.Attendance.Domain.Contracts;

/// <summary>
///     A provider for generating automatic entries for students
/// </summary>
public interface IAttendanceAutomaticEntryProvider
{
    /// <summary>
    ///     Gets automatic entries for a slot
    /// </summary>
    Task<Dictionary<Guid, AttendanceState>> GetEntriesPerStudent(AttendanceScope scope,
        Guid slotId,
        AttendanceSlotMetadata metadata);
}
