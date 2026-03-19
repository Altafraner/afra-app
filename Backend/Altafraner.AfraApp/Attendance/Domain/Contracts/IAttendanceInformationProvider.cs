using System.Security.Claims;
using Altafraner.AfraApp.Attendance.Domain.Dto;
using Altafraner.AfraApp.Attendance.Domain.Dto.Enrollments;
using Altafraner.AfraApp.Attendance.Domain.Models;
using Altafraner.AfraApp.User.Domain.Models;

namespace Altafraner.AfraApp.Attendance.Domain.Contracts;

/// <summary>
///     Defines methods used to provide the AttendanceService with data
/// </summary>
public interface IAttendanceInformationProvider
{
    /// <summary>
    ///     The scope name for this provider
    /// </summary>
    AttendanceScope Scope { get; }

    /// <summary>
    ///     Gets alle Enrollments for a given Slot
    /// </summary>
    Task<IEnumerable<EventWithEnrollments>> GetEnrollmentsForSlot(Guid slotId);

    /// <summary>
    ///     Finds a student in a slot
    /// </summary>
    /// <returns>The eventId the student is enrolled in</returns>
    Task<Guid> GetEventForStudentAndSlot(Guid slotId, Guid studentId);

    /// <summary>
    ///     Gets all Enrollments for a given Event
    /// </summary>
    Task<IEnumerable<Person>> GetEnrollmentsForEvent(Guid slotId, Guid eventId);

    /// <summary>
    ///     Gets the events in a slot
    /// </summary>
    Task<IEnumerable<Event>> GetEventsForSlot(Guid slotId);

    /// <summary>
    ///     Gets the enabled features for a slot
    /// </summary>
    Task<AttendanceSlotMetadata> GetMetadataForSlot(Guid slotId);

    /// <summary>
    ///     Moves a student to a new event beginning at the start of the current block
    /// </summary>
    Task MoveStudent(Guid studentId, Guid slotId, Guid eventId);

    /// <summary>
    ///     Moves a student to a new event beginning now
    /// </summary>
    Task MoveStudentNow(Guid studentId, Guid slotId, Guid eventId);

    /// <summary>
    ///     Checks whether a given user is allowed to edit data for a given slot
    /// </summary>
    Task<bool> Authorize(Guid slotId, ClaimsPrincipal user);

    /// <summary>
    ///     Gets all slots that are currently available for supervision
    /// </summary>
    Task<IEnumerable<AttendanceSlot>> GetAvailableSlots(ClaimsPrincipal user);

    /// <summary>
    ///     Gets the currently active slots
    /// </summary>
    Task<IEnumerable<AttendanceSlot>> GetActiveSlots();
}
