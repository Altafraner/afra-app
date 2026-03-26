using Altafraner.AfraApp.Attendance.Domain.Contracts;
using Altafraner.AfraApp.Attendance.Domain.Dto.Enrollments;
using Altafraner.AfraApp.Attendance.Domain.HubClients;
using Altafraner.AfraApp.Attendance.Domain.Models;
using Microsoft.AspNetCore.SignalR;

namespace Altafraner.AfraApp.Attendance.API.Hubs;

/// <summary>
///     A hub for managing attendance updates in the Otium application.
/// </summary>
internal partial class AttendanceHub : Hub<IAttendanceHubClient>
{
    private readonly IAttendanceService _attendanceService;
    private readonly IServiceProvider _serviceProvider;
    private readonly IAttendanceNotificationService _notificationService;

    /// <summary>
    ///     Constructs a new instance of the <see cref="AttendanceHub" /> class.
    /// </summary>
    public AttendanceHub(IAttendanceService attendanceService,
        IServiceProvider serviceProvider,
        IAttendanceNotificationService notificationService)
    {
        _attendanceService = attendanceService;
        _serviceProvider = serviceProvider;
        _notificationService = notificationService;
    }

    /// <summary>
    ///     Subscribes a user to get updates for a specific event.
    /// </summary>
    public async Task<IAttendanceHubClient.Capabilities> SubscribeToEvent(AttendanceScope scope,
        Guid slotId,
        Guid eventId)
    {
        if (Scope is not null) throw new HubException("AttendanceHub already subscribed");

        Scope = scope;
        Type = AttendanceType.Event;
        EventId = eventId;
        SlotId = slotId;

        var informationProvider = GetInformationProvider();
        await Authorize(informationProvider);
        var metadata = await informationProvider.GetMetadataForSlot(SlotId);

        if (!metadata.IsInPast) await _attendanceService.CreateAutomaticEntries(scope, slotId);
        await Groups.AddToGroupAsync(Context.ConnectionId, EventGroupName(scope, slotId, eventId));
        await _notificationService.UpdateEventAttendance(scope, slotId, eventId, Context.ConnectionId);
        await ScheduleMissingStudentNotifications(metadata);

        return new IAttendanceHubClient.Capabilities(metadata.EnableNotes,
            metadata.EnableMove,
            metadata.MoveNowIntervall);
    }

    /// <summary>
    ///     Subscribes a user to get updates for a specific block.
    /// </summary>
    public async Task<IAttendanceHubClient.Capabilities> SubscribeToSlot(AttendanceScope scope, Guid slotId)
    {
        if (Scope is not null) throw new HubException("AttendanceHub already subscribed");

        Scope = scope;
        Type = AttendanceType.Slot;
        SlotId = slotId;

        var informationProvider = GetInformationProvider();
        await Authorize(informationProvider);

        var metadata = await informationProvider.GetMetadataForSlot(SlotId);

        if (!metadata.IsInPast) await _attendanceService.CreateAutomaticEntries(scope, slotId);
        await Groups.AddToGroupAsync(Context.ConnectionId, SlotGroupName(scope, slotId));
        await _notificationService.UpdateSlotAttendances(scope, slotId, false, Context.ConnectionId);
        await ScheduleMissingStudentNotifications(metadata);

        return new IAttendanceHubClient.Capabilities(metadata.EnableNotes,
            metadata.EnableMove,
            metadata.MoveNowIntervall);
    }

    /// <summary>
    ///     Sets the attendance status for a user in a block.
    /// </summary>
    /// <param name="studentId">The id of the Students <see cref="User.Domain.Models.Person" /> entity.</param>
    /// <param name="status">The new status</param>
    /// <param name="dbContext">Injected via DI</param>
    public async Task SetAttendanceStatus(Guid studentId, AttendanceState status, AfraAppContext dbContext)
    {
        EnsureSubscribed();
        var informationProvider = GetInformationProvider();
        await Authorize(informationProvider);
        await _attendanceService.SetAttendanceAsync(Scope!.Value, SlotId, studentId, status);
    }

    /// <summary>
    ///     Updates the checked-status of a specific event or all missing persons in a block.
    /// </summary>
    /// <param name="eventId">
    ///     The id of the event the update is for. Use <see cref="Guid.Empty">Guid.Empty</see> for missing
    ///     persons.
    /// </param>
    /// <param name="status">The new status</param>
    public async Task SetEventStatus(Guid eventId, bool status)
    {
        EnsureSubscribed();
        var informationProvider = GetInformationProvider();
        await Authorize(informationProvider);
        await _attendanceService.SetEventStatusAsync(Scope!.Value, SlotId, eventId, status);
    }

    /// <summary>
    /// Retrieves the parallel running events for a given event.
    /// </summary>
    /// <returns></returns>
    public async Task<IEnumerable<Event>> GetEventsAvailable()
    {
        EnsureSubscribed();
        var informationProvider = GetInformationProvider();
        await Authorize(informationProvider);
        return await informationProvider.GetEventsForSlot(SlotId);
    }

    /// <summary>
    ///     Moves a student from one event to another.
    /// </summary>
    /// <param name="studentId">The id of the student to move</param>
    /// <param name="toEventId">The id of the event to move the student to</param>
    public async Task MoveStudentNow(Guid studentId,
        Guid toEventId)
    {
        EnsureSubscribed();
        var informationProvider = GetInformationProvider();
        await Authorize(informationProvider);
        var capabilitiesForSlot = await informationProvider.GetMetadataForSlot(SlotId);
        if (!capabilitiesForSlot.EnableMove || !capabilitiesForSlot.MoveNowIntervall.HasValue ||
            !capabilitiesForSlot.MoveNowIntervall.Value.Contains(DateTime.Now))
            throw new HubException("Moving students is beginning now is not enabled");
        try
        {
            await informationProvider.MoveStudentNow(studentId, SlotId, toEventId);
        }
        catch (KeyNotFoundException)
        {
            throw new HubException("The student could not be moved since he or the event does not exist");
        }
    }

    /// <summary>
    ///     Moves a student from one event to another.
    /// </summary>
    /// <param name="studentId">The id of the user</param>
    /// <param name="eventId">The id of the event to move the user to</param>
    public async Task MoveStudent(Guid studentId, Guid eventId)
    {
        EnsureSubscribed();
        var informationProvider = GetInformationProvider();
        await Authorize(informationProvider);
        var capabilitiesForSlot = await informationProvider.GetMetadataForSlot(SlotId);
        if (!capabilitiesForSlot.EnableMove)
            throw new HubException("Moving students is beginning now is not enabled");
        try
        {
            await informationProvider.MoveStudent(studentId, SlotId, eventId);
        }
        catch (KeyNotFoundException)
        {
            throw new HubException("The student could not be moved since he or the event does not exist");
        }
    }
}
