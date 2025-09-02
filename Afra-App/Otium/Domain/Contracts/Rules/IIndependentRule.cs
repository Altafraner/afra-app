using Afra_App.Otium.Domain.Models;
using Afra_App.User.Domain.Models;

namespace Afra_App.Otium.Domain.Contracts.Rules;

/// <summary>
///     A rule that can be checked independently of other rules.
/// </summary>
public interface IIndependentRule
{
    /// <summary>
    ///     Checks if the rule is valid for the given person and enrollment.
    /// </summary>
    /// <returns>True, if the enrollment is valid in retrospective</returns>
    ValueTask<RuleStatus> IsValidAsync(Person person, OtiumEinschreibung enrollment);

    /// <summary>
    ///     Checks if the person may enroll to the given termin.
    /// </summary>
    /// <param name="person">The person wanting to enroll</param>
    /// <param name="termin">The termin the person wants to enroll to</param>
    /// <returns></returns>
    ValueTask<RuleStatus> MayEnrollAsync(Person person, OtiumTermin termin);

    /// <summary>
    ///     Checks if the person may unenroll from the given termin.
    /// </summary>
    /// <param name="person">The person wanting to unenroll</param>
    /// <param name="termin">The termin the person wants to unenroll from</param>
    /// <returns></returns>
    ValueTask<RuleStatus> MayUnenrollAsync(Person person, OtiumTermin termin);
}
