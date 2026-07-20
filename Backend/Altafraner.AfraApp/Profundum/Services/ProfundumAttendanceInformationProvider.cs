using System.Security.Claims;
using Altafraner.AfraApp.Attendance.Domain.Contracts;
using Altafraner.AfraApp.Attendance.Domain.Dto;
using Altafraner.AfraApp.Attendance.Domain.Dto.Enrollments;
using Altafraner.AfraApp.Attendance.Domain.Models;
using Altafraner.AfraApp.Backbone.Authorization;
using Altafraner.AfraApp.Profundum.Domain.Models;
using Altafraner.AfraApp.User.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Altafraner.AfraApp.Profundum.Services;

internal class ProfundumAttendanceInformationProvider : IAttendanceInformationProvider
{
    private readonly AfraAppContext _dbContext;

    public ProfundumAttendanceInformationProvider(AfraAppContext dbContext)
    {
        _dbContext = dbContext;
    }

    internal const AttendanceScope ScopeValue = AttendanceScope.Profundum;

    public AttendanceScope Scope => ScopeValue;

    public async Task<IEnumerable<EventWithEnrollments>> GetEnrollmentsForSlot(Guid slotId)
    {
        var date = await _dbContext.ProfundaTermine.Include(e => e.Slot).FirstOrDefaultAsync(e => e.Id == slotId);
        if (date is null) throw new KeyNotFoundException();

        var enrollments = await _dbContext.ProfundaEinschreibungen
            .Where(e => e.SlotId == date.Slot.Id && e.ProfundumInstanzId != null)
            .Include(e => e.BetroffenePerson)
            .Include(e => e.ProfundumInstanz)
            .ThenInclude(e => e!.Profundum)
            .AsAsyncEnumerable()
            .GroupBy(e => e.ProfundumInstanz)
            .Select(e => new EventWithEnrollments
            {
                Enrollments = e.Select(e2 => e2.BetroffenePerson),
                EventId = e.First().ProfundumInstanzId!.Value,
                Name = e.First().ProfundumInstanz!.Profundum.Bezeichnung,
                Location = e.First().ProfundumInstanz!.Ort
            })
            .ToArrayAsync();

        return enrollments;
    }

    public async Task<Guid> GetEventForStudentAndSlot(Guid slotId, Guid studentId)
    {
        var date = await _dbContext.ProfundaTermine.Include(e => e.Slot).FirstOrDefaultAsync(e => e.Id == slotId);
        if (date is null) throw new KeyNotFoundException();

        var profundumEvent = await
            _dbContext.ProfundaEinschreibungen.FirstOrDefaultAsync(e =>
                e.BetroffenePersonId == studentId && e.SlotId == date.Slot.Id);
        return profundumEvent?.ProfundumInstanzId ?? throw new KeyNotFoundException();
    }

    public async Task<IEnumerable<Person>> GetEnrollmentsForEvent(Guid slotId, Guid eventId)
    {
        var date = await _dbContext.ProfundaTermine.Include(e => e.Slot).FirstOrDefaultAsync(e => e.Id == slotId);
        if (date is null) throw new KeyNotFoundException();

        var personen = await _dbContext.ProfundaInstanzen
            .Include(e => e.Einschreibungen.Where(e2 => e2.SlotId == date.Slot.Id))
            .ThenInclude(e => e.BetroffenePerson)
            .Where(e => e.Id == eventId)
            .SelectMany(e => e.Einschreibungen)
            .Select(e => e.BetroffenePerson)
            .ToArrayAsync();
        return personen;
    }

    public async Task<IEnumerable<Event>> GetEventsForSlot(Guid slotId)
    {
        var date = await _dbContext.ProfundaTermine
            .Include(e => e.Slot)
            .ThenInclude(e => e.Angebote)
            .ThenInclude(e => e.Profundum)
            .FirstOrDefaultAsync(e => e.Id == slotId);
        if (date is null) throw new KeyNotFoundException();

        return date.Slot.Angebote.Select(e => new Event
        {
            EventId = e.Id,
            Name = e.Profundum.Bezeichnung,
            Location = e.Ort
        });
    }

    public async Task<AttendanceSlotMetadata> GetMetadataForSlot(Guid slotId)
    {
        var date = await _dbContext.ProfundaTermine.FindAsync(slotId);
        if (date is null) throw new KeyNotFoundException();
        var now = DateTime.Now;

        return new AttendanceSlotMetadata
        {
            EnableNotes = false,
            EnableMove = false,
            IsInPast = EndOfAttendanceDateTime(date) < now,
            StartDate = date.Day,
            StartLesson = date.Lesson,
            MissingStudentsNotificationRecipients = [],
            MissingStudentsNotificationTime = null
        };
    }

    public Task MoveStudent(Guid studentId, Guid slotId, Guid eventId)
    {
        throw new NotImplementedException();
    }

    public Task MoveStudentNow(Guid studentId, Guid slotId, Guid eventId)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> Authorize(Guid slotId, ClaimsPrincipal user)
    {
        if (IsProfundumsverantwortlich(user))
            return true;

        var date = await _dbContext.ProfundaTermine.FindAsync(slotId);
        if (date is null) throw new KeyNotFoundException();

        var now = DateTime.Now;
        return IsInAttendanceTimeframe(date, now);
    }

    public async Task<IEnumerable<AttendanceSlot>> GetAvailableSlots(ClaimsPrincipal user)
    {
        var now = DateTime.Now;
        var today = DateOnly.FromDateTime(now);

        var date = await _dbContext.ProfundaTermine.FirstOrDefaultAsync(e => e.Day == today);
        if (date is null) return [];

        return IsProfundumsverantwortlich(user) || IsInAttendanceTimeframe(date, now)
            ?
            [
                new AttendanceSlot
                {
                    Label = "Profundum",
                    Scope = Scope,
                    SlotId = date.Id
                }
            ]
            : [];
    }

    public async Task<IEnumerable<AttendanceSlot>> GetActiveSlots()
    {
        var now = DateTime.Now;
        var today = DateOnly.FromDateTime(now);

        var date = await _dbContext.ProfundaTermine.FirstOrDefaultAsync(e => e.Day == today);
        if (date is null) return [];

        return IsInAttendanceTimeframe(date, now)
            ?
            [
                new AttendanceSlot
                {
                    Label = "Profundum",
                    Scope = Scope,
                    SlotId = date.Id
                }
            ]
            : [];
    }

    private static DateTime EndOfAttendanceDateTime(ProfundumTermin date)
    {
        return date.Day.ToDateTime(date.EndTime).AddMinutes(30);
    }

    private static DateTime StartOfAttendanceDateTime(ProfundumTermin date)
    {
        return date.Day.ToDateTime(date.StartTime).AddHours(-1);
    }

    private static bool IsInAttendanceTimeframe(ProfundumTermin date, DateTime time)
    {
        return EndOfAttendanceDateTime(date) >= time && StartOfAttendanceDateTime(date) <= time;
    }

    private static bool IsProfundumsverantwortlich(ClaimsPrincipal user)
    {
        return user.HasClaim(AfraAppClaimTypes.GlobalPermission, nameof(GlobalPermission.Profundumsverantwortlich));
    }
}
