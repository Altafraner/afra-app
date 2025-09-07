using Afra_App.Otium.Domain.Contracts.Rules;
using Afra_App.Otium.Domain.Models;
using Person = Afra_App.User.Domain.Models.Person;

namespace Afra_App.Otium.Services.Rules;

/// <summary>
///     Checks that the termin is not cancelled.
/// </summary>
public class NotCancelledRule : IIndependentRule
{
    /// <inheritdoc />
    public ValueTask<RuleStatus> IsValidAsync(Person person, OtiumEinschreibung enrollment)
    {
        return new ValueTask<RuleStatus>(Rule(enrollment.Termin));
    }

    /// <inheritdoc />
    public ValueTask<RuleStatus> MayEnrollAsync(Person person, OtiumTermin termin)
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
