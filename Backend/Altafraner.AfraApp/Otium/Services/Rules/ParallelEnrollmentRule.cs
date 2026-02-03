using Altafraner.AfraApp.Otium.Domain.Contracts.Rules;
using Altafraner.AfraApp.Otium.Domain.Models;
using Altafraner.AfraApp.User.Domain.Models;

namespace Altafraner.AfraApp.Otium.Services.Rules;

/// <summary>
///     Checks that the person is not enrolled in another termin at the same time.
/// </summary>
public class ParallelEnrollmentRule : IBlockRule
{
    /// <inheritdoc />
    public ValueTask<RuleStatus> MayEnrollAsync(
        Person person,
        IEnumerable<OtiumEinschreibung> einschreibungen,
        OtiumTermin termin
    )
    {
        return new ValueTask<RuleStatus>(
            einschreibungen.Any()
                ? RuleStatus.Invalid("Du bist bereits zur selben Zeit eingeschrieben")
                : RuleStatus.Valid
        );
    }
}
