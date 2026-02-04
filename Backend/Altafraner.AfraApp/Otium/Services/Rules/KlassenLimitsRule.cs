using Altafraner.AfraApp.Otium.Domain.Contracts.Rules;
using Altafraner.AfraApp.Otium.Domain.Models;
using Altafraner.AfraApp.User.Services;
using Models_Person = Altafraner.AfraApp.User.Domain.Models.Person;

namespace Altafraner.AfraApp.Otium.Services.Rules;

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
    public ValueTask<RuleStatus> MayEnrollAsync(Models_Person person, OtiumTermin termin)
    {
        var klasse = _userService.GetKlassenstufe(person);

        if (termin.Otium.MinKlasse is not null && termin.Otium.MinKlasse > klasse)
        {
            return new ValueTask<RuleStatus>(
                RuleStatus.Invalid(
                    $"Dieses Otium ist nur für Schüler:innen ab Klasse {termin.Otium.MinKlasse} vorgesehen"
                )
            );
        }
        if (termin.Otium.MaxKlasse is not null && termin.Otium.MaxKlasse < klasse)
        {
            return new ValueTask<RuleStatus>(
                RuleStatus.Invalid(
                    $"Dieses Otium ist nur für Schüler:innen bis Klasse {termin.Otium.MinKlasse} vorgesehen"
                )
            );
        }
        return new ValueTask<RuleStatus>(RuleStatus.Valid);
    }
}
