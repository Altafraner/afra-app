using Altafraner.AfraApp.Profundum.Domain.Contracts.Rules;
using Altafraner.AfraApp.Profundum.Domain.DTO;
using Altafraner.AfraApp.Profundum.Domain.Models;
using Altafraner.AfraApp.User.Domain.Models;
using Google.OrTools.Sat;
using Microsoft.EntityFrameworkCore;

namespace Altafraner.AfraApp.Profundum.Services.Rules;

/// <summary>
///     For a Profundum with <see cref="ProfundumDefinition.PflichtFuerBerechtigte" /> set, every student within its
///     <see cref="ProfundumDefinition.MinKlasse" />/<see cref="ProfundumDefinition.MaxKlasse" /> range is expected
///     to enroll whenever it's offered. Soft only (a matching-time objective penalty plus a staff-facing warning) -
///     never a hard submission block or solver constraint, same reasoning as the rest of this rules engine: a hard
///     constraint here could make an otherwise-fine matching run infeasible over a single student/slot.
/// </summary>
public class PflichtProfundumRule : IProfundumIndividualRule
{
    /// <summary>
    ///     Penalty for an eligible student ending up without an enrollment in a Pflicht-Profundum that was offered
    ///     to them. Matches the magnitude of <see cref="ProfilRule" />'s "must have a Profil" penalty - both are the
    ///     same kind of "this student should really have this" soft preference.
    /// </summary>
    private const int PflichtProfundumPenalty = -10000;

    private readonly AfraAppContext _dbContext;

    ///
    public PflichtProfundumRule(AfraAppContext dbContext)
    {
        _dbContext = dbContext;
    }

    private List<ProfundumInstanz>? _pflichtInstanzen;

    /// <summary>
    ///     Every Instanz of a Pflicht-Profundum, regardless of period - small reference data, so it's fetched once
    ///     and reused for the rest of this rule instance's lifetime, same "computed once, reused" pattern as
    ///     <see cref="ProfilRule.AlleProfilKategorien" />. <see cref="AddConstraints" /> doesn't need this itself -
    ///     its <c>belegVars</c> keys already carry the Instanz/Profundum data - only <see cref="GetWarnings" /> does,
    ///     since it isn't handed the offered-Instanzen catalog.
    /// </summary>
    private List<ProfundumInstanz> PflichtInstanzen =>
        _pflichtInstanzen ??= _dbContext.ProfundaInstanzen
            .Include(i => i.Slots)
            .Include(i => i.Profundum)
            .Where(i => i.Profundum.PflichtFuerBerechtigte)
            .ToList();

    private static bool IstBerechtigt(int klasse, ProfundumDefinition profundum)
    {
        if (profundum.MinKlasse is not null && klasse < profundum.MinKlasse) return false;
        if (profundum.MaxKlasse is not null && klasse > profundum.MaxKlasse) return false;
        return true;
    }

    /// <inheritdoc/>
    public RuleStatus CheckForSubmission(Person student,
        IEnumerable<ProfundumSlot> slots,
        IEnumerable<ProfundumEinschreibung> enrollments,
        IEnumerable<ProfundumBelegWunsch> wuensche)
        => RuleStatus.Valid;

    /// <inheritdoc/>
    public void AddConstraints(Person student,
        int klasse,
        IEnumerable<ProfundumSlot> slots,
        IEnumerable<ProfundumBelegWunsch> wuensche,
        IEnumerable<ProfundumEinschreibung> enrollments,
        Dictionary<(ProfundumSlot s, ProfundumInstanz i), BoolVar> belegVars,
        Dictionary<ProfundumSlot, BoolVar> personNotEnrolledVars,
        CpModel model,
        LinearExprBuilder objective)
    {
        var pflichtGroups = belegVars
            .Where(x => x.Key.i.Profundum.PflichtFuerBerechtigte && IstBerechtigt(klasse, x.Key.i.Profundum))
            .GroupBy(x => (x.Key.s, x.Key.i.Profundum.Id));

        foreach (var group in pflichtGroups)
        {
            var vars = group.Select(x => x.Value).ToList();
            var hasEnrolled = model.NewBoolVar($"pflichtProfundum-{student.Id}-{group.Key.s.Id}-{group.Key.Id}");
            model.AddMaxEquality(hasEnrolled, vars);
            objective.AddTerm(hasEnrolled.Not(), PflichtProfundumPenalty);
        }
    }

    /// <inheritdoc/>
    public IEnumerable<MatchingWarning> GetWarnings(Person student, int klasse, IEnumerable<ProfundumSlot> slots, IEnumerable<ProfundumEinschreibung> enrollments)
    {
        var slotsArray = slots as ProfundumSlot[] ?? slots.ToArray();
        var enrolledInstanzIds = enrollments
            .Where(e => e.ProfundumInstanz is not null)
            .Select(e => e.ProfundumInstanz!.Id)
            .ToHashSet();

        var warnings = new List<MatchingWarning>();
        foreach (var profundumGroup in PflichtInstanzen
                     .Where(i => i.Slots.Any(slotsArray.Contains) && IstBerechtigt(klasse, i.Profundum))
                     .GroupBy(i => i.Profundum))
        {
            if (!profundumGroup.Any(i => enrolledInstanzIds.Contains(i.Id)))
            {
                warnings.Add(new MatchingWarning(
                    $"Pflichtprofundum {profundumGroup.Key.Bezeichnung} nicht belegt, obwohl angeboten und berechtigt."));
            }
        }

        return warnings;
    }
}
