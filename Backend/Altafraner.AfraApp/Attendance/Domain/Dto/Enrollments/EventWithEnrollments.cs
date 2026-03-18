using Altafraner.AfraApp.User.Domain.Models;

namespace Altafraner.AfraApp.Attendance.Domain.Dto.Enrollments;

/// <summary>
///     An event for which there is Attendance functionality
/// </summary>
public class EventWithEnrollments : Event
{
    /// <summary>
    ///     All persons enrolled to this event
    /// </summary>
    public required IEnumerable<Person> Enrollments;
}
