using Afra_App.Otium.Domain.Contracts.Rules;
using Afra_App.Otium.Domain.Models;
using Afra_App.Schuljahr.Domain.Models;
using Afra_App.User.Domain.Models;

namespace Afra_App.Otium.Services.Rules;

/// <summary>
///     Checks that the person is not enrolled in another termin at the same time.
/// </summary>
public class ParallelEnrollmentRule : IBlockRule
{
    /// <inheritdoc />
    public ValueTask<RuleStatus> IsValidAsync(Person person, Block block, IEnumerable<OtiumEinschreibung> enrollments)
    {
        return new ValueTask<RuleStatus>(RuleStatus.Valid);
    }

    /// <inheritdoc />
    public ValueTask<RuleStatus> MayEnrollAsync(Person person, IEnumerable<OtiumEinschreibung> einschreibungen,
        OtiumTermin termin)
    {
        return new ValueTask<RuleStatus>(einschreibungen.Any()
            ? RuleStatus.Invalid("Du bist bereits zur selben Zeit eingeschrieben")
            : RuleStatus.Valid);
    }

    /// <inheritdoc />
    public ValueTask<RuleStatus> MayUnenrollAsync(Person person, IEnumerable<OtiumEinschreibung> einschreibungen,
        OtiumTermin termin)
    {
        return new ValueTask<RuleStatus>(RuleStatus.Valid);
    }
}
