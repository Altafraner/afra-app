using System.Security.Claims;
using Altafraner.AfraApp.Attendance.Domain.Contracts;
using Altafraner.AfraApp.Attendance.Domain.Dto;
using Altafraner.AfraApp.Attendance.Domain.Dto.Enrollments;
using Altafraner.AfraApp.Attendance.Domain.Models;
using Altafraner.AfraApp.Backbone.Authorization;
using Altafraner.AfraApp.Domain.TimeInterval;
using Altafraner.AfraApp.Otium.Configuration;
using Altafraner.AfraApp.Otium.Domain.Models;
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

    // This function could propab. be heavily optimized. I'm not doing that just yet.
    public async Task<IEnumerable<EventWithEnrollments>> GetEnrollmentsForSlot(Guid slotId)
    {
        var block = await _dbContext.Blocks.FindAsync(slotId);
        var schema = _blockHelper.Get(block!.SchemaId)!;

        var now = DateTime.Now;
        var nowTime = TimeOnly.FromDateTime(now);
        var blockStatus = _blockHelper.GetBlockStatus(block);

        var attendancesQuery = BuildStatusFilteredEnrollmentsQuery(slotId, blockStatus, nowTime, schema.Interval.End);
        attendancesQuery = blockStatus switch
        {
            BlockHelper.BlockStatus.Running => SelectOneEnrollmentPerStudentByStart(attendancesQuery,
                true),
            BlockHelper.BlockStatus.Done => SelectOneEnrollmentPerStudentByStart(attendancesQuery,
                false),
            _ => attendancesQuery
        };

        var attendanceRows = await attendancesQuery
            .Include(e => e.Termin)
            .ThenInclude(e => e.Otium)
            .Include(e => e.BetroffenePerson)
            .Select(e => new
            {
                e.Termin,
                e.BetroffenePerson
            })
            .ToListAsync();

        // For some reason we don't like ordering in the db.
        var attendances = attendanceRows
            .GroupBy(e => e.Termin.Id)
            .Select(g => new
            {
                g.First().Termin,
                Enrollments = g.Select(x => x.BetroffenePerson).OrderBy(e => e.LastName).ThenBy(e => e.FirstName)
            })
            .OrderBy(e => e.Termin.Ort)
            .ThenBy(e => e.Termin.Bezeichnung)
            .Select(e => new EventWithEnrollments
            {
                Enrollments = e.Enrollments,
                EventId = e.Termin.Id,
                Name = e.Termin.Bezeichnung,
                Location = e.Termin.Ort
            })
            .ToArray();

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
        var block = await _dbContext.Blocks.FindAsync(slotId);
        if (block is null)
            throw new KeyNotFoundException("Block not found");
        var schema = _blockHelper.Get(block.SchemaId)!;
        var now = DateTime.Now;
        var nowTime = TimeOnly.FromDateTime(now);
        var blockStatus = _blockHelper.GetBlockStatus(block);

        var enrollmentQuery = BuildStatusFilteredEnrollmentsQuery(slotId, blockStatus, nowTime, schema.Interval.End)
            .Where(e => e.BetroffenePerson.Id == studentId);

        enrollmentQuery = blockStatus switch
        {
            BlockHelper.BlockStatus.Running => enrollmentQuery
                .OrderByDescending(e => e.Interval.Start),
            BlockHelper.BlockStatus.Done => enrollmentQuery
                .OrderBy(e => e.Interval.Start),
            _ => enrollmentQuery
                .OrderBy(e => e.Interval.Start)
        };

        var enrollment = await enrollmentQuery
            .Select(e => new { e.Termin.Id })
            .FirstOrDefaultAsync();

        return enrollment?.Id ?? Guid.Empty;
    }

    public async Task<IEnumerable<Person>> GetEnrollmentsForEvent(Guid slotId, Guid eventId)
    {
        var block = await _dbContext.Blocks.FindAsync(slotId);
        if (block is null)
            throw new KeyNotFoundException("Block not found");

        var schema = _blockHelper.Get(block.SchemaId)!;
        var nowTime = TimeOnly.FromDateTime(DateTime.Now);
        var blockStatus = _blockHelper.GetBlockStatus(block);

        var selectedEnrollments =
            BuildStatusFilteredEnrollmentsQuery(slotId, blockStatus, nowTime, schema.Interval.End);
        selectedEnrollments = blockStatus switch
        {
            BlockHelper.BlockStatus.Running => SelectOneEnrollmentPerStudentByStart(selectedEnrollments,
                true),
            BlockHelper.BlockStatus.Done => SelectOneEnrollmentPerStudentByStart(selectedEnrollments,
                false),
            _ => selectedEnrollments
        };

        if (eventId != Guid.Empty)
            return await selectedEnrollments.Where(e => e.Termin.Id == eventId)
                .Select(e => e.BetroffenePerson)
                .OrderBy(e => e.LastName)
                .ThenBy(e => e.FirstName)
                .ToListAsync();

        return await _dbContext.Personen.LeftJoin(
                selectedEnrollments,
                e => e.Id,
                e => e.BetroffenePerson.Id,
                (p, e) => new { Person = p, Einschreibung = e })
            .Where(e => e.Einschreibung == null && e.Person.Rolle == Rolle.Mittelstufe)
            .Select(e => e.Person)
            .ToListAsync();
    }

    public async Task<IEnumerable<Event>> GetEventsForSlot(Guid slotId)
    {
        var block = await _dbContext.Blocks.FindAsync(slotId);
        var schema = _blockHelper.Get(block!.SchemaId)!;
        var events = await _dbContext.OtiaTermine.Where(e => e.Block.Id == slotId)
            .Select(e => new Event
            {
                EventId = e.Id,
                Name = e.OverrideBezeichnung ?? e.Otium.Bezeichnung,
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
        var block = await _dbContext.Blocks.FindAsync(slotId);
        if (block is null)
            throw new KeyNotFoundException("Block not found");

        var now = DateTime.Now;
        var nowTime = TimeOnly.FromDateTime(now);
        var blockStatus = _blockHelper.GetBlockStatus(block);
        if (blockStatus != BlockHelper.BlockStatus.Running)
            throw new InvalidOperationException("Block is not running");

        var current =
            await BuildStatusFilteredEnrollmentsQuery(slotId, BlockHelper.BlockStatus.Running, nowTime, nowTime)
                .Where(e => e.BetroffenePerson.Id == studentId)
            .OrderByDescending(e => e.Interval.Start)
            .Select(e => new { e.Termin.Id })
            .FirstOrDefaultAsync();
        if (current is null && eventId == Guid.Empty) return;
        await _enrollmentService.ForceMoveNow(studentId, current?.Id ?? Guid.Empty, eventId);
    }

    private IQueryable<OtiumEinschreibung> BuildStatusFilteredEnrollmentsQuery(
        Guid slotId,
        BlockHelper.BlockStatus blockStatus,
        TimeOnly nowTime,
        TimeOnly schemaEnd)
    {
        var baseQuery = _dbContext.OtiaEinschreibungen
            .Where(e => e.Termin.Block.Id == slotId);

        return blockStatus switch
        {
            BlockHelper.BlockStatus.Running => baseQuery
                .Where(ei => ei.Interval.Start <= nowTime && ei.Interval.Start.Add(ei.Interval.Duration) > nowTime),
            BlockHelper.BlockStatus.Done => baseQuery
                .Where(ei => ei.Interval.Start.Add(ei.Interval.Duration) >= schemaEnd),
            _ => baseQuery
        };
    }

    private static IQueryable<OtiumEinschreibung> SelectOneEnrollmentPerStudentByStart(
        IQueryable<OtiumEinschreibung> candidates,
        bool pickLatestStart)
    {
        var selectedStartPerStudent = candidates
            .GroupBy(e => e.BetroffenePerson.Id)
            .Select(g => new
            {
                StudentId = g.Key,
                Start = pickLatestStart
                    ? g.Max(e => e.Interval.Start)
                    : g.Min(e => e.Interval.Start)
            });

        var withSelectedStart = candidates.Join(
            selectedStartPerStudent,
            e => new { StudentId = e.BetroffenePerson.Id, e.Interval.Start },
            s => new { s.StudentId, s.Start },
            (e, _) => e);

        // Overlapping enrollments are not expected. If they happen with identical starts,
        // we intentionally do not define tie-breaking here.
        return withSelectedStart;
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
                    Label = schema.Bezeichnung
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
                Label = e.Schema.Bezeichnung,
                Scope = ScopeValue,
                SlotId = e.Block.Id
            })
            .ToListAsync();
    }
}
