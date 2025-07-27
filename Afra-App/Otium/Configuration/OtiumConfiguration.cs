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
    public required NotificationInfo EnrollmentReminder { get; init; }

    /// <summary>
    ///     Information about the student misbehaviour notification
    /// </summary>
    public required NotificationInfo StudentMisbehaviourNotification { get; init; }

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
    ///     Information about a notification
    /// </summary>
    /// <param name="Enabled">Whether to send notifications</param>
    /// <param name="Time">The time to send the notifications at</param>
    public record NotificationInfo(
        bool Enabled,
        TimeOnly Time
    );

    /// <summary>
    ///     Information about the automatic report of missing students
    /// </summary>
    /// <param name="Enabled">Whether to send automatic reports</param>
    /// <param name="Recipients">The mail adresses of the notifications recipients</param>
    public record MissingStudentsReportInfo(
        bool Enabled,
        string[] Recipients
    );
}
