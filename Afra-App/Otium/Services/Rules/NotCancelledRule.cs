using Altafraner.AfraApp.Otium.Domain.Contracts.Rules;
using Altafraner.AfraApp.Otium.Domain.Models;
using Models_Person = Altafraner.AfraApp.User.Domain.Models.Person;

namespace Altafraner.AfraApp.Otium.Services.Rules;

/// <summary>
///     Checks that the termin is not cancelled.
/// </summary>
public class NotCancelledRule : IIndependentRule
{
    /// <inheritdoc />
    public ValueTask<RuleStatus> IsValidAsync(Models_Person person, OtiumEinschreibung enrollment)
    {
        return new ValueTask<RuleStatus>(Rule(enrollment.Termin));
    }

    /// <inheritdoc />
    public ValueTask<RuleStatus> MayEnrollAsync(Models_Person person, OtiumTermin termin)
    {
        return new ValueTask<RuleStatus>(Rule(termin));
    }

    private static RuleStatus Rule(OtiumTermin termin)
    {
        return termin.IstAbgesagt
            ? RuleStatus.Invalid("Der Termin wurde abgesagt.")
            : RuleStatus.Valid;
    }
}
