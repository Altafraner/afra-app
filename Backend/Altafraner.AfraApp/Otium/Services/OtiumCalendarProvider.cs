using Altafraner.AfraApp.Calendar;
using Altafraner.AfraApp.User.Domain.Models;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Microsoft.EntityFrameworkCore;

namespace Altafraner.AfraApp.Otium.Services;

///<summary>A <see href="ICalendarProvider"/> serving otium events taught or enrolled in</summary>
public class OtiumCalendarProvider : ICalendarProvider
{
    private readonly AfraAppContext _dbContext;
    private readonly BlockHelper _blockHelper;

    /// <summary>Construct the service. Called by the DI container</summary>
    public OtiumCalendarProvider(AfraAppContext dbContext, BlockHelper blockHelper)
    {
        _dbContext = dbContext;
        _blockHelper = blockHelper;
    }

    /// <inheritdoc/>
    public IEnumerable<CalendarEvent> GetEventsForPerson(Person person)
    {
        var enrollments = _dbContext.OtiaEinschreibungen
            .Where(e => e.BetroffenePerson == person)
            .Include(e => e.Termin).ThenInclude(t => t.Otium)
            .Include(e => e.Termin).ThenInclude(t => t.Block).ThenInclude(b => b.Schultag);
        var enrolledEvents = enrollments.Select(e => new CalendarEvent
        {
            Uid = e.Id.ToString(),
            Summary = e.Termin.Bezeichnung,
            Description = e.Termin.Beschreibung,
            Location = e.Termin.Ort,
            Start = new CalDateTime(new DateTime(e.Termin.Block.Schultag.Datum, e.Interval.Start), true),
            End = new CalDateTime(new DateTime(e.Termin.Block.Schultag.Datum, e.Interval.End), true),
            LastModified =
                new CalDateTime(
                    new[] { e.LastModified, e.Termin.LastModified, e.Termin.Otium.LastModified }.Max(),
                    true),
            Created = new CalDateTime(e.CreatedAt, true)
        }).AsEnumerable();

        var taught = _dbContext.OtiaTermine
            .Where(e => e.Tutor != null && e.Tutor == person)
            .Include(t => t.Otium)
            .Include(t => t.Block).ThenInclude(b => b.Schultag);
        var taughtEvents = taught.Select(e => new CalendarEvent
        {
            Uid = e.Id.ToString(),
            Summary = e.Bezeichnung,
            Description = e.Beschreibung,
            Location = e.Ort,
            Start = new CalDateTime(
                new DateTime(e.Block.Schultag.Datum, _blockHelper.Get(e.Block.SchemaId)!.Interval.Start), true),
            End = new CalDateTime(
                new DateTime(e.Block.Schultag.Datum, _blockHelper.Get(e.Block.SchemaId)!.Interval.End), true),
            LastModified = new CalDateTime(new[] { e.LastModified, e.Otium.LastModified }.Max(), true),
            Created = new CalDateTime(e.CreatedAt, true)
        }).AsEnumerable();

        return enrolledEvents.Concat(taughtEvents);
    }
}
