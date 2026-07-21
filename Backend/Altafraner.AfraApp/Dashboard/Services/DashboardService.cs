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
}
