using Altafraner.AfraApp.Attendance.Domain.Contracts;
using Altafraner.AfraApp.Attendance.Domain.Dto;
using Altafraner.AfraApp.Dashboard.Contracts;
using Altafraner.AfraApp.Dashboard.Contracts.Models;
using Altafraner.AfraApp.Otium.Domain.Models;
using Altafraner.AfraApp.Schuljahr.Domain.Models;
using Altafraner.AfraApp.User.Domain.Models;
using Altafraner.AfraApp.User.Services;
using Altafraner.Backbone.Utils;
using Microsoft.EntityFrameworkCore;

namespace Altafraner.AfraApp.Otium.Services;

internal class OtiumDashboardProvider : IDashboardProvider
{
    private readonly AfraAppContext _dbContext;
    private readonly BlockHelper _blockHelper;
    private readonly IAttendanceService _attendanceService;
    private readonly UserService _userService;
    private readonly RulesValidationService _rulesValidationService;

    public OtiumDashboardProvider(AfraAppContext dbContext,
        BlockHelper blockHelper,
        IAttendanceService attendanceService,
        UserService userService,
        RulesValidationService rulesValidationService)
    {
        _dbContext = dbContext;
        _blockHelper = blockHelper;
        _attendanceService = attendanceService;
        _userService = userService;
        _rulesValidationService = rulesValidationService;
    }

    public string Scope => "Otium";

    public async Task<IEnumerable<DashboardTutorEventDescriptor>> GetTutorEvents(Person tutor,
        DateOnly start,
        DateOnly end)
    {
        var termine = await _dbContext.OtiaTermine
            .Include(t => t.Otium)
            .Include(t => t.Block)
            .ThenInclude(b => b.Schultag)
            .OrderBy(t => t.Block.Schultag.Datum)
            .ThenBy(t => t.Block.SchemaId)
            .Where(t => !t.IstAbgesagt && t.Tutor != null && t.Tutor.Id == tutor.Id &&
                        t.Block.SchultagKey >= start && t.Block.SchultagKey < end)
            .Select(t => new { original = t, enrollmentCount = t.Enrollments.Count })
            .ToListAsync();

        return termine.Select(e =>
        {
            var schema = _blockHelper.Get(e.original.Block.SchemaId);
            return new DashboardTutorEventDescriptor
            {
                Label = e.original.Bezeichnung,
                Start = e.original.Block.SchultagKey.ToDateTime(schema!.Interval.Start),
                SlotLabel = schema.Bezeichnung,
                Payload = new
                {
                    TerminId = e.original.Id
                },
                Occupancy = e.original.MaxEinschreibungen is null
                    ? null
                    : (float)e.enrollmentCount / e.original.MaxEinschreibungen.Value,
                Location = e.original.Ort
            };
        });
    }

    public async Task<Dictionary<Guid, Dictionary<DateOnly, DashboardMenteeStatus>>> GetMenteeStatus(Person[] mentees,
        DateOnly[] weeks)
    {
        var startDate = weeks.Min();
        var endDate = weeks.Max().AddDays(7);

        var enrollments = await _dbContext.OtiaEinschreibungen
            .Where(e => mentees.Contains(e.BetroffenePerson))
            .Where(e => e.Termin.Block.Schultag.Datum >= startDate && e.Termin.Block.Schultag.Datum < endDate)
            .Include(p => p.Termin)
            .ThenInclude(p => p.Block)
            .ThenInclude(b => b.Schultag)
            .Include(e => e.Termin)
            .ThenInclude(t => t.Otium)
            .ThenInclude(o => o.Kategorie)
            .GroupBy(e => e.BetroffenePerson.Id)
            .ToDictionaryAsync(e => e.Key, e => e.AsEnumerable());

        var schultage = await _dbContext.Schultage
            .Include(s => s.Blocks)
            .Where(s => s.Datum >= startDate && s.Datum < endDate)
            .ToListAsync();

        var blocks = schultage.SelectMany(e => e.Blocks);

        var allTermine = await _dbContext.OtiaTermine
            .Include(t => t.Otium)
            .ThenInclude(e => e.Kategorie)
            .Where(e => blocks.Contains(e.Block))
            .ToListAsync();

        var attendanceIds = from block in schultage.SelectMany(e => e.Blocks)
            from student in mentees
            select new AttendanceEntryId
            {
                Scope = OtiumAttendanceInformationProvider.ScopeValue,
                SlotId = block.Id,
                StudentId = student.Id
            };
        var attendances = await _attendanceService.GetAttendances(attendanceIds);

        Dictionary<Guid, Dictionary<DateOnly, DashboardMenteeStatus>> results = [];

        foreach (var mentee in mentees)
        {
            var menteeResults = new Dictionary<DateOnly, DashboardMenteeStatus>();
            results[mentee.Id] = menteeResults;
            var menteesEnrollments = enrollments.GetValueOrDefault(mentee.Id, []).ToArray();
            var klassenstufe = _userService.GetKlassenstufe(mentee);
            var menteesTermine = allTermine
                .Where(e =>
                    (e.Otium.MinKlasse == null || e.Otium.MinKlasse <= klassenstufe) &&
                    (e.Otium.MaxKlasse == null || e.Otium.MaxKlasse >= klassenstufe))
                .ToList();
            foreach (var week in weeks)
                menteeResults[week] = await GetMenteeStatusForWeek(mentee, menteesEnrollments, week, menteesTermine);
        }

        return results;

        async Task<DashboardMenteeStatus> GetMenteeStatusForWeek(Person mentee,
            OtiumEinschreibung[] menteesEnrollments,
            DateOnly monday,
            List<OtiumTermin> termine)
        {
            if (mentee.Rolle != Rolle.Mittelstufe) return DashboardMenteeStatus.NotApplicable;

            var endOfWeek = monday.AddDays(7);
            var schultageInWeek = schultage.Where(s =>
                    s.Datum >= monday && s.Datum < endOfWeek)
                .ToList();
            var blocksInWeek = schultageInWeek.SelectMany(e => e.Blocks);
            var menteesTermineInWeek = termine.Where(e => blocksInWeek.Contains(e.Block)).ToList();
            if (schultageInWeek.Count == 0) return DashboardMenteeStatus.NotApplicable;

            var studentsAttendancesInWeek = schultageInWeek.SelectMany(e => e.Blocks)
                .ToDictionary(e => e.Id,
                    e => attendances[new AttendanceEntryId
                    {
                        Scope = OtiumAttendanceInformationProvider.ScopeValue,
                        SlotId = e.Id,
                        StudentId = mentee.Id
                    }]
                );

            var weeksMessages = await _rulesValidationService.GetMessagesForWeekAsync(mentee,
                schultageInWeek,
                menteesTermineInWeek,
                menteesEnrollments.Where(e => schultageInWeek.Contains(e.Termin.Block.Schultag)).ToList(),
                studentsAttendancesInWeek);
            if (weeksMessages.Count > 0) return DecideBetweenOpenAndConspicuous(schultageInWeek);

            foreach (var schultag in schultageInWeek)
            {
                var studentsAttendancesOnDay = schultag.Blocks
                    .ToDictionary(e => e.Id,
                        e => attendances[new AttendanceEntryId
                        {
                            Scope = OtiumAttendanceInformationProvider.ScopeValue,
                            SlotId = e.Id,
                            StudentId = mentee.Id
                        }]);
                var daysMessages = await _rulesValidationService.GetMessagesForDayAsync(mentee,
                    schultag,
                    menteesEnrollments.Where(e => e.Termin.Block.Schultag == schultag).ToList(),
                    studentsAttendancesOnDay);
                if (daysMessages.Count > 0) return DecideBetweenOpenAndConspicuous(schultageInWeek);
            }

            var enrollmentsMessages =
                await _rulesValidationService.GetMessagesForEnrollmentsAsync(mentee, menteesEnrollments.ToList());
            return enrollmentsMessages.Count > 0
                ? DecideBetweenOpenAndConspicuous(schultageInWeek)
                : DashboardMenteeStatus.Valid;
        }

        DashboardMenteeStatus DecideBetweenOpenAndConspicuous(List<Schultag> daysInWeek)
        {
            var today = DateOnly.FromDateTime(DateTime.Now);
            var lastDayWithBlocks = daysInWeek.Where(s => s.Blocks.Count > 0).MaxBy(s => s.Datum)?.Datum;
            if (lastDayWithBlocks is null) return DashboardMenteeStatus.NotApplicable;
            if (lastDayWithBlocks >= today) return DashboardMenteeStatus.Uncertain;
            return DashboardMenteeStatus.Invalid;
        }
    }

    public async Task<DashboardStudentOverview> GetStudentOverview(Person student, DateOnly start, int weeks)
    {
        var dailyMessages = new Dictionary<DateOnly, List<string>>();
        var weeklyMessages = new Dictionary<DateOnly, List<string>>();
        var end = start.AddDays((weeks + 1) * 7);

        var schultage = await _dbContext.Schultage
            .Include(s => s.Blocks)
            .Where(s => s.Datum >= start && s.Datum < end)
            .OrderBy(s => s.Datum)
            .ToListAsync();

        var blocks = schultage.SelectMany(s => s.Blocks);
        var klassenstufe = _userService.GetKlassenstufe(student);
        var termine = await _dbContext.OtiaTermine
            .Include(t => t.Otium)
            .ThenInclude(e => e.Kategorie)
            .Where(e => blocks.Contains(e.Block))
            .Where(e =>
                (e.Otium.MinKlasse == null || e.Otium.MinKlasse <= klassenstufe) &&
                (e.Otium.MaxKlasse == null || e.Otium.MaxKlasse >= klassenstufe))
            .ToListAsync();

        var einschreibungen = await _dbContext.OtiaEinschreibungen
            .Where(e => e.BetroffenePerson.Id == student.Id)
            .Include(e => e.Termin)
            .ThenInclude(e => e.Block)
            .ThenInclude(e => e.Schultag)
            .Include(e => e.Termin)
            .ThenInclude(e => e.Tutor)
            .Include(e => e.Termin)
            .ThenInclude(e => e.Otium)
            .ThenInclude(e => e.Kategorie)
            .OrderBy(s => s.Termin.Block.SchultagKey)
            .ThenBy(s => s.Termin.Block.SchemaId)
            .Where(e => schultage.Contains(e.Termin.Block.Schultag))
            .ToListAsync();

        var schultageByWeek = schultage.GroupBy(s => s.Datum.GetStartOfWeek());
        var attendanceIds = schultage.SelectMany(s => s.Blocks)
            .Select(e => new AttendanceEntryId
            {
                Scope = OtiumAttendanceInformationProvider.ScopeValue,
                SlotId = e.Id,
                StudentId = student.Id
            });
        var attendancesByBlock =
            (await _attendanceService.GetAttendances(attendanceIds)).ToDictionary(e => e.Key.SlotId, e => e.Value);

        await PopulateMessages();

        List<DashboardStudentEventDescriptor> studentEvents = [];

        foreach (var schultag in schultage)
        {
            var einschreibungenForDay =
                einschreibungen.TakeWhile(e => e.Termin.Block.SchultagKey == schultag.Datum).ToArray();
            einschreibungen.RemoveRange(0, einschreibungenForDay.Length);
            foreach (var block in schultag.Blocks)
            {
                var isBlockDoneOrRunning =
                    _blockHelper.GetBlockStatus(block) is BlockHelper.BlockStatus.Done
                        or BlockHelper.BlockStatus.Running;
                var einschreibungenForBlock = einschreibungenForDay.Where(e => e.Termin.Block.Id == block.Id).ToArray();
                var schema = _blockHelper.Get(block.SchemaId)!;
                if (einschreibungenForBlock.Length == 0)
                {
                    studentEvents.Add(new DashboardStudentEventDescriptor
                    {
                        Label = null,
                        Start = schultag.Datum.ToDateTime(schema.Interval.Start),
                        SlotLabel = schema.Bezeichnung,
                        Payload = new
                        {
                            Started = isBlockDoneOrRunning
                        },
                        Attendance = isBlockDoneOrRunning && schema.Verpflichtend
                            ? attendancesByBlock[block.Id].State
                            : null,
                        Location = null
                    });
                    continue;
                }

                foreach (var einschreibung in einschreibungenForBlock)
                    studentEvents.Add(new DashboardStudentEventDescriptor
                    {
                        Label = einschreibung.Termin.Bezeichnung,
                        Start = schultag.Datum.ToDateTime(einschreibung.Interval.Start),
                        SlotLabel = schema.Bezeichnung,
                        Payload = new
                        {
                            CategoryId = einschreibung.Termin.Otium.Kategorie.Id,
                            TerminId = einschreibung.Termin.Id,
                            Started = isBlockDoneOrRunning
                        },
                        Attendance = isBlockDoneOrRunning ? attendancesByBlock[block.Id].State : null,
                        Location = einschreibung.Termin.Ort
                    });
            }
        }

        return new DashboardStudentOverview
        {
            DailyWarnings = dailyMessages,
            WeeklyWarnings = weeklyMessages,
            Events = studentEvents
        };

        async Task PopulateMessages()
        {
            var localEinschreibungen = einschreibungen.ToList();
            foreach (var week in schultageByWeek)
            {
                var blocksInWeek = week.SelectMany(e => e.Blocks);
                var weekEnd = week.Key.AddDays(7);

                // Increase performance by taking from the already sorted list of enrollments, then removing them from the list before the next iteration.
                var einschreibungenForWeek = localEinschreibungen
                    .TakeWhile(e => e.Termin.Block.SchultagKey < weekEnd)
                    .ToList();
                localEinschreibungen.RemoveRange(0, einschreibungenForWeek.Count);

                var attendancesInWeek =
                    week.SelectMany(e => e.Blocks).ToDictionary(e => e.Id, e => attendancesByBlock[e.Id]);

                foreach (var schultag in week)
                {
                    var attendancesOnDay = schultag.Blocks.ToDictionary(e => e.Id, e => attendancesByBlock[e.Id]);
                    var messagesForBlocksOnDay =
                        await _rulesValidationService.GetMessagesForDayAsync(student,
                            schultag,
                            einschreibungenForWeek,
                            attendancesOnDay);
                    if (messagesForBlocksOnDay.Count == 0) continue;
                    dailyMessages[schultag.Datum] = messagesForBlocksOnDay;
                }

                var termineInWeek = termine.Where(e => blocksInWeek.Contains(e.Block)).ToList();

                var messagesForWeek =
                    await _rulesValidationService.GetMessagesForWeekAsync(student,
                        week.ToList(),
                        termineInWeek,
                        einschreibungenForWeek,
                        attendancesInWeek);
                weeklyMessages[week.Key] = messagesForWeek;
            }
        }
    }
}
