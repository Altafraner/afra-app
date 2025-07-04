using Afra_App.Otium.Domain.Models.Schuljahr;

namespace Afra_App.Otium.Configuration;

/// <summary>
///     A configuration object containing the blocks in a
/// </summary>
public class OtiumConfiguration
{
    /// <summary>
    ///     A 2d array containing the subblocks of each block
    /// </summary>
    public required BlockMetadata[] Blocks { get; init; }

    /// <summary>
    ///     Information about the enrollment reminder
    /// </summary>
    public required EnrollmentReminderInfo EnrollmentReminder { get; init; }

    /// <summary>
    ///     Information about the automatic reporting of missing students
    /// </summary>
    public required MissingStudentsReportInfo MissingStudentsReport { get; init; }

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

    /// <summary>
    /// Information about the automatic report of missing students
    /// </summary>
    /// <param name="Enabled">Whether to send automatic reports</param>
    /// <param name="Recipients">The mail adresses of the notifications recipients</param>
    public record MissingStudentsReportInfo(
        bool Enabled,
        string[] Recipients
    );
}
