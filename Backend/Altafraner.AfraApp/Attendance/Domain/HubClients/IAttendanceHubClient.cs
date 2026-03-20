using Altafraner.AfraApp.Attendance.Domain.Dto.Notes;
using Altafraner.AfraApp.Attendance.Domain.Models;
using Altafraner.AfraApp.Domain.TimeInterval;
using Altafraner.AfraApp.User.Domain.DTO;

namespace Altafraner.AfraApp.Attendance.Domain.HubClients;

/// <summary>
/// Represents the client interface for the <see cref="API.Hubs.AttendanceHub"/>.
/// </summary>
public interface IAttendanceHubClient
{
    /// <summary>
    /// The severity of a notification that is sent to the client.
    /// </summary>
    public enum NotificationSeverity
    {
        /// <summary>
        /// An error has occurred, immediate action is required.
        /// </summary>
        Error,

        /// <summary>
        /// There has been some unexpected behavior, but it is not critical.
        /// </summary>
        Warning,

        /// <summary>
        /// Some information that is not critical, but might be useful.
        /// </summary>
        Info,
    }

    /// <summary>
    /// Tells the client to update the attendance status of a student.
    /// </summary>
    /// <param name="update">The update to send</param>
    Task UpdateAttendance(AttendanceUpdate update);

    /// <summary>
    ///     Tells the client to update the notes regarding a student.
    /// </summary>
    /// <param name="update">The update to send</param>
    Task UpdateNote(NoteUpdate update);

    /// <summary>
    /// Tells the client to update the attendance status of all students in a block.
    /// </summary>
    Task UpdateSlot(IEnumerable<EventWithEnrollments> updates);

    /// <summary>
    /// Tells the client to update the attendance status of all students in a specific termin.
    /// </summary>
    /// <param name="updates">A list of all attendees</param>
    Task UpdateEvent(IEnumerable<StudentStatus> updates);

    /// <summary>
    /// Tells the client to update the status of a specific termin.
    /// </summary>
    /// <param name="update">The status-update to send</param>
    Task UpdateEventStatus(TerminStatusUpdate update);

    /// <summary>
    /// Sends a notification to the client with the given information.
    /// </summary>
    /// <param name="notification">The notification to send</param>
    Task Notify(Notification notification);

    /// <summary>
    /// A dto for updating the attendance status of a student in a specific block.
    /// </summary>
    /// <param name="StudentId">The students id</param>
    /// <param name="EventId">The events id</param>
    /// <param name="Status">The students updated status</param>
    record AttendanceUpdate(Guid StudentId, Guid EventId, AttendanceState Status);

    /// <summary>
    ///     A dto for updating the notes of a student in a specific block.
    /// </summary>
    /// <param name="StudentId">The students id</param>
    /// <param name="Notes">All notes for the student during the block</param>
    record NoteUpdate(Guid StudentId, IEnumerable<Note> Notes);

    /// <summary>
    /// A dto for updating the status of a specific termin.
    /// </summary>
    /// <param name="EventId">The id of the event the update is for</param>
    /// <param name="Status">The new status</param>
    record TerminStatusUpdate(Guid EventId, bool Status);

    /// <summary>
    /// A dto for sending the information about a specific termin to the client.
    /// </summary>
    /// <param name="EventId">The ID of the event</param>
    /// <param name="Name">The name of the event</param>
    /// <param name="Location">The location of the event</param>
    /// <param name="Enrollments">a list of all enrollments with their attendance state</param>
    /// <param name="Status">whether a supervisor has checked attendance for this termin</param>
    record EventWithEnrollments(
        Guid EventId,
        string Name,
        string Location,
        IEnumerable<StudentStatus> Enrollments,
        bool Status);

    /// <summary>
    /// A dto for sending a notification to the client.
    /// </summary>
    /// <param name="Subject">The notification subject</param>
    /// <param name="Body">The notification body</param>
    /// <param name="Severity">The notification severity</param>
    public record Notification(
        string Subject,
        string Body,
        NotificationSeverity Severity
    );

    /// <summary>
    ///     Correlates a student with his/her enrollment status
    /// </summary>
    /// <param name="Student">The student</param>
    /// <param name="Status">The enrollment status</param>
    /// <param name="Notes">The notes registered for this student in this block</param>
    record struct StudentStatus(PersonInfoMinimal Student, AttendanceState Status, IEnumerable<Note> Notes);

    /// <summary>
    ///     The capabilities of this supervision session
    /// </summary>
    /// <param name="EnableNotes">Whether notes are enabled</param>
    /// <param name="EnableMove">Whether moving students is allowed</param>
    /// <param name="MoveNowInterval">
    ///     The timeInterval for which a valid interpretation of moving a student for only the time
    ///     after the current time exists
    /// </param>
    record Capabilities(bool EnableNotes, bool EnableMove, DateTimeInterval? MoveNowInterval);
}
