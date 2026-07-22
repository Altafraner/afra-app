using System.Diagnostics;
using Altafraner.AfraApp.Dashboard.Contracts;
using Altafraner.AfraApp.Dashboard.Contracts.DTO;
using Altafraner.AfraApp.Dashboard.Contracts.Models;
using Altafraner.AfraApp.User.Domain.DTO;
using Altafraner.AfraApp.User.Domain.Models;
using Altafraner.Backbone.Utils;
using Person = Altafraner.AfraApp.User.Domain.Models.Person;

namespace Altafraner.AfraApp.Dashboard.Services;

internal class DashboardService
{
    private readonly IDashboardProvider[] _providers;

    public DashboardService(IEnumerable<IDashboardProvider> providers)
    {
        _providers = providers.ToArray();
    }

    public async Task<IEnumerable<ScopedDashboardTutorEventDescriptor>> GetTutorDashboard(Person person)
    {
        var now = DateTime.Now;
        var today = DateOnly.FromDateTime(now);
        var endOfInterval = DateOnly.FromDayNumber(today.DayNumber + 21);

        var termine = new List<ScopedDashboardTutorEventDescriptor>();

        foreach (var provider in _providers)
        {
            var providerEvents = await provider.GetTutorEvents(person, today, endOfInterval);
            termine.AddRange(providerEvents.Select(e =>
                ScopedDashboardTutorEventDescriptor.FromDashboardTutorEvent(e, provider.Scope)));
        }

        return termine
            .OrderBy(e => e.Start)
            .ThenBy(e => e.Label);
    }

    public async Task<IEnumerable<DashboardMenteeOverview>> GetMenteeStatuses(Person[] mentees)
    {
        Dictionary<Guid, Dictionary<DateOnly, DashboardMenteeStatus>> results = [];

        var current = DateOnly.FromDateTime(DateTime.Now).GetStartOfWeek();
        var last = current.AddDays(-7);
        var next = current.AddDays(7);
        DateOnly[] weeksArray = [last, current, next];

        foreach (var mentee in mentees)
            results[mentee.Id] = new Dictionary<DateOnly, DashboardMenteeStatus>
            {
                [last] = DashboardMenteeStatus.NotApplicable,
                [current] = DashboardMenteeStatus.NotApplicable,
                [next] = DashboardMenteeStatus.NotApplicable
            };

        foreach (var provider in _providers)
        {
            var providerResults = await provider.GetMenteeStatus(mentees, weeksArray);
            foreach (var (menteeId, statusByWeek) in providerResults)
            {
                var menteeResults = results[menteeId];
                foreach (var (week, status) in statusByWeek) menteeResults[week] = menteeResults[week].Merge(status);
            }
        }

        List<DashboardMenteeOverview> dashboardElements = [];
        foreach (var mentee in mentees)
        {
            var menteeResults = results[mentee.Id];
            dashboardElements.Add(new DashboardMenteeOverview
            {
                Mentee = new PersonInfoMinimal(mentee),
                Last = menteeResults[last],
                Current = menteeResults[current],
                Next = menteeResults[next]
            });
        }

        return dashboardElements
            .OrderBy(s => s.Mentee.Rolle switch
            {
                Rolle.Mittelstufe => 0,
                Rolle.Oberstufe => 1,
                _ => -1
            })
            .ThenBy(s => s.Mentee.Vorname)
            .ThenBy(s => s.Mentee.Nachname);
    }

    public async Task<List<StudentWeek>> GetStudentWeeks(Person user, DateOnly start, int numWeeks)
    {
        Debug.Assert(start.GetStartOfWeek() == start, "Must be monday!");

        Dictionary<DateOnly, List<string>> dailyWarnings = [];
        Dictionary<DateOnly, List<string>> weeklyWarnings = [];
        List<ScopedDashboardStudentEventDescriptor> events = [];

        foreach (var provider in _providers)
        {
            var providerData = await provider.GetStudentOverview(user, start, numWeeks);
            foreach (var (date, warnings) in providerData.DailyWarnings)
            {
                if (!dailyWarnings.ContainsKey(date))
                    dailyWarnings[date] = [];
                dailyWarnings[date].AddRange(warnings);
            }

            foreach (var (date, warnings) in providerData.WeeklyWarnings)
            {
                if (!weeklyWarnings.ContainsKey(date))
                    weeklyWarnings[date] = [];
                weeklyWarnings[date].AddRange(warnings);
            }

            events.AddRange(
                providerData.Events.Select(e => new ScopedDashboardStudentEventDescriptor(e, provider.Scope)));
        }

        List<StudentWeek> weeks = [];
        for (var i = 0; i < numWeeks; i++)
        {
            var currWeek = start.AddDays(i * 7);
            var currWeekEnd = currWeek.AddDays(7);

            var weekWarnings = weeklyWarnings.GetValueOrDefault(currWeek, []);
            var dayWarningsInWeek = dailyWarnings
                .Where(e => e.Key >= currWeek && e.Key < currWeekEnd)
                .ToDictionary(e => e.Key, e => e.Value);

            var weekEvents = events.Where(e =>
                {
                    var date = DateOnly.FromDateTime(e.Start);
                    return date >= currWeek && date < currWeekEnd;
                })
                .OrderBy(e => e.Start)
                .ToArray();

            if (weekWarnings.Count > 0 || dayWarningsInWeek.Count > 0 || weekEvents.Length > 0)
                weeks.Add(new StudentWeek
                {
                    Monday = currWeek,
                    Warnings = weekWarnings,
                    DailyWarnings = dayWarningsInWeek,
                    Events = weekEvents
                });
        }

        return weeks;
    }
}
