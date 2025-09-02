using Afra_App.Otium.Domain.Models;
using Afra_App.Otium.Domain.Models.Schuljahr;
using Afra_App.User.Domain.Models;

namespace Afra_App.Otium.Domain.Contracts.Rules;

/// <summary>
///     A rule that may depend on other enrollments in the same block.
/// </summary>
public interface IBlockRule
{
    /// <summary>
    ///     Checks if the rule is valid for the given person and enrollments during a block.
    /// </summary>
    /// <returns>True, if the enrollments are valid in retrospective</returns>
    ValueTask<RuleStatus> IsValidAsync(Person person, Block block, IEnumerable<OtiumEinschreibung> einschreibungen);

    /// <summary>
    ///     Checks if the person may enroll to the given termin.
    /// </summary>
    /// <param name="person">The person wanting to enroll</param>
    /// <param name="einschreibungen">The persons other enrollments in the same block</param>
    /// <param name="termin">The termin the person wants to enroll to</param>
    /// <returns></returns>
    ValueTask<RuleStatus> MayEnrollAsync(Person person, IEnumerable<OtiumEinschreibung> einschreibungen,
        OtiumTermin termin);

    /// <summary>
    ///     Checks if the person may unenroll from the given termin.
    /// </summary>
    /// <param name="person">The person wanting to unenroll</param>
    /// <param name="einschreibungen">The persons other enrollments in the same block</param>
    /// <param name="termin">The termin the person wants to unenroll from</param>
    /// <returns></returns>
    ValueTask<RuleStatus> MayUnenrollAsync(Person person, IEnumerable<OtiumEinschreibung> einschreibungen,
        OtiumTermin termin);
}
