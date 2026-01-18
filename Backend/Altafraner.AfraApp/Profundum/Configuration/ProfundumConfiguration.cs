using Altafraner.AfraApp.Profundum.Domain.Models;

namespace Altafraner.AfraApp.Profundum.Configuration;

///
public class ProfundumConfiguration
{
    /// <summary>
    ///  When disabled all entries are randomized before a matching to improve fairness in unlikely cases.
    ///  Enable for debugging only.
    /// </summary>
    public required bool DeterministicMatching { get; set; }

    /// <summary>
    ///     A dictionary containing a grade level as key and list of quartals as value that describes when which grade level
    ///     must enroll in a profilprofundum
    /// </summary>
    public required Dictionary<int, ProfundumQuartal[]> ProfilPflichtigkeit { get; set; }

    /// <summary>
    ///     A dictionary containing a grade as key and list of quartals as value that describes when which grade may enroll in
    ///     a profilprofundum even if it's not mandatory
    /// </summary>
    public required Dictionary<string, ProfundumQuartal[]> ProfilZulassung { get; set; }
}
