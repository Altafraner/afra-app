using System.Security.Claims;
using Altafraner.AfraApp.Attendance.Domain.Contracts;
using Altafraner.AfraApp.Attendance.Domain.Dto;
using Altafraner.AfraApp.Attendance.Domain.Dto.Enrollments;
using Altafraner.AfraApp.Attendance.Domain.Models;
using Altafraner.AfraApp.Backbone.Authorization;
using Altafraner.AfraApp.Otium.Configuration;
using Altafraner.AfraApp.Otium.Domain.Models.TimeInterval;
using Altafraner.AfraApp.Schuljahr.Domain.Models;
using Altafraner.AfraApp.User.Domain.Models;
using Altafraner.AfraApp.User.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Altafraner.AfraApp.Otium.Services;

internal sealed class OtiumAttendanceInformationProvider : IAttendanceInformationProvider
{
    private readonly EnrollmentService _enrollmentService;
    private readonly AfraAppContext _dbContext;
    private readonly BlockHelper _blockHelper;
    private readonly OtiumConfiguration _otiumConfiguration;
    private readonly UserService _userService;

    public OtiumAttendanceInformationProvider(EnrollmentService enrollmentService,
        AfraAppContext dbContext,
        BlockHelper blockHelper,
        IOptions<OtiumConfiguration> otiumConfiguration,
        UserService userService)
    {
        _enrollmentService = enrollmentService;
        _dbContext = dbContext;
        _blockHelper = blockHelper;
        _userService = userService;
        _otiumConfiguration = otiumConfiguration.Value;
    }

    internal const AttendanceScope ScopeValue = AttendanceScope.Otium;
    public AttendanceScope Scope => ScopeValue;

    public async Task<IEnumerable<EventWithEnrollments>> GetEnrollmentsForSlot(Guid slotId)
    {
        var block = await _dbContext.Blocks.FindAsync(slotId);
        var schema = _blockHelper.Get(block!.SchemaId)!;

        var attendances = await _dbContext.OtiaEinschreibungen
            .Where(e => e.Termin.Block.Id == slotId)
            .GroupBy(e => e.BetroffenePerson)
            .Select(e => e.OrderByDescending(ei => ei.Interval.End).Last())
            .GroupBy(e => e.Termin)
            .OrderBy(e => e.Key.Ort)
            .ThenBy(e => e.Key.Bezeichnung)
            .Select(e => new EventWithEnrollments
            {
                Enrollments = e.Select(ei => ei.BetroffenePerson),
                EventId = e.Key.Id,
                Name = e.Key.Bezeichnung,
                Location = e.Key.Ort
            })
            .ToListAsync();

        if (!schema.Verpflichtend) return attendances;

        var attending = attendances.SelectMany(e => e.Enrollments);
        var nonAttending = await _dbContext.Personen.Where(e => e.Rolle == Rolle.Mittelstufe && !attending.Contains(e))
            .ToListAsync();
        return attendances.Prepend(new EventWithEnrollments
        {
            Enrollments = nonAttending,
            EventId = Guid.Empty,
            Name = "Nicht eingeschrieben",
            Location = "FEHLEND"
        });
    }

    public async Task<Guid> GetEventForStudentAndSlot(Guid slotId, Guid studentId)
    {
        var enrollment = await _dbContext.OtiaEinschreibungen
            .Where(e => e.BetroffenePerson.Id == studentId && e.Termin.Block.Id == slotId)
            .OrderByDescending(e => e.Interval.Start)
            .Select(e => new { e.Termin.Id })
            .FirstOrDefaultAsync();

        return enrollment?.Id ?? Guid.Empty;
    }

    public async Task<IEnumerable<Person>> GetEnrollmentsForEvent(Guid eventId)
    {
        return await _dbContext.OtiaEinschreibungen.Where(e => e.Termin.Id == eventId)
            .Select(e => e.BetroffenePerson)
            .ToListAsync();
    }

    public async Task<Guid> GetSlotForEvent(Guid eventId)
    {
        var termin = await _dbContext.OtiaTermine.Select(e => new
            {
                OtiumId = e.Otium.Id,
                e.Id
            })
            .FirstAsync(e => e.Id == eventId);
        return termin.OtiumId;
    }

    public async Task<IEnumerable<Event>> GetEventsForSlot(Guid slotId)
    {
        var block = await _dbContext.Blocks.FindAsync(slotId);
        var schema = _blockHelper.Get(block!.SchemaId)!;
        var events = await _dbContext.OtiaTermine.Where(e => e.Block.Id == slotId)
            .Select(e => new Event
            {
                EventId = e.Id,
                Name = e.Bezeichnung,
                Location = e.Ort
            })
            .OrderBy(e => e.Location)
            .ThenBy(e => e.Name)
            .ToListAsync();
        if (!schema.Verpflichtend) return events;
        return events.Prepend(new Event
        {
            EventId = Guid.Empty,
            Location = "FEHLEND",
            Name = "Nicht eingeschrieben"
        });
    }

    public async Task<AttendanceSlotMetadata> GetMetadataForSlot(Guid slotId)
    {
        var block = await _dbContext.Blocks.FindAsync(slotId);
        var schema = _blockHelper.Get(block!.SchemaId)!;
        return new AttendanceSlotMetadata
        {
            EnableNotes = true,
            EnableMove = true,
            MoveNowIntervall = schema.Interval.ToDateTimeInterval(block.SchultagKey),
            MissingStudentsNotificationRecipients = _otiumConfiguration.MissingStudentsReport.Recipients,
            MissingStudentsNotificationTime = _otiumConfiguration.MissingStudentsReport.Enabled
                ? block.SchultagKey.ToDateTime(schema.Interval.Start.AddMinutes(30))
                : null
        };
    }

    public async Task MoveStudent(Guid studentId, Guid slotId, Guid eventId)
    {
        if (eventId == Guid.Empty)
        {
            var student = await _userService.GetUserByIdAsync(studentId);
            await _enrollmentService.UnenrollAsync(slotId, student, true);
            return;
        }

        await _enrollmentService.ForceMove(studentId, eventId);
    }

    public async Task MoveStudentNow(Guid studentId, Guid slotId, Guid eventId)
    {
        var current = await _dbContext.OtiaEinschreibungen
            .Where(e => e.BetroffenePerson.Id == studentId
                        && e.Termin.Block.Id == slotId)
            .OrderByDescending(e => e.Interval.Start)
            .Select(e => new { e.Termin.Id })
            .FirstOrDefaultAsync();
        if (current is null && eventId == Guid.Empty) return;
        await _enrollmentService.ForceMoveNow(studentId, current?.Id ?? Guid.Empty, eventId);
    }

    public async Task<bool> Authorize(Guid slotId, ClaimsPrincipal user)
    {
        var block = await _dbContext.Blocks.FindAsync(slotId);
        return Authorize(block!, user);
    }

    private bool Authorize(Block block, ClaimsPrincipal user)
    {
        if (user.HasClaim(AfraAppClaimTypes.GlobalPermission, nameof(GlobalPermission.Otiumsverantwortlich)))
            return true;

        var now = DateTime.Now;
        var timeInterval = _blockHelper.Get(block.SchemaId)!.Interval;
        var dateTimeInterval = timeInterval.ToDateTimeInterval(block.SchultagKey);
        var supervisionInterval = new DateTimeInterval(dateTimeInterval.Start.Subtract(TimeSpan.FromHours(1)),
            dateTimeInterval.Duration + TimeSpan.FromHours(2));

        return supervisionInterval.Contains(now);
    }

    public async Task<IEnumerable<AttendanceSlot>> GetAvailableSlots(ClaimsPrincipal user)
    {
        var today = DateOnly.FromDateTime(DateTime.Now);
        var blocksToday = await _dbContext.Blocks
            .AsNoTracking()
            .Where(b => b.SchultagKey == today)
            .OrderBy(b => b.SchemaId)
            .ToListAsync();

        return blocksToday.Where(b => Authorize(b, user))
            .Select(b =>
            {
                var schema = _blockHelper.Get(b.SchemaId)!;
                return new AttendanceSlot
                {
                    Scope = ScopeValue,
                    SlotId = b.Id,
                    Bezeichnung = schema.Bezeichnung
                };
            });
    }

    public async Task<IEnumerable<AttendanceSlot>> GetActiveSlots()
    {
        var today = DateOnly.FromDateTime(DateTime.Now);
        return await _dbContext.Blocks
            .AsNoTracking()
            .Where(b => b.SchultagKey == today)
            .OrderBy(b => b.SchemaId)
            .AsAsyncEnumerable()
            .Select(b => new
            {
                Block = b,
                Schema = _blockHelper.Get(b.SchemaId)!
            })
            .Where(e =>
            {
                var now = DateTime.Now;
                var timeInterval = e.Schema.Interval;
                var dateTimeInterval = timeInterval.ToDateTimeInterval(today);
                var supervisionInterval = new DateTimeInterval(dateTimeInterval.Start.Subtract(TimeSpan.FromHours(1)),
                    dateTimeInterval.Duration + TimeSpan.FromHours(2));
                return supervisionInterval.Contains(now);
            })
            .Select(e => new AttendanceSlot
            {
                Bezeichnung = e.Schema.Bezeichnung,
                Scope = ScopeValue,
                SlotId = e.Block.Id
            })
            .ToListAsync();
    }
}
