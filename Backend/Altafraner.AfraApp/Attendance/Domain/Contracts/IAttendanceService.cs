using Altafraner.AfraApp.Attendance.Domain.Dto;
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
    ///     Gets attendances for a set of slots and students
    /// </summary>
    /// <param name="requests">All student-slot-combinations the attendances need to be fetched for</param>
    /// <returns>A dictionary that assigns each element in requests an AttendanceState</returns>
    Task<Dictionary<AttendanceEntryId, AttendanceInformation>> GetAttendances(
        IEnumerable<AttendanceEntryId> requests);

    /// <summary>
    ///     Gets the attendance status for a student in a specific slot
    /// </summary>
    /// <returns>The <see cref="AttendanceState" /> for the enrollment.</returns>
    Task<AttendanceInformation> GetAttendance(AttendanceEntryId request);

    /// <summary>
    ///     Gets the attendance status for all students in a specific slot
    /// </summary>
    /// <param name="scope">The scope the slot is in</param>
    /// <param name="slotId">The slot to get all attendance states for</param>
    /// <returns>A dictionary connecting persons to attendance states. If a person is missing from the dictionary, he should be considered missing.</returns>
    Task<Dictionary<Person, AttendanceInformation>> GetAttendanceForSlotAsync(
        AttendanceScope scope,
        Guid slotId);

    /// <summary>
    ///     Sets the attendance status for a specific enrollment
    /// </summary>
    /// <param name="id">Describes the attendance to be set</param>
    /// <param name="status">The status to set</param>
    Task SetAttendanceAsync(AttendanceEntryId id, AttendanceState status);

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

    /// <summary>
    ///     Creates attendance entries for all students
    /// </summary>
    internal Task CreateAutomaticEntries(AttendanceScope scope, Guid slotId);
}
