using Altafraner.AfraApp.Dashboard.Contracts;
using Altafraner.AfraApp.Dashboard.Contracts.Models;
using Altafraner.AfraApp.User.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Altafraner.AfraApp.Profundum.Services;

internal class ProfundumDashboardProvider : IDashboardProvider
{
    private readonly AfraAppContext _dbContext;

    public ProfundumDashboardProvider(AfraAppContext dbContext)
    {
        _dbContext = dbContext;
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
            Payload = new { EventId = e.Id },
            Occupancy = null
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
}
