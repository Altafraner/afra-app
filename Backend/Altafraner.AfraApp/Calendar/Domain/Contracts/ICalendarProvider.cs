using Altafraner.AfraApp.User.Domain.Models;
using Ical.Net.CalendarComponents;
namespace Altafraner.AfraApp.Calendar;

/// <summary>Implemented by Classes that provide personal calendar events per given user</summary>
public interface ICalendarProvider
{
    /// <summary>Get all current or future calendar events for the user <paramref name="p"/></summary>
    public IEnumerable<CalendarEvent> GetEventsForPerson(Person p);
}
