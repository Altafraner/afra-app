using Altafraner.AfraApp.Attendance.Domain.Contracts;
using Altafraner.AfraApp.Attendance.Domain.Dto;
using Altafraner.AfraApp.Dashboard.Contracts;
using Altafraner.AfraApp.Dashboard.Contracts.Models;
using Altafraner.AfraApp.Profundum.Domain.Models;
using Altafraner.AfraApp.User.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Altafraner.AfraApp.Profundum.Services;

internal class ProfundumDashboardProvider : IDashboardProvider
{
    private readonly AfraAppContext _dbContext;
    private readonly IAttendanceService _attendanceService;

    public ProfundumDashboardProvider(AfraAppContext dbContext, IAttendanceService attendanceService)
    {
        _dbContext = dbContext;
        _attendanceService = attendanceService;
    }

    public string Scope => "Profundum";

    public async Task<IEnumerable<DashboardTutorEventDescriptor>> GetTutorEvents(Person tutor,
        DateOnly start,
        DateOnly end)
    {
        var termine = await _dbContext.ProfundaTermine
            .Include(e => e.Slot)
            .ThenInclude(e => e.Angebote.Where(a => a.Verantwortliche.Contains(tutor)))
            .ThenInclude(a => a.Profundum)
            .Where(e => e.Day >= start && e.Day <= end)
            .Where(e => e.Slot.Angebote.Any(a => a.Verantwortliche.Contains(tutor)))
            .ToListAsync();
        return termine.SelectMany(e => e.Slot.Angebote.Select(a => new DashboardTutorEventDescriptor
        {
            Label = a.Profundum.Bezeichnung,
            Start = e.Day.ToDateTime(e.StartTime),
            SlotLabel = "Profundum",
            Payload = new
            {
                SlotId = e.Id,
                EventId = a.Id
            },
            Occupancy = null,
            Location = a.Ort
        }));
    }

    public Task<Dictionary<Guid, Dictionary<DateOnly, DashboardMenteeStatus>>> GetMenteeStatus(Person[] mentees,
        DateOnly[] weeks)
    {
        Dictionary<Guid, Dictionary<DateOnly, DashboardMenteeStatus>> results = [];
        foreach (var mentee in mentees)
        {
            Dictionary<DateOnly, DashboardMenteeStatus> menteeResults = [];
            results[mentee.Id] = menteeResults;
            foreach (var week in weeks) menteeResults[week] = DashboardMenteeStatus.NotApplicable;
        }

        return Task.FromResult(results);
    }

    public async Task<DashboardStudentOverview> GetStudentOverview(Person student, DateOnly start, int weeks)
    {
        var now = DateTime.Now;
        var endDate = start.AddDays(7 * weeks);

        var termine = await _dbContext.ProfundaTermine
            .Include(e => e.Slot)
            .Where(e => e.Day >= start && e.Day < endDate)
            .ToListAsync();
        var slots = termine.Select(e => e.Slot).Distinct();
        var enrollments = await _dbContext.ProfundaEinschreibungen
            .Include(e => e.ProfundumInstanz)
            .ThenInclude(e => e!.Profundum)
            .Where(e => e.BetroffenePersonId == student.Id && slots.Contains(e.Slot) && e.IsFixed)
            .ToDictionaryAsync(e => e.SlotId);

        List<(ProfundumTermin termin, ProfundumEinschreibung einschreibung)> enrollmentsWithDates = [];
        List<AttendanceEntryId> entryIds = [];
        foreach (var termin in termine)
        {
            if (!enrollments.TryGetValue(termin.Slot.Id, out var enrollment) ||
                enrollment.ProfundumInstanz is null) continue;
            enrollmentsWithDates.Add((termin, enrollment));
            if (IsInFuture(termin)) continue;
            entryIds.Add(new AttendanceEntryId
            {
                Scope = ProfundumAttendanceInformationProvider.ScopeValue,
                SlotId = termin.Id,
                StudentId = student.Id
            });
        }

        var attendances = await _attendanceService.GetAttendances(entryIds);
        var attendanceByBlock = attendances.ToDictionary(e => e.Key.SlotId, e => e.Value.State);

        List<DashboardStudentEventDescriptor> results = [];

        foreach (var (termin, enrollment) in enrollmentsWithDates)
            results.Add(new DashboardStudentEventDescriptor
            {
                Label = enrollment.ProfundumInstanz!.Profundum.Bezeichnung,
                Start = termin.Day.ToDateTime(termin.StartTime),
                SlotLabel = "Profundum",
                Payload = new { },
                Location = enrollment.ProfundumInstanz.Ort,
                Attendance = attendanceByBlock.TryGetValue(termin.Id, out var value) ? value : null
            });

        return new DashboardStudentOverview
        {
            Events = results,
            DailyWarnings = [],
            WeeklyWarnings = []
        };

        bool IsInFuture(ProfundumTermin termin)
        {
            return termin.Day.ToDateTime(termin.StartTime) > now;
        }
    }
}
