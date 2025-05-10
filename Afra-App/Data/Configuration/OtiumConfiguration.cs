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
    ///     A static method to validate the configuration
    /// </summary>
    /// <returns>True, iff the configuration looks valid</returns>
    public static bool Validate(OtiumConfiguration config)
    {
        if (config.Blocks.Length == 0) return false;
        if (config.Blocks.Any(sb => sb.Interval.Duration <= TimeSpan.Zero))
            return false;

        return true;
    }
}
