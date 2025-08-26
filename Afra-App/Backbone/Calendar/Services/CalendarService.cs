using Afra_App.Backbone.Calendar.Domain.Models;
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

    ///
    public CalendarService(AfraAppContext dbContext, UserService userService)
    {
        _dbContext = dbContext;
        _userService = userService;
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

        var calEvents = enrollments.Select(e => new CalendarEvent
        {
            Summary = e.Termin.Otium.Bezeichnung,
            Description = e.Termin.Otium.Beschreibung,
            Location = e.Termin.Ort,
            Start = new CalDateTime(new DateTime(e.Termin.Block.Schultag.Datum, e.Interval.Start), true),
            End = new CalDateTime(new DateTime(e.Termin.Block.Schultag.Datum, e.Interval.End), true),
        });

        var calendar = new Ical.Net.Calendar();

        calendar.Events.AddRange(calEvents);
        calendar.AddTimeZone(new VTimeZone("Europe/Berlin"));

        var serializer = new CalendarSerializer();
        return serializer.SerializeToString(calendar)!;
    }
}
