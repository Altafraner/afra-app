using Altafraner.AfraApp.Domain.TimeInterval;

namespace Altafraner.AfraApp.Attendance.Domain.Dto.Enrollments;

/// <summary>
///     Metadata required for configuring an attendance slot
/// </summary>
public class AttendanceSlotMetadata
{
    /// <summary>
    ///     If true, the notes function should be enabled
    /// </summary>
    public required bool EnableNotes { get; set; }

    /// <summary>
    ///     If true, the provider supports moving students to and from events
    /// </summary>
    public required bool EnableMove { get; set; }

    /// <summary>
    ///     Must be true, if the block is done
    /// </summary>
    public required bool IsInPast { get; set; }

    /// <summary>
    ///     The date this slot starts on
    /// </summary>
    public required DateOnly StartDate { get; set; }

    /// <summary>
    ///     The lesson this slot starts in
    /// </summary>
    public required int StartLesson { get; set; }

    /// <summary>
    ///     The time intervall in which moving a student beginning now has a valid interpretation and should be enabled.
    ///     Requires <see cref="EnableMove" /> to be <c>true</c>.
    /// </summary>
    public DateTimeInterval? MoveNowIntervall { get; set; } = null;

    /// <summary>
    ///     A list of E-Mail-Addresses that should be notified about missing students
    /// </summary>
    public required string[] MissingStudentsNotificationRecipients;

    /// <summary>
    ///     The time at which to send the notification about missing students. If null, no notification will be sent.
    /// </summary>
    public required DateTime? MissingStudentsNotificationTime;
}
