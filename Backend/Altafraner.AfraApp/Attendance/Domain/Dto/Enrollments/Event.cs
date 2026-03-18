namespace Altafraner.AfraApp.Attendance.Domain.Dto.Enrollments;

/// <summary>
///     An event for which there is Attendance functionality
/// </summary>
public class Event
{
    /// <summary>
    ///     The unique identifier of the event
    /// </summary>
    public required Guid EventId { get; set; }

    /// <summary>
    ///     The event name
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    ///     The event location
    /// </summary>
    public required string Location { get; set; }
}
