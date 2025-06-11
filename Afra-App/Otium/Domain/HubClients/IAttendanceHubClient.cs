using Afra_App.Otium.API.Hubs;
using Afra_App.Otium.Domain.DTO;
using Afra_App.Otium.Domain.Models;

namespace Afra_App.Otium.Domain.HubClients;

/// <summary>
/// Represents the client interface for the <see cref="AttendanceHub"/>.
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
    /// Tells the client to update the attendance status of all students in a block.
    /// </summary>
    Task UpdateBlockAttendances(IEnumerable<TerminInformation> updates);

    /// <summary>
    /// Tells the client to update the attendance status of all students in a specific termin.
    /// </summary>
    /// <param name="updates">A list of all attendees</param>
    Task UpdateTerminAttendances(IEnumerable<LehrerEinschreibung> updates);

    /// <summary>
    /// Tells the client to update the status of a specific termin.
    /// </summary>
    /// <param name="update">The status-update to send</param>
    Task UpdateTerminStatus(TerminStatusUpdate update);

    /// <summary>
    /// Sends a notification to the client with the given information.
    /// </summary>
    /// <param name="notification">The notification to send</param>
    Task Notify(Notification notification);

    /// <summary>
    /// A dto for updating the attendance status of a student in a specific block.
    /// </summary>
    /// <param name="StudentId">The students id</param>
    /// <param name="TerminId">The termins id</param>
    /// <param name="BlockId">The blocks id</param>
    /// <param name="Status">The students updated status</param>
    public record AttendanceUpdate(Guid StudentId, Guid TerminId, Guid BlockId, AnwesenheitsStatus Status);

    /// <summary>
    /// A dto for updating the status of a specific termin.
    /// </summary>
    /// <param name="TerminId">The id of the termin the update is for</param>
    /// <param name="SindAnwesenheitenErfasst">The new status</param>
    public record TerminStatusUpdate(Guid TerminId, bool SindAnwesenheitenErfasst);

    /// <summary>
    /// A dto for sending the information about a specific termin to the client.
    /// </summary>
    /// <param name="TerminId">The Id of the termin</param>
    /// <param name="Otium">The name of the otium the termin is for</param>
    /// <param name="Ort">The location the termin is at</param>
    /// <param name="Einschreibungen">a list of all enrollments with their attendance state</param>
    /// <param name="SindAnwesenheitenErfasst">whether a supervisor has checked attendance for this termin</param>
    public record TerminInformation(
        Guid TerminId,
        string Otium,
        string Ort,
        IEnumerable<LehrerEinschreibung> Einschreibungen,
        bool SindAnwesenheitenErfasst);

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
}
