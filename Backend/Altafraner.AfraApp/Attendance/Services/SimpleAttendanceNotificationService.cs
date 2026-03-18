using Altafraner.AfraApp.Attendance.API.Hubs;
using Altafraner.AfraApp.Attendance.Domain.Contracts;
using Altafraner.AfraApp.Attendance.Domain.Dto.Notiz;
using Altafraner.AfraApp.Attendance.Domain.HubClients;
using Altafraner.AfraApp.Attendance.Domain.Models;
using Microsoft.AspNetCore.SignalR;

namespace Altafraner.AfraApp.Attendance.Services;

internal sealed class SimpleAttendanceNotificationService
{
    private readonly IHubContext<AttendanceHub, IAttendanceHubClient> _hubContext;
    private readonly IServiceProvider _serviceProvider;

    public SimpleAttendanceNotificationService(IHubContext<AttendanceHub, IAttendanceHubClient> hubContext,
        IServiceProvider serviceProvider)
    {
        _hubContext = hubContext;
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    ///     Updates a single attendance
    /// </summary>
    public async Task UpdateSingleAttendance(AttendanceScope scope,
        Guid slotId,
        Guid studentId,
        AttendanceState attendanceState)
    {
        var informationProvider = _serviceProvider.GetRequiredKeyedService<IAttendanceInformationProvider>(scope);
        var eventId = await informationProvider.GetEventForStudentAndSlot(slotId, studentId);
        await _hubContext.Clients.Groups(AttendanceHub.SlotGroupName(scope, slotId),
                AttendanceHub.EventGroupName(scope, eventId))
            .UpdateAttendance(new IAttendanceHubClient.AttendanceUpdate(studentId, eventId, attendanceState));
    }

    /// <summary>
    ///     Updates the notes registered for a student
    /// </summary>
    public async Task UpdateNotesForSingleStudent(AttendanceScope scope,
        Guid slotId,
        Guid studentId,
        IEnumerable<AttendanceNote> notes)
    {
        var informationProvider = _serviceProvider.GetRequiredKeyedService<IAttendanceInformationProvider>(scope);
        var eventId = await informationProvider.GetEventForStudentAndSlot(slotId, studentId);
        await _hubContext.Clients.Groups(AttendanceHub.EventGroupName(scope, eventId),
                AttendanceHub.SlotGroupName(scope, slotId))
            .UpdateNote(new IAttendanceHubClient.NoteUpdate(studentId, notes.Select(n => new Notiz(n))));
    }
}
