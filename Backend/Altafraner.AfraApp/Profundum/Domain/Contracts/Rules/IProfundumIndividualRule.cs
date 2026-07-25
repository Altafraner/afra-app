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
    ///     Add constraints to matching solver. <paramref name="klasse" /> is the student's grade level, precomputed
    ///     once by the caller (a single batched query across all students) rather than looked up here - this method
    ///     runs once per (student, rule) in a tight loop over every student, so a per-call DB lookup here would
    ///     multiply into hundreds of round-trips per matching run.
    /// </summary>
    void AddConstraints(Person student,
        int klasse,
        IEnumerable<ProfundumSlot> slots,
        IEnumerable<ProfundumBelegWunsch> wuensche,
        Dictionary<(ProfundumSlot s, ProfundumInstanz i), BoolVar> belegVars,
        Dictionary<ProfundumSlot, BoolVar> personNotEnrolledVars,
        CpModel model,
        LinearExprBuilder objective);

    /// <summary>
    ///     Gets warnings for a student. <paramref name="klasseAsOf" /> resolves the student's grade level as of an
    ///     arbitrary point in time, backed by a group-history log the caller already batch-loaded for every student
    ///     up front - rules that need a grade level as of a specific historical Einwahlzeitraum/enrollment must go
    ///     through this instead of querying per call, since this method runs once per student across a potentially
    ///     large enrollment history.
    /// </summary>
    IEnumerable<MatchingWarning> GetWarnings(Person student,
        Func<DateTime, int> klasseAsOf,
        IEnumerable<ProfundumSlot> slots,
        IEnumerable<ProfundumEinschreibung> enrollments);
}
