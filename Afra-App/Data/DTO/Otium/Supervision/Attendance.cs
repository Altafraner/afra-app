using Afra_App.Data.Otium;

namespace Afra_App.Data.DTO.Otium.Supervision;

/// <summary>
/// Represents a DTO for attendance information for one student
/// </summary>
public record Attendance
{
    /// <summary>
    /// The unique ID of the attendance entry
    /// </summary>
    public Guid AttendanceId { get; set; }

    /// <summary>
    /// The unique ID of the termin the attendance is for
    /// </summary>
    public Guid? TerminId { get; set; }

    /// <summary>
    /// The student who is or is not attending
    /// </summary>
    public PersonInfoMinimal Student { get; set; }

    /// <summary>
    /// The status of the attendance
    /// </summary>
    public AnwesenheitsStatus Status { get; set; }
}