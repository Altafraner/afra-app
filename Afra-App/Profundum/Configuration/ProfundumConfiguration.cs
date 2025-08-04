namespace Afra_App.Profundum.Configuration;

using Profundum.Domain.Models;

///
public class ProfundumConfiguration
{
    ///
    public required Dictionary<int, ProfundumQuartal[]> ProfilPflichtigkeit { get; set; }

    ///
    public static bool Validate(ProfundumConfiguration config)
    {
        return true;
    }
}
