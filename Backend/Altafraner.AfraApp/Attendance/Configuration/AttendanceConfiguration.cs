using Altafraner.AfraApp.Attendance.AbsenceProviders.Cevex;

namespace Altafraner.AfraApp.Attendance.Configuration;

/// <summary>
///     Contains configuration for the attendance subsystem
/// </summary>
public class AttendanceConfiguration
{
    /// <inheritdoc cref="CevexConfig" />
    public CevexConfig? Cevex { get; set; }
}
