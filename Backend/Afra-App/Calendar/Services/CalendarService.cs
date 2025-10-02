using System.Security.Cryptography;
using Altafraner.AfraApp.Calendar.Domain.Models;
using Altafraner.AfraApp.Otium.Services;
using Altafraner.AfraApp.User.Domain.Models;
using Altafraner.AfraApp.User.Services;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Ical.Net.Serialization;
using Microsoft.EntityFrameworkCore;

namespace Altafraner.AfraApp.Calendar.Services;

/// <summary>
///     A Service for Calendar Subscriptions to Events
/// </summary>
public class CalendarService
{
    private readonly AfraAppContext _dbContext;
    private readonly UserService _userService;
    private readonly BlockHelper _blockHelper;

    /// <summary>
    ///     Constructor for the CalendarService. Usually called by the DI container.
    /// </summary>
    public CalendarService(AfraAppContext dbContext, UserService userService, BlockHelper blockHelper)
    {
        _dbContext = dbContext;
        _userService = userService;
        _blockHelper = blockHelper;
    }

    /// <summary>
    ///     Creates a new Calendar Subscription and returns the key
    /// </summary>
    /// <param name="user">The person to generate the subscription for</param>
    public async Task<Guid> AddCalendarSubscriptionAsync(Person user)
    {
        byte[] randBytes = RandomNumberGenerator.GetBytes(16);
        var key = new Guid(randBytes);
        var sub = new CalendarSubscription { BetroffenePerson = user, Id = key };
        _dbContext.CalendarSubscriptions.Add(sub);
        await _dbContext.SaveChangesAsync();
        return sub.Id;
    }

    /// <summary>
    ///     Deletes all Calendar Subscriptions for the user
    /// </summary>
    /// <param name="user">The person to delete the subscriptions for</param>
    public async Task DeleteAllCalendarSubscriptionAsync(Person user)
    {
        _dbContext.CalendarSubscriptions.Where(s => s.BetroffenePerson.Id == user.Id).ExecuteDelete();
        await _dbContext.SaveChangesAsync();
    }

    /// <summary>
    ///     Gets the number of active calendar subscriptions for a user
    /// </summary>
    /// <param name="user">The person to get the count for</param>
    public async Task<int> GetNumCalendarSubscriptionAsync(Person user)
    {
        return await _dbContext.CalendarSubscriptions.Where(s => s.BetroffenePerson.Id == user.Id).CountAsync();
    }

    /// <summary>
    ///     Gets the current ical for a calendar subcription from the subscription key
    /// </summary>
    /// <param name="subscriptionId">The Id of the Calendar Subscription</param>
    public async Task<string?> GetCalendarAsync(Guid subscriptionId)
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
            Uid = e.Id.ToString(),
            Summary = e.Termin.Bezeichnung,
            Description = e.Termin.Beschreibung,
            Location = e.Termin.Ort,
            Start = new CalDateTime(new DateTime(e.Termin.Block.Schultag.Datum, e.Interval.Start), true),
            End = new CalDateTime(new DateTime(e.Termin.Block.Schultag.Datum, e.Interval.End), true),
            LastModified = new CalDateTime(new DateTime[] { e.LastModified, e.Termin.LastModified, e.Termin.Otium.LastModified }.Max(), true),
            Created = new CalDateTime(e.CreatedAt, true),
        });

        var taught = _dbContext.OtiaTermine
            .Where(e => e.Tutor != null && e.Tutor.Id == sub.BetroffenePerson.Id)
            .Include(t => t.Otium)
            .Include(t => t.Block).ThenInclude(b => b.Schultag);
        var taughtEvents = taught.Select(e => new CalendarEvent
        {
            Uid = e.Id.ToString(),
            Summary = e.Bezeichnung,
            Description = e.Beschreibung,
            Location = e.Ort,
            Start = new CalDateTime(new DateTime(e.Block.Schultag.Datum, _blockHelper.Get(e.Block.SchemaId)!.Interval.Start), true),
            End = new CalDateTime(new DateTime(e.Block.Schultag.Datum, _blockHelper.Get(e.Block.SchemaId)!.Interval.End), true),
            LastModified = new CalDateTime(new DateTime[] { e.LastModified, e.Otium.LastModified }.Max(), true),
            Created = new CalDateTime(e.CreatedAt, true),
        });

        var calendar = new Ical.Net.Calendar();

        calendar.Events.AddRange(enrolledEvents);
        calendar.Events.AddRange(taughtEvents);
        calendar.AddTimeZone(new VTimeZone("Europe/Berlin"));

        var serializer = new CalendarSerializer();
        return serializer.SerializeToString(calendar)!;
    }
}
