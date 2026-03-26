using System.Linq.Expressions;
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
    private readonly IEnumerable<IAttendanceAutomaticEntryProvider> _automaticEntryProviders;
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="AttendanceService"/> class.
    /// </summary>
    public AttendanceService(AfraAppContext dbContext,
        SimpleAttendanceNotificationService simpleAttendanceNotificationService,
        IEnumerable<IAttendanceAutomaticEntryProvider> automaticEntryProviders,
        IServiceProvider serviceProvider)
    {
        _dbContext = dbContext;
        _simpleAttendanceNotificationService = simpleAttendanceNotificationService;
        _automaticEntryProviders = automaticEntryProviders;
        _serviceProvider = serviceProvider;
    }

    public async Task<AttendanceState> GetAttendanceForStudentInSlotAsync(AttendanceScope scope,
        Guid slotId,
        Guid studentId)
    {
        var attendanceEntry = await _dbContext.Attendances
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
        var attendanceEntry = await _dbContext.Attendances
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
        var parameter = Expression.Parameter(typeof(Domain.Models.Attendance), "e");
        Expression body = Expression.Constant(false);

        var slotsArray = slots as (AttendanceScope Scope, Guid SlotId)[] ?? slots.ToArray();
        foreach (var slot in slotsArray)
        {
            // e.Scope == slot.Scope
            var scopeEqual = Expression.Equal(
                Expression.Property(parameter, nameof(Domain.Models.Attendance.Scope)),
                Expression.Constant(slot.Scope));

            // e.SlotId == slot.SlotId
            var slotIdEqual = Expression.Equal(
                Expression.Property(parameter, nameof(Domain.Models.Attendance.SlotId)),
                Expression.Constant(slot.SlotId));

            // e.Scope == slot.Scope && e.SlotId == slot.SlotId
            var andExpression = Expression.AndAlso(scopeEqual, slotIdEqual);

            body = Expression.OrElse(body, andExpression);
        }

        var compositeFilter = Expression.Lambda<Func<Domain.Models.Attendance, bool>>(body, parameter);

        var attendanceEntries = await _dbContext.Attendances
            .Where(e => e.StudentId == personId)
            .Where(compositeFilter)
            .Select(e => new { e.Scope, e.SlotId, e.Status })
            .ToDictionaryAsync(e => (e.Scope, e.SlotId), e => e.Status);
        var keys = attendanceEntries.Keys;
        var missing = slotsArray.Where(s => !keys.Contains(s)).ToArray();
        attendanceEntries.EnsureCapacity(attendanceEntries.Count + missing.Length);
        foreach (var slot in missing) attendanceEntries.Add(slot, IAttendanceService.DefaultAttendanceStatus);

        return attendanceEntries;
    }

    public async Task<Dictionary<Person, AttendanceState>> GetAttendanceForSlotAsync(AttendanceScope scope, Guid slotId)
    {
        return await _dbContext.Personen
            .LeftJoin(
                _dbContext.Attendances
                    .Where(e => e.Scope == scope && e.SlotId == slotId),
                p => p.Id,
                a => a.StudentId,
                (p, a) => new { Person = p, Attendance = a })
            .ToDictionaryAsync(x => x.Person, x => x.Attendance?.Status ?? IAttendanceService.DefaultAttendanceStatus);
    }

    public async Task SetAttendanceAsync(AttendanceScope scope, Guid slotId, Guid studentId, AttendanceState status)
    {
        var attendanceEntry = await _dbContext.Attendances
            .FirstOrDefaultAsync(e => e.Scope == scope && e.SlotId == slotId && e.StudentId == studentId);
        if (attendanceEntry is null)
        {
            _dbContext.Attendances.Add(new Domain.Models.Attendance
            {
                Scope = scope,
                SlotId = slotId,
                StudentId = studentId,
                Status = status,
                EntryType = AttendanceEntryType.Manual
            });
            await _simpleAttendanceNotificationService.UpdateSingleAttendance(scope, slotId, studentId, status);
            await _dbContext.SaveChangesAsync();
            return;
        }

        attendanceEntry.Status = status;
        attendanceEntry.EntryType = AttendanceEntryType.Manual;
        await _simpleAttendanceNotificationService.UpdateSingleAttendance(scope, slotId, studentId, status);
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
            await _simpleAttendanceNotificationService.UpdateStatusForEvent(scope, slotId, eventId, status);
            return;
        }

        entity.Status = status;
        await _dbContext.SaveChangesAsync();
        await _simpleAttendanceNotificationService.UpdateStatusForEvent(scope, slotId, eventId, status);
    }

    public async Task<Dictionary<Guid, bool>> GetEventStatusForSlotAsync(AttendanceScope scope, Guid slotId)
    {
        return await _dbContext.AttendanceEventStatus
            .Where(e => e.Scope == scope && e.SlotId == slotId)
            .ToDictionaryAsync(e => e.EventId, e => e.Status);
    }

    public async Task CreateAutomaticEntries(AttendanceScope scope, Guid slotId)
    {
        var attendances = await _dbContext.Attendances
            .Include(e => e.Student)
            .Where(e => e.Scope == scope && e.SlotId == slotId)
            .ToDictionaryAsync(e => e.Student.Id, e => e);
        var allMsStudentIds = await _dbContext.Personen
            .Where(e => e.Rolle == Rolle.Mittelstufe)
            .Select(e => e.Id)
            .ToHashSetAsync();
        var studentIds = attendances.Where(e => e.Value.EntryType == AttendanceEntryType.Automatic)
            .Select(e => e.Key)
            .ToHashSet();

        var attendanceProvider = _serviceProvider.GetRequiredKeyedService<IAttendanceInformationProvider>(scope);
        var metadata = await attendanceProvider.GetMetadataForSlot(slotId);

        var results = new Dictionary<Guid, AttendanceState>();
        foreach (var entryProvider in _automaticEntryProviders)
        {
            var providerResults = await entryProvider.GetEntriesPerStudent(scope, slotId, metadata);
            results.EnsureCapacity(results.Count + providerResults.Count);
            foreach (var entry in providerResults)
            {
                if (results.TryAdd(entry.Key, entry.Value)) continue;
                var theirResult = results[entry.Key];
                if (theirResult == entry.Value) continue;
                if (theirResult == AttendanceState.Entschuldigt) continue;
                if (entry.Value == AttendanceState.Entschuldigt)
                {
                    results[entry.Key] = AttendanceState.Entschuldigt;
                    continue;
                }

                if (theirResult == AttendanceState.Anwesend) continue;

                // This if is redundant, but I'm gonna leave it for readability
                if (entry.Value == AttendanceState.Anwesend) results[entry.Key] = AttendanceState.Anwesend;
            }
        }

        foreach (var (studentId, attendanceState) in results)
        {
            if (!allMsStudentIds.Contains(studentId)) continue;
            if (attendances.TryGetValue(studentId, out var attendance))
            {
                if (attendance.EntryType == AttendanceEntryType.Manual ||
                    attendance.Status == attendanceState) continue;
                attendance.Status = attendanceState;
                await _simpleAttendanceNotificationService.UpdateSingleAttendance(scope,
                    slotId,
                    studentId,
                    attendance.Status);
            }

            _dbContext.Attendances.Add(new Domain.Models.Attendance
            {
                EntryType = AttendanceEntryType.Automatic,
                Scope = scope,
                SlotId = slotId,
                Status = attendanceState,
                StudentId = studentId
            });
        }

        studentIds.ExceptWith(results.Keys);
        foreach (var studentId in studentIds)
        {
            _dbContext.Remove(attendances[studentId]);
            await _simpleAttendanceNotificationService.UpdateSingleAttendance(scope,
                slotId,
                studentId,
                IAttendanceService.DefaultAttendanceStatus);
        }

        await _dbContext.SaveChangesAsync();
    }
}
