using Altafraner.AfraApp.Otium.API.Hubs;
using Altafraner.AfraApp.Otium.Domain.DTO.Notiz;
using Altafraner.AfraApp.Otium.Domain.HubClients;
using Altafraner.AfraApp.Otium.Domain.Models;
using Microsoft.AspNetCore.SignalR;

namespace Altafraner.AfraApp.Otium.Services;

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
