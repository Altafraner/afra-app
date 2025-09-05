using Afra_App.Otium.Domain.Contracts.Rules;
using Afra_App.Otium.Domain.Models;
using Person = Afra_App.User.Domain.Models.Person;

namespace Afra_App.Otium.Services.Rules;

/// <summary>
///     Checks if the enrollment is done within the allowed timeframe.
/// </summary>
public class EnrollmentTimeframeRule : IIndependentRule
{
    private readonly BlockHelper _blockHelper;

    ///
    public EnrollmentTimeframeRule(BlockHelper blockHelper)
    {
        _blockHelper = blockHelper;
    }

    /// <inheritdoc />
    public ValueTask<RuleStatus> MayEnrollAsync(Person person, OtiumTermin termin)
    {
        var block = _blockHelper.Get(termin.Block.SchemaId)!;
        var now = DateTime.Now;
        var today = DateOnly.FromDateTime(now);
        var time = TimeOnly.FromDateTime(now);
        if (today < termin.Block.SchultagKey || (today == termin.Block.SchultagKey && time < block.EinschreibenBis))
            return new ValueTask<RuleStatus>(RuleStatus.Valid);

        return new ValueTask<RuleStatus>(today > termin.Block.SchultagKey
            ? RuleStatus.Invalid("Der Termin liegt in der Vergangenheit.")
            : RuleStatus.Invalid("Die Einschreibefrist ist abgelaufen."));
    }

    /// <inheritdoc />
    public ValueTask<RuleStatus> MayUnenrollAsync(Person person, OtiumTermin termin)
    {
        return MayEnrollAsync(person, termin);
    }
}
