using Altafraner.AfraApp.Otium.Domain.Models;
using Altafraner.AfraApp.Schuljahr.Domain.Models;
using Altafraner.AfraApp.User.Domain.Models;

namespace Altafraner.AfraApp.Otium.Domain.Contracts.Rules;

/// <summary>
///     A rule that may depend on other enrollments in the same block.
/// </summary>
public interface IBlockRule
{
    /// <summary>
    ///     Checks if the rule is valid for the given person and enrollments during a block.
    /// </summary>
    ValueTask<RuleStatus> IsValidAsync(Person person, Block block, IEnumerable<OtiumEinschreibung> einschreibungen)
    {
        return new ValueTask<RuleStatus>(RuleStatus.Valid);
    }

    /// <summary>
    ///     Checks if the person may enroll to the given termin.
    /// </summary>
    /// <param name="person">The person wanting to enroll</param>
    /// <param name="einschreibungen">The persons other enrollments in the same block</param>
    /// <param name="termin">The termin the person wants to enroll to</param>
    ValueTask<RuleStatus> MayEnrollAsync(Person person, IEnumerable<OtiumEinschreibung> einschreibungen,
        OtiumTermin termin)
    {
        return new ValueTask<RuleStatus>(RuleStatus.Valid);
    }

    /// <summary>
    ///     Checks if the person may unenroll from the given termin.
    /// </summary>
    /// <param name="person">The person wanting to unenroll</param>
    /// <param name="einschreibungen">The persons other enrollments in the same block</param>
    /// <param name="einschreibung">The enrollment the person wants to unenroll from</param>
    ValueTask<RuleStatus> MayUnenrollAsync(Person person, IEnumerable<OtiumEinschreibung> einschreibungen,
        OtiumEinschreibung einschreibung)
    {
        return new ValueTask<RuleStatus>(RuleStatus.Valid);
    }
}
