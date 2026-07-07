using Altafraner.AfraApp.Attendance.API.Hubs;
using Altafraner.AfraApp.Attendance.Domain.Contracts;
using Altafraner.AfraApp.Attendance.Domain.Dto;
using Altafraner.AfraApp.Attendance.Domain.Dto.Notes;
using Altafraner.AfraApp.Attendance.Domain.HubClients;
using Altafraner.AfraApp.Attendance.Domain.Models;
using Altafraner.AfraApp.User.Domain.DTO;
using Microsoft.AspNetCore.SignalR;

namespace Altafraner.AfraApp.Attendance.Services;

internal class AttendanceNotificationService : IAttendanceNotificationService
{
    private readonly IHubContext<AttendanceHub, IAttendanceHubClient> _hubContext;
    private readonly IServiceProvider _serviceProvider;
    private readonly IAttendanceService _attendanceService;
    private readonly NotesService _notesService;

    public AttendanceNotificationService(IHubContext<AttendanceHub, IAttendanceHubClient> hubContext,
        IServiceProvider serviceProvider,
        IAttendanceService attendanceService,
        NotesService notesService)
    {
        _hubContext = hubContext;
        _serviceProvider = serviceProvider;
        _attendanceService = attendanceService;
        _notesService = notesService;
    }

    public async Task SendNotificationToSlot(AttendanceScope scope,
        Guid slotId,
        IAttendanceHubClient.Notification notification)
    {
        var provider = GetProvider(scope);
        var events = await provider.GetEventsForSlot(slotId);
        var attendanceGroups = events
            .Select(eventInfo => AttendanceHub.EventGroupName(scope, slotId, eventInfo.EventId))
            .Append(AttendanceHub.SlotGroupName(scope, slotId));
        await _hubContext.Clients.Groups(attendanceGroups).Notify(notification);
    }

    public async Task SendNotificationToEventInSlot(AttendanceScope scope,
        Guid slotId,
        Guid eventId,
        IAttendanceHubClient.Notification notification)
    {
        await _hubContext.Clients
            .Groups(AttendanceHub.SlotGroupName(scope, slotId), AttendanceHub.EventGroupName(scope, slotId, eventId))
            .Notify(notification);
    }

    public async Task UpdateSlotAttendances(AttendanceScope scope,
        Guid slotId,
        bool includeEvents,
        string? callerOnlyId = null)
    {
        var provider = GetProvider(scope);
        var enrollments = await provider.GetEnrollmentsForSlot(slotId);
        var notes = await _notesService.GetNotesBySlotAsync(scope, slotId);
        var attendances = await _attendanceService.GetAttendanceForSlotAsync(scope, slotId);
        var states = await _attendanceService.GetEventStatusForSlotAsync(scope, slotId);

        // we should keep the order from enrollments as it could have meaning
        var events = enrollments.Select(e =>
            {
                var enrollmentsForEvent = e.Enrollments.Select(student =>
                {
                    var attendance = attendances.GetValueOrDefault(student,
                        new AttendanceInformation
                        {
                            State = IAttendanceService.DefaultAttendanceStatus,
                            Type = AttendanceEntryType.Manual
                        });
                    return new IAttendanceHubClient.StudentStatus(new PersonInfoMinimal(student),
                        attendance.State,
                        attendance.Type,
                        notes.GetValueOrDefault(student.Id, []).Select(note => new Note(note)));
                });
                return new IAttendanceHubClient.EventWithEnrollments(e.EventId,
                    e.Name,
                    e.Location,
                    enrollmentsForEvent,
                    states.GetValueOrDefault(e.EventId, false));
            })
            .ToArray();

        var target = callerOnlyId is null
            ? _hubContext.Clients.Group(AttendanceHub.SlotGroupName(scope, slotId))
            : _hubContext.Clients.Client(callerOnlyId);

        await target.UpdateSlot(events);

        if (callerOnlyId is not null || !includeEvents) return;

        foreach (var terminInformation in events)
            await _hubContext.Clients.Group(AttendanceHub.EventGroupName(scope, slotId, terminInformation.EventId))
                .UpdateEvent(terminInformation.Enrollments);
    }

    public async Task UpdateEventAttendance(AttendanceScope scope,
        Guid slotId,
        Guid eventId,
        string? callerOnlyId = null)
    {
        var provider = GetProvider(scope);
        var enrollments = (await provider.GetEnrollmentsForEvent(slotId, eventId)).ToArray();
        var notes = await _notesService.GetNotesBySlotAsync(scope, slotId);
        var attendances =
            await _attendanceService.GetAttendances(enrollments.Select(e => new AttendanceEntryId
            {
                Scope = scope,
                SlotId = slotId,
                StudentId = e.Id
            }));

        var target = callerOnlyId is null
            ? _hubContext.Clients.Group(AttendanceHub.EventGroupName(scope, slotId, eventId))
            : _hubContext.Clients.Client(callerOnlyId);

        await target.UpdateEvent(enrollments.Select(e =>
        {
            var id = new AttendanceEntryId
            {
                Scope = scope,
                SlotId = slotId,
                StudentId = e.Id
            };
            var attendance = attendances[id];
            return new IAttendanceHubClient.StudentStatus(new PersonInfoMinimal(e),
                attendance.State,
                attendance.Type,
                notes.GetValueOrDefault(e.Id, []).Select(note => new Note(note)));
        }));
    }

    private IAttendanceInformationProvider GetProvider(AttendanceScope scope)
    {
        return _serviceProvider.GetRequiredKeyedService<IAttendanceInformationProvider>(scope);
    }
}
