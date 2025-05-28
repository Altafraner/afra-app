using Afra_App.Data.DTO.Otium;
using Afra_App.Data.Otium;

namespace Afra_App.Endpoints.Otium;

/// <summary>
/// Represents the client interface for the <see cref="AttendanceHub"/>.
/// </summary>
public interface IAttendanceHubClient
{
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
    /// A dto for updating the attendance status of a student in a specific block.
    /// </summary>
    /// <param name="StudentId">The students id</param>
    /// <param name="TerminId">The termins id</param>
    /// <param name="BlockId">The blocks id</param>
    /// <param name="Status">The students updated status</param>
    public record AttendanceUpdate(Guid StudentId, Guid TerminId, Guid BlockId, AnwesenheitsStatus Status);

    /// <summary>
    /// A dto for sending the information about a specific termin to the client.
    /// </summary>
    /// <param name="TerminId">The Id of the termin</param>
    /// <param name="Otium">The name of the otium the termin is for</param>
    /// <param name="Ort">The location the termin is at</param>
    /// <param name="Einschreibungen">a list of all enrollments with their attendance state</param>
    public record TerminInformation(
        Guid TerminId,
        string Otium,
        string Ort,
        IEnumerable<LehrerEinschreibung> Einschreibungen);
}
