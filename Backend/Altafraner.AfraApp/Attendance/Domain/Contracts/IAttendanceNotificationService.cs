using Altafraner.AfraApp.Attendance.Domain.HubClients;
using Altafraner.AfraApp.Attendance.Domain.Models;

namespace Altafraner.AfraApp.Attendance.Domain.Contracts;

/// <summary>
///     A service responsible for notifying realtime attendance clients about any relevant changes
/// </summary>
public interface IAttendanceNotificationService
{
    /// <summary>
    ///     Sends a simple notification to all subscribers for a slot
    /// </summary>
    Task SendNotificationToSlot(AttendanceScope scope, Guid slotId, IAttendanceHubClient.Notification notification);

    /// <summary>
    ///     Sends a simple notification to all users subscribed to an event in a slot as well as the slots general supervisors.
    /// </summary>
    Task SendNotificationToEventInSlot(AttendanceScope scope,
        Guid slotId,
        Guid eventId,
        IAttendanceHubClient.Notification notification);

    /// <summary>
    ///     Updates all attendances in a given slot
    /// </summary>
    /// <param name="scope">The scope the slot is in</param>
    /// <param name="slotId">The slot to update attendances for</param>
    /// <param name="includeEvents">Iff true, also updates attendances for events included in the slot.</param>
    /// <param name="callerOnlyId">If set, only sends information to the hubs caller</param>
    Task UpdateSlotAttendances(AttendanceScope scope, Guid slotId, bool includeEvents, string? callerOnlyId = null);

    /// <summary>
    ///     Updates the attendances for a single event in a slot
    /// </summary>
    /// <param name="scope">The slots scope</param>
    /// <param name="slotId">The events slot</param>
    /// <param name="eventId">The events unique id</param>
    /// <param name="callerOnlyId">If set, only sends information to the hubs caller</param>
    Task UpdateEventAttendance(AttendanceScope scope, Guid slotId, Guid eventId, string? callerOnlyId = null);
}
