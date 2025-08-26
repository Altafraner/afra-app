using Afra_App.Backbone.Calendar.Domain.Models;
using Afra_App.Otium.Services;
using Afra_App.User.Domain.Models;
using Afra_App.User.Services;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;
using Microsoft.EntityFrameworkCore;

namespace Afra_App.Backbone.Calendar.Services;

///
public class CalendarService
{
    private readonly AfraAppContext _dbContext;
    private readonly UserService _userService;
    private readonly BlockHelper _blockHelper;

    ///
    public CalendarService(AfraAppContext dbContext, UserService userService, BlockHelper blockHelper)
    {
        _dbContext = dbContext;
        _userService = userService;
        _blockHelper = blockHelper;
    }

    ///
    public async Task<Guid> addCalendarSubscriptonAsync(Person user)
    {
        var sub = new CalendarSubscription { BetroffenePerson = user };
        _dbContext.CalendarSubscriptions.Add(sub);
        await _dbContext.SaveChangesAsync();
        return sub.Id;
    }

    ///
    public async Task<string?> getCalendarAsync(Guid subscriptionId)
    {
        var sub = await _dbContext.CalendarSubscriptions
            .Where(s => s.Id == subscriptionId)
            .Include(s => s.BetroffenePerson).FirstOrDefaultAsync();
        if (sub is null)
        {
            return null;
        }

        var enrollments = _dbContext.OtiaEinschreibungen
            .Where(e => e.BetroffenePerson.Id == sub.BetroffenePerson.Id)
            .Include(e => e.Termin).ThenInclude(t => t.Otium)
            .Include(e => e.Termin).ThenInclude(t => t.Block).ThenInclude(b => b.Schultag);
        var enrolledEvents = enrollments.Select(e => new CalendarEvent
        {
            Summary = e.Termin.Otium.Bezeichnung,
            Description = e.Termin.Otium.Beschreibung,
            Location = e.Termin.Ort,
            Start = new CalDateTime(new DateTime(e.Termin.Block.Schultag.Datum, e.Interval.Start), true),
            End = new CalDateTime(new DateTime(e.Termin.Block.Schultag.Datum, e.Interval.End), true),
        });

        var taught = _dbContext.OtiaTermine
            .Where(e => e.Tutor != null && e.Tutor.Id == sub.BetroffenePerson.Id)
            .Include(t => t.Otium)
            .Include(t => t.Block).ThenInclude(b => b.Schultag);
        var taughtEvents = taught.Select(e => new CalendarEvent
        {
            Summary = e.Otium.Bezeichnung,
            Description = e.Otium.Beschreibung,
            Location = e.Ort,
            Start = new CalDateTime(new DateTime(e.Block.Schultag.Datum, _blockHelper.Get(e.Block.SchemaId)!.Interval.Start), true),
            End = new CalDateTime(new DateTime(e.Block.Schultag.Datum, _blockHelper.Get(e.Block.SchemaId)!.Interval.End), true),
        });

        var calendar = new Ical.Net.Calendar();

        calendar.Events.AddRange(enrolledEvents);
        calendar.Events.AddRange(taughtEvents);
        calendar.AddTimeZone(new VTimeZone("Europe/Berlin"));

        var serializer = new CalendarSerializer();
        return serializer.SerializeToString(calendar)!;
    }
}
