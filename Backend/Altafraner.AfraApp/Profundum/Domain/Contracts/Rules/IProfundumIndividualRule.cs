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
    ///     multiply into hundreds of round-trips per matching run. <paramref name="enrollments" /> is the student's
    ///     fixed (already-committed, from past periods) enrollment history - e.g. needed by
    ///     <see cref="Altafraner.AfraApp.Profundum.Services.Rules.ProfilRule" /> to steer the solver toward a
    ///     Profil-Kategorie the student hasn't covered yet rather than a redundant one.
    /// </summary>
    void AddConstraints(Person student,
        int klasse,
        IEnumerable<ProfundumSlot> slots,
        IEnumerable<ProfundumEinschreibung> enrollments,
        Dictionary<(ProfundumSlot s, ProfundumInstanz i), BoolVar> belegVars,
        CpModel model,
        LinearExprBuilder objective);

    /// <summary>
    ///     Gets warnings for a student. <paramref name="klasse" /> is the student's current grade level, precomputed
    ///     once by the caller (same value <see cref="AddConstraints" /> already gets). <paramref name="slots" /> is
    ///     restricted to the current Einwahlzeitraum - old, already-fixed enrollments are not re-validated - while
    ///     <paramref name="enrollments" /> is the student's full enrollment history, since some rules (e.g.
    ///     dependency/duplicate/Profil-coverage checks) need that as evidence even though they only warn about
    ///     something in the current period.
    /// </summary>
    IEnumerable<MatchingWarning> GetWarnings(Person student,
        int klasse,
        IEnumerable<ProfundumSlot> slots,
        IEnumerable<ProfundumEinschreibung> enrollments);
}
