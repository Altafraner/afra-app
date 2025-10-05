using Altafraner.AfraApp.Otium.Domain.Models;
using Altafraner.AfraApp.User.Domain.Models;

namespace Altafraner.AfraApp.Otium.Domain.Contracts.Rules;

/// <summary>
///     A rule that can be checked independently of other rules.
/// </summary>
public interface IIndependentRule
{
    /// <summary>
    ///     Checks if the rule is valid for the given person and enrollment.
    /// </summary>
    /// <returns>True, if the enrollment is valid in retrospective</returns>
    ValueTask<RuleStatus> IsValidAsync(Person person, OtiumEinschreibung enrollment)
    {
        return new ValueTask<RuleStatus>(RuleStatus.Valid);
    }

    /// <summary>
    ///     Checks if the person may enroll to the given termin.
    /// </summary>
    /// <param name="person">The person wanting to enroll</param>
    /// <param name="termin">The termin the person wants to enroll to</param>
    /// <returns></returns>
    ValueTask<RuleStatus> MayEnrollAsync(Person person, OtiumTermin termin)
    {
        return new ValueTask<RuleStatus>(RuleStatus.Valid);
    }

    /// <summary>
    ///     Checks if the person may unenroll from the given termin.
    /// </summary>
    /// <param name="person">The person wanting to unenroll</param>
    /// <param name="einschreibung">The enrollment the person wants to unenroll from</param>
    /// <returns></returns>
    ValueTask<RuleStatus> MayUnenrollAsync(Person person, OtiumEinschreibung einschreibung)
    {
        return new ValueTask<RuleStatus>(RuleStatus.Valid);
    }
}
