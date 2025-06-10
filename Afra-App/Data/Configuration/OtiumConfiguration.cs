using Afra_App.Data.Schuljahr;

namespace Afra_App.Data.Configuration;

/// <summary>
///     A configuration object containing the blocks in a
/// </summary>
public class OtiumConfiguration
{
    /// <summary>
    ///     A 2d array containing the subblocks of each block
    /// </summary>
    public required BlockMetadata[] Blocks { get; set; }

    /// <summary>
    ///     Information about the enrollment reminder
    /// </summary>
    public required EnrollmentReminderInfo EnrollmentReminder { get; set; }

    /// <summary>
    ///     A static method to validate the configuration
    /// </summary>
    /// <returns>True, iff the configuration looks valid</returns>
    public static bool Validate(OtiumConfiguration config)
    {
        if (config.Blocks is []) return false;
        if (config.Blocks.Any(sb => sb.Interval.Duration <= TimeSpan.Zero))
            return false;
        if (config is { EnrollmentReminder: null })
            return false;

        return true;
    }

    /// <summary>
    /// Information about the enrollment reminder, which is sent out to users
    /// </summary>
    /// <param name="Enabled">Whether to send enrollment reminders</param>
    /// <param name="Time">The time to send the reminders at</param>
    public record EnrollmentReminderInfo(
        bool Enabled,
        TimeOnly Time
    );
}
