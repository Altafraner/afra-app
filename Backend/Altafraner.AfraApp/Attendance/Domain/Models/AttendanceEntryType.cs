namespace Altafraner.AfraApp.Attendance.Domain.Models;

/// <summary>
///     The type of attendance
/// </summary>
public enum AttendanceEntryType
{
    /// <summary>
    ///     The attendance was created by a human
    /// </summary>
    Manual,

    /// <summary>
    ///     The attendance was inferred by the programm
    /// </summary>
    Automatic
}
