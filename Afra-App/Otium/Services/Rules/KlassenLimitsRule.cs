using Afra_App.Otium.Domain.Contracts.Rules;
using Afra_App.Otium.Domain.Models;
using Afra_App.User.Services;
using Person = Afra_App.User.Domain.Models.Person;

namespace Afra_App.Otium.Services.Rules;

/// <summary>
///     Checks that the grade limits are respected
/// </summary>
public class KlassenLimitsRule : IIndependentRule
{
    private readonly UserService _userService;

    ///
    public KlassenLimitsRule(UserService userService)
    {
        _userService = userService;
    }

    /// <inheritdoc />
    public ValueTask<RuleStatus> MayEnrollAsync(Person person, OtiumTermin termin)
    {
        var klasse = _userService.GetKlassenstufe(person);

        if (termin.Otium.MinKlasse is not null && termin.Otium.MinKlasse > klasse)
        {
            return new ValueTask<RuleStatus>(
                    RuleStatus.Invalid($"Dieses Otium ist nur für Schüler:innen ab Klasse {termin.Otium.MinKlasse} vorgesehen"));
        }
        if (termin.Otium.MaxKlasse is not null && termin.Otium.MaxKlasse < klasse)
        {
            return new ValueTask<RuleStatus>(
                    RuleStatus.Invalid($"Dieses Otium ist nur für Schüler:innen bis Klasse {termin.Otium.MinKlasse} vorgesehen"));
        }
        return new ValueTask<RuleStatus>(RuleStatus.Valid);
    }
}
