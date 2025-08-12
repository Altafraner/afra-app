namespace Afra_App.Profundum.Configuration;

using Profundum.Domain.Models;

///
public class ProfundumConfiguration
{
    /// <summary>
    /// When disabled all entries are randomized before a matching to improve fairness in unlikely cases.
    /// Enable for debugging only.
    /// </summary>
    public required bool deterministicMatching { get; set; }

    ///
    public required Dictionary<int, ProfundumQuartal[]> ProfundumBlockiert { get; set; }

    ///
    public required Dictionary<int, ProfundumQuartal[]> ProfilPflichtigkeit { get; set; }

    ///
    public required Dictionary<string, ProfundumQuartal[]> ProfilZulassung { get; set; }

    ///
    public static bool Validate(ProfundumConfiguration config)
    {
        return true;
    }
}
