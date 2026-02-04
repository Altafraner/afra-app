using Altafraner.AfraApp.Otium.Domain.Contracts.Rules;
using Altafraner.AfraApp.Otium.Domain.Models;
using Models_Person = Altafraner.AfraApp.User.Domain.Models.Person;

namespace Altafraner.AfraApp.Otium.Services.Rules;

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
    public ValueTask<RuleStatus> MayEnrollAsync(Models_Person person, OtiumTermin termin)
    {
        var block = _blockHelper.Get(termin.Block.SchemaId)!;
        var now = DateTime.Now;
        var today = DateOnly.FromDateTime(now);
        var time = TimeOnly.FromDateTime(now);
        if (
            today < termin.Block.SchultagKey
            || (today == termin.Block.SchultagKey && time < block.EinschreibenBis)
        )
            return new ValueTask<RuleStatus>(RuleStatus.Valid);

        return new ValueTask<RuleStatus>(
            today > termin.Block.SchultagKey
                ? RuleStatus.Invalid("Der Termin liegt in der Vergangenheit.")
                : RuleStatus.Invalid("Die Einschreibefrist ist abgelaufen.")
        );
    }

    /// <inheritdoc />
    public ValueTask<RuleStatus> MayUnenrollAsync(
        Models_Person person,
        OtiumEinschreibung einschreibung
    )
    {
        return MayEnrollAsync(person, einschreibung.Termin);
    }
}
