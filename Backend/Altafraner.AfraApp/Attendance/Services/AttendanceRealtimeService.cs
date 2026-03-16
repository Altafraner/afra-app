using Altafraner.AfraApp.Attendance.Domain.Dto.Notiz;
using Altafraner.AfraApp.Attendance.Domain.HubClients;
using Altafraner.AfraApp.Attendance.Domain.Models;
using Altafraner.AfraApp.Otium.Services;
using Microsoft.AspNetCore.SignalR;
using AttendanceHub = Altafraner.AfraApp.Attendance.API.Hubs.AttendanceHub;

namespace Altafraner.AfraApp.Attendance.Services;

internal sealed class AttendanceRealtimeService
{
    private readonly IHubContext<AttendanceHub, IAttendanceHubClient> _hubContext;
    private readonly ManagementService _managementService;

    public AttendanceRealtimeService(IHubContext<AttendanceHub, IAttendanceHubClient> hubContext,
        ManagementService managementService)
    {
        _hubContext = hubContext;
        _managementService = managementService;
    }

    internal async Task SendNoteUpdate(Guid studentId, Guid blockId, IEnumerable<OtiumAnwesenheitsNotiz> notes)
    {
        var termine = await _managementService.GetTermineInBlock(blockId);
        var update = new IAttendanceHubClient.NoteUpdate(studentId, notes.Select(n => new Notiz(n)));

        foreach (var termin in termine)
            await _hubContext.Clients.Group(AttendanceHub.TerminGroupName(termin)).UpdateNote(update);
        await _hubContext.Clients.Group(AttendanceHub.BlockGroupName(blockId)).UpdateNote(update);
    }
}
