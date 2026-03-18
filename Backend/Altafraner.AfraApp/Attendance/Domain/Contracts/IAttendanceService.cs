using Altafraner.AfraApp.Attendance.Domain.Models;
using Altafraner.AfraApp.User.Domain.Models;

namespace Altafraner.AfraApp.Attendance.Domain.Contracts;

/// <summary>
///     A service interface for managing attendance in the Otium module
/// </summary>
public interface IAttendanceService
{
    /// <summary>
    /// The default attendance state
    /// </summary>
    internal const AttendanceState DefaultAttendanceStatus = AttendanceState.Fehlend;

    /// <summary>
    ///     Gets the attendance status for a student in a specific slot
    /// </summary>
    /// <param name="scope">The scope the slot is in</param>
    /// <param name="slotId">The id of the slot the attendance is for.</param>
    /// <param name="studentId">The id of the student the attendance is for.</param>
    /// <returns>The <see cref="AttendanceState" /> for the enrollment.</returns>
    Task<AttendanceState> GetAttendanceForStudentInSlotAsync(AttendanceScope scope,
        Guid slotId,
        Guid studentId);

    /// <summary>
    ///     Gets the attendance status for students in a specific slot
    /// </summary>
    /// <param name="scope">The scope the slot is in</param>
    /// <param name="slotId">The id of the slot the attendance is for.</param>
    /// <param name="studentIds">The id of the students the attendance is for.</param>
    /// <returns>The <see cref="AttendanceState" /> for the enrollment.</returns>
    Task<Dictionary<Guid, AttendanceState>> GetAttendanceForStudentsInSlotAsync(AttendanceScope scope,
        Guid slotId,
        IEnumerable<Guid> studentIds);

    /// <summary>
    /// Gets the attendance for a student in all provided slots
    /// </summary>
    Task<Dictionary<(AttendanceScope Scope, Guid SlotId), AttendanceState>> GetAttendanceForStudentInSlotsAsync(
        IEnumerable<(AttendanceScope Scope, Guid SlotId)> slots,
        Guid personId);

    /// <summary>
    ///     Gets the attendance status for all students in a specific slot
    /// </summary>
    /// <param name="scope">The scope the slot is in</param>
    /// <param name="slotId">The slot to get all attendance states for</param>
    /// <returns>A dictionary connecting persons to attendance states. If a person is missing from the dictionary, he should be considered missing.</returns>
    Task<Dictionary<Person, AttendanceState>> GetAttendanceForSlotAsync(AttendanceScope scope, Guid slotId);

    /// <summary>
    ///     Sets the attendance status for a specific enrollment
    /// </summary>
    /// <param name="scope">The scope the slot is in</param>
    /// <param name="slotId">The slot to set the attendance state in</param>
    /// <param name="studentId">The student to set the attendance for</param>
    /// <param name="status">The status to set</param>
    /// <exception cref="KeyNotFoundException">The enrollment does not exist</exception>
    Task SetAttendanceAsync(AttendanceScope scope, Guid slotId, Guid studentId, AttendanceState status);

    /// <summary>
    ///     Sets the checked status for a specific termin
    /// </summary>
    /// <param name="scope">The scope the slot is in</param>
    /// <param name="slotId">The slot the termin is in</param>
    /// <param name="eventId">The id of the termin</param>
    /// <param name="status">The new status</param>
    Task SetEventStatusAsync(AttendanceScope scope, Guid slotId, Guid eventId, bool status);

    /// <summary>
    ///     Gets the status for all registered events in a slot
    /// </summary>
    /// <param name="scope">The scope the slot is in</param>
    /// <param name="slotId">The slot to get the status for</param>
    Task<Dictionary<Guid, bool>> GetEventStatusForSlotAsync(AttendanceScope scope, Guid slotId);
}
