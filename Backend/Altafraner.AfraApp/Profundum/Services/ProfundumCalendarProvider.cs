using Altafraner.AfraApp.Calendar;
using Altafraner.AfraApp.User.Domain.Models;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Microsoft.EntityFrameworkCore;

namespace Altafraner.AfraApp.Profundum.Services;

///<summary>A <see href="ICalendarProvider"/> serving profundum events taught or enrolled in</summary>
public class ProfundumCalendarProvider : ICalendarProvider
{
    private readonly AfraAppContext _dbContext;

    /// <summary>Construct the service. Called by the DI container</summary>
    public ProfundumCalendarProvider(AfraAppContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <inheritdoc/>
    public IEnumerable<CalendarEvent> GetEventsForPerson(Person person)
    {
        var enrollments = _dbContext.ProfundaEinschreibungen
            .Where(e => e.IsFixed)
            .Where(e => e.BetroffenePerson == person)
            .Where(e => e.ProfundumInstanz != null)
            .Include(e => e.ProfundumInstanz).ThenInclude(i => i!.Slots).ThenInclude(s => s.Termine);
        var enrolledEvents = enrollments
            .SelectMany(e => e.Slot.Termine
            .Select(t => new CalendarEvent
            {
                Summary = e.ProfundumInstanz!.Profundum.Bezeichnung,
                Description = e.ProfundumInstanz!.Profundum.Beschreibung,
                Location = e.ProfundumInstanz!.Ort,
                Start = new CalDateTime(new DateTime(t.Day, t.StartTime), true),
                End = new CalDateTime(new DateTime(t.Day, t.EndTime), true),
                LastModified = new CalDateTime(new[] { e.LastModified, e.ProfundumInstanz.LastModified, e.ProfundumInstanz.Profundum.LastModified }.Max(), true),
                Created = new CalDateTime(e.CreatedAt, true)
            })).AsEnumerable();

        var teaching = _dbContext.ProfundaInstanzen
            .Where(i => i.Verantwortliche.Contains(person))
            .Include(i => i!.Slots).ThenInclude(s => s.Termine);
        var taughtEvents = teaching
            .SelectMany(i => i.Slots
            .SelectMany(s => s.Termine
            .Select(t => new CalendarEvent
            {
                Summary = i.Profundum.Bezeichnung,
                Description = i.Profundum.Beschreibung,
                Location = i.Ort,
                Start = new CalDateTime(new DateTime(t.Day, t.StartTime), true),
                End = new CalDateTime(new DateTime(t.Day, t.EndTime), true),
                LastModified = new CalDateTime(new[] { i.LastModified, i.Profundum.LastModified }.Max(), true),
                Created = new CalDateTime(i.CreatedAt, true)
            }))).AsEnumerable();
        return taughtEvents.Concat(enrolledEvents);
    }
}
