using System.Security.Cryptography;
using Altafraner.AfraApp.Calendar.Domain.Models;
using Altafraner.AfraApp.Otium.Services;
using Altafraner.AfraApp.User.Domain.Models;
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
    private readonly IEnumerable<ICalendarProvider> _calendarProviders;

    /// <summary>
    ///     Constructor for the CalendarService. Usually called by the DI container.
    /// </summary>
    public CalendarService(AfraAppContext dbContext, IEnumerable<ICalendarProvider> calendarProviders)
    {
        _dbContext = dbContext;
        _calendarProviders = calendarProviders;
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
        return await GetCalendarAsync(sub.BetroffenePerson);
    }
    ///
    public async Task<string> GetCalendarAsync(Person person)
    {
        var calendar = new Ical.Net.Calendar();

        calendar.Events.AddRange(_calendarProviders
                .Select(c => c.GetEventsForPerson(person))
                .SelectMany(x => x));

        calendar.AddTimeZone(new VTimeZone("Europe/Berlin"));

        var serializer = new CalendarSerializer();
        return serializer.SerializeToString(calendar)!;
    }
}
