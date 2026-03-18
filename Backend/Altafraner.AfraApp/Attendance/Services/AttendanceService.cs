using Altafraner.AfraApp.Attendance.Domain.Contracts;
using Altafraner.AfraApp.Attendance.Domain.Models;
using Altafraner.AfraApp.User.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Altafraner.AfraApp.Attendance.Services;

/// <summary>
/// A service for managing attendance in the Otium module of the Afra App.
/// </summary>
internal sealed class AttendanceService : IAttendanceService
{
    private readonly AfraAppContext _dbContext;
    private readonly SimpleAttendanceNotificationService _simpleAttendanceNotificationService;

    /// <summary>
    /// Initializes a new instance of the <see cref="AttendanceService"/> class.
    /// </summary>
    public AttendanceService(AfraAppContext dbContext,
        SimpleAttendanceNotificationService simpleAttendanceNotificationService)
    {
        _dbContext = dbContext;
        _simpleAttendanceNotificationService = simpleAttendanceNotificationService;
    }

    public async Task<AttendanceState> GetAttendanceForStudentInSlotAsync(AttendanceScope scope,
        Guid slotId,
        Guid studentId)
    {
        var attendanceEntry = await _dbContext.OtiaAnwesenheiten
            .Where(e =>
                e.Scope == scope && e.SlotId == slotId && e.StudentId == studentId)
            .Select(e => new { e.Status })
            .FirstOrDefaultAsync();
        return attendanceEntry?.Status ?? IAttendanceService.DefaultAttendanceStatus;
    }

    public async Task<Dictionary<Guid, AttendanceState>> GetAttendanceForStudentsInSlotAsync(AttendanceScope scope,
        Guid slotId,
        IEnumerable<Guid> studentIds)
    {
        var attendanceEntry = await _dbContext.OtiaAnwesenheiten
            .Where(e =>
                e.Scope == scope && e.SlotId == slotId && studentIds.Contains(e.StudentId))
            .Select(e => new { e.StudentId, e.Status })
            .ToDictionaryAsync(e => e.StudentId, e => e.Status);
        foreach (var studentId in studentIds)
            attendanceEntry.TryAdd(studentId, IAttendanceService.DefaultAttendanceStatus);

        return attendanceEntry;
    }

    public async Task<Dictionary<(AttendanceScope Scope, Guid SlotId), AttendanceState>>
        GetAttendanceForStudentInSlotsAsync(
            IEnumerable<(AttendanceScope Scope, Guid SlotId)> slots,
            Guid personId)
    {
        var attendanceEntries = await _dbContext.OtiaAnwesenheiten
            .Where(e => slots.Any(s => s.Scope == e.Scope && s.SlotId == e.SlotId)
                        && e.StudentId == personId)
            .Select(e => new { e.Scope, e.SlotId, e.Status })
            .ToDictionaryAsync(e => (e.Scope, e.SlotId), e => e.Status);
        var keys = attendanceEntries.Keys;
        var missing = slots.Where(s => !keys.Contains(s)).ToArray();
        attendanceEntries.EnsureCapacity(attendanceEntries.Count + missing.Length);
        foreach (var slot in missing) attendanceEntries.Add(slot, IAttendanceService.DefaultAttendanceStatus);

        return attendanceEntries;
    }

    public async Task<Dictionary<Person, AttendanceState>> GetAttendanceForSlotAsync(AttendanceScope scope, Guid slotId)
    {
        return await _dbContext.Personen
            .LeftJoin(
                _dbContext.OtiaAnwesenheiten
                    .Where(e => e.Scope == scope && e.SlotId == slotId),
                p => p.Id,
                a => a.StudentId,
                (p, a) => new { Person = p, Attendance = a })
            .ToDictionaryAsync(x => x.Person, x => x.Attendance?.Status ?? IAttendanceService.DefaultAttendanceStatus);
    }

    public async Task SetAttendanceAsync(AttendanceScope scope, Guid slotId, Guid studentId, AttendanceState status)
    {
        var attendanceEntry = await _dbContext.OtiaAnwesenheiten
            .FirstOrDefaultAsync(e => e.Scope == scope && e.SlotId == slotId && e.StudentId == studentId);
        if (attendanceEntry is null)
        {
            _dbContext.OtiaAnwesenheiten.Add(new Domain.Models.Attendance
            {
                Scope = scope,
                SlotId = slotId,
                StudentId = studentId,
                Status = status
            });
            await _simpleAttendanceNotificationService.UpdateSingleAttendance(scope, slotId, studentId, status);
            await _dbContext.SaveChangesAsync();
            return;
        }

        attendanceEntry.Status = status;
        await _dbContext.SaveChangesAsync();
    }

    public async Task SetEventStatusAsync(AttendanceScope scope, Guid slotId, Guid eventId, bool status)
    {
        var entity = await _dbContext.AttendanceEventStatus.FirstOrDefaultAsync(e =>
            e.Scope == scope && e.SlotId == slotId && e.EventId == eventId);
        if (entity is null)
        {
            _dbContext.AttendanceEventStatus.Add(new AttendanceEventStatus
            {
                Scope = scope,
                SlotId = slotId,
                EventId = eventId,
                Status = status
            });
            await _dbContext.SaveChangesAsync();
            return;
        }

        entity.Status = status;
        await _dbContext.SaveChangesAsync();
    }

    public async Task<Dictionary<Guid, bool>> GetEventStatusForSlotAsync(AttendanceScope scope, Guid slotId)
    {
        return await _dbContext.AttendanceEventStatus
            .Where(e => e.Scope == scope && e.SlotId == slotId)
            .ToDictionaryAsync(e => e.EventId, e => e.Status);
    }
}
