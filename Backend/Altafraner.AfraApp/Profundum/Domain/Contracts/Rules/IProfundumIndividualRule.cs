using Altafraner.AfraApp.Profundum.Domain.DTO;
using Altafraner.AfraApp.Profundum.Domain.Models;
using Altafraner.AfraApp.User.Domain.Models;
using Google.OrTools.Sat;

namespace Altafraner.AfraApp.Profundum.Domain.Contracts.Rules;

/// <summary>
///     A profundum rule that is constrained only by the enrollments of a single student
/// </summary>
public interface IProfundumIndividualRule
{
    /// <summary>
    ///     Checks the students wishes before they can be submitted.
    /// </summary>
    RuleStatus CheckForSubmission(Person student,
    IEnumerable<ProfundumSlot> slots,
    IEnumerable<ProfundumEinschreibung> enrollments,
    IEnumerable<ProfundumBelegWunsch> wuensche);

    /// <summary>
    ///     Whether this rule could ever accept <paramref name="definition" /> into a submission for
    ///     <paramref name="student" />, independent of whatever else ends up in the same wish list - used to keep the
    ///     enrollment catalog from suggesting Profunda that are certain to be rejected anyway (e.g. an unmet
    ///     Dependency, or one already enrolled in). Defaults to <see cref="RuleStatus.Valid" />: only override this
    ///     when the rule's eligibility for a single Definition can be decided without knowing the rest of the wish
    ///     list. A rule whose <see cref="CheckForSubmission" /> logic is inherently cross-wish-dependent (e.g. "at
    ///     least one Profil pick across the whole submission") must NOT reuse that logic here, since probing it with
    ///     a single candidate would spuriously reject Definitionen that are perfectly pickable alongside others.
    /// </summary>
    RuleStatus CheckDefinitionEligibility(Person student,
        ProfundumDefinition definition,
        IEnumerable<ProfundumSlot> slots,
        IEnumerable<ProfundumEinschreibung> enrollments)
        => RuleStatus.Valid;

    /// <summary>
    ///     Add constraints to matching solver
    /// </summary>
    void AddConstraints(Person student,
        IEnumerable<ProfundumSlot> slots,
        IEnumerable<ProfundumBelegWunsch> wuensche,
        Dictionary<(ProfundumSlot s, ProfundumInstanz i), BoolVar> belegVars,
        Dictionary<ProfundumSlot, BoolVar> personNotEnrolledVars,
        CpModel model,
        LinearExprBuilder objective);

    /// <summary>
    ///  Gets warnings for a student
    /// </summary>
    /// <param name="student">The student to get warnings for</param>
    /// <param name="slots">The slots the student can enroll for</param>
    /// <param name="enrollments">The students enrollments</param>
    /// <returns></returns>
    IEnumerable<MatchingWarning> GetWarnings(Person student,
        IEnumerable<ProfundumSlot> slots,
        IEnumerable<ProfundumEinschreibung> enrollments);
}
