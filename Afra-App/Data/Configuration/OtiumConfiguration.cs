using Afra_App.Data.Schuljahr;

namespace Afra_App.Data.Configuration;

/// <summary>
///     A configuration object containing the blocks in a
/// </summary>
public class OtiumConfiguration
{
    public SubBlock[][] Blocks { get; set; }

    public static bool Validate(OtiumConfiguration config)
    {
        if (config.Blocks.Length == 0) return false;
        if (config.Blocks.Any(list => list.Length == 0)) return false;
        if (config.Blocks.SelectMany(list => list).Any(sb => sb.Interval.Duration == TimeSpan.Zero))
            return false;

        return true;
    }
}