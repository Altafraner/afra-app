using Altafraner.AfraApp.Attendance.Domain.HubClients;

namespace Altafraner.AfraApp.Profundum.Domain.DTO;

/// <summary>
///     A dto for transmitting student enrollment information for a specific instance and termin
/// </summary>
public class ProfundumTerminInstanceInfo
{
    /// <summary>
    ///     The slot the profundum is in
    /// </summary>
    public required DTOProfundumSlot Slot { get; init; }

    /// <summary>
    ///     The time this termin starts by
    /// </summary>
    public required DateTime Start { get; init; }

    /// <summary>
    ///     The profundums name
    /// </summary>
    public required string Label { get; init; }

    /// <summary>
    ///     The enrollments for the given profundum
    /// </summary>
    public required IEnumerable<IAttendanceHubClient.StudentStatus> Enrollments { get; init; }

    /// <summary>
    ///     Whether this termin is done or started
    /// </summary>
    public required bool IsDoneOrStarted { get; init; }

    /// <summary>
    ///     Whether the user may edit the students attendance
    /// </summary>
    public required bool IsAttendanceEditable { get; init; }
}
