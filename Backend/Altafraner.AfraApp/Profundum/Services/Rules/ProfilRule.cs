using Altafraner.AfraApp.Profundum.Configuration;
using Altafraner.AfraApp.Profundum.Domain.Contracts.Rules;
using Altafraner.AfraApp.Profundum.Domain.DTO;
using Altafraner.AfraApp.Profundum.Domain.Models;
using Altafraner.AfraApp.User.Domain.Models;
using Altafraner.AfraApp.User.Services;
using Google.OrTools.Sat;
using Microsoft.Extensions.Options;

namespace Altafraner.AfraApp.Profundum.Services.Rules;

/// <summary>
///     Implements the Profil-Profundum rules, entirely driven by the global
///     <see cref="ProfundumConfiguration.ProfilPflichtigkeit" /> config (grade -&gt; the Quartale that grade may/must
///     enroll in a Profilprofundum for) - there is no per-student override:
///     <list type="number">
///         <item>A Profilprofundum may only be wished for/enrolled in during a Halbjahr the student's current grade
///         is configured for.</item>
///         <item>In every Einwahlzeitraum where that's the case, at least one Profilprofundum must be wished for/
///         enrolled in.</item>
///         <item>At most one Profilprofundum may be enrolled in per Einwahlzeitraum (multiple may still be ranked).</item>
///         <item>By the end of Klasse 10, every Profil-Kategorie must have been covered by some enrollment.</item>
///     </list>
///     <see cref="CheckForSubmission" /> enforces these hard at submission time, but every one of them is a soft
///     (heavily penalized, never a hard <c>model.Add</c>/<c>AddAtMostOne</c>) preference in <see cref="AddConstraints" />
///     during matching - a hard constraint here could make the entire matching run infeasible over a single
///     student/Einwahlzeitraum, which is worse than one student ending up with a rule violation the staff overview
///     can flag via <see cref="GetWarnings" />.
/// </summary>
public class ProfilRule : IProfundumIndividualRule
{
    private readonly AfraAppContext _dbContext;
    private readonly UserService _userService;
    private readonly IOptions<ProfundumConfiguration> _profundumConfiguration;

    ///
    public ProfilRule(AfraAppContext dbContext,
        UserService userService,
        IOptions<ProfundumConfiguration> profundumConfiguration)
    {
        _dbContext = dbContext;
        _userService = userService;
        _profundumConfiguration = profundumConfiguration;
    }

    private List<Guid>? _alleProfilKategorien;

    /// <summary>
    ///     The id of every Profil-Kategorie - fixed reference data that can't change within a single request, so it's
    ///     fetched once and reused for the rest of this rule instance's lifetime rather than re-queried per student.
    ///     Safe because <see cref="ProfilRule" /> is registered scoped (see <c>AddRulesExtension</c>) and resolved
    ///     once per request by <c>ServiceProviderRulesFactory</c>, which every per-student loop in
    ///     <c>ProfundumMatchingService</c> then reuses - this is not shared across requests.
    /// </summary>
    private List<Guid> AlleProfilKategorien =>
        _alleProfilKategorien ??= _dbContext.ProfundaKategorien.Where(k => k.ProfilProfundum).Select(k => k.Id).ToList();

    /// <inheritdoc/>
    public RuleStatus CheckForSubmission(Person student,
        IEnumerable<ProfundumSlot> slots,
        IEnumerable<ProfundumEinschreibung> enrollments,
        IEnumerable<ProfundumBelegWunsch> wuensche)
    {
        var klasse = _userService.GetKlassenstufe(student, DateTime.UtcNow);
        var enrollmentsArray = enrollments as ProfundumEinschreibung[] ?? enrollments.ToArray();
        var wuenscheArray = wuensche as ProfundumBelegWunsch[] ?? wuensche.ToArray();
        var wantsProfil = wuenscheArray.Any(w => w.ProfundumDefinition.Kategorie.ProfilProfundum);

        var zeitraum = wuenscheArray.Select(w => w.EinwahlZeitraum).FirstOrDefault();
        var profilErlaubt = zeitraum is not null && zeitraum.Slots.Any(s => IsProfilPflichtig(klasse, s.Quartal));

        if (wantsProfil && !profilErlaubt)
        {
            return RuleStatus.Invalid("Profilprofundum ist für diese Klassenstufe in diesem Halbjahr nicht vorgesehen.");
        }

        if (profilErlaubt && !wantsProfil)
        {
            var hatSchonProfil = enrollmentsArray.Any(e => zeitraum!.Slots.Contains(e.Slot)
                                                            && (e.ProfundumInstanz?.Profundum.Kategorie.ProfilProfundum ?? false));
            if (!hatSchonProfil)
            {
                return RuleStatus.Invalid("Profilprofundum ist nicht in der Einwahl enthalten.");
            }
        }

        if (klasse == 10)
        {
            var grade10Quartale = _profundumConfiguration.Value.ProfilPflichtigkeit.GetValueOrDefault(10) ?? [];
            var slotsArray = slots as ProfundumSlot[] ?? slots.ToArray();
            var inLetztemHalbjahr = slotsArray.Any(s => grade10Quartale.Contains(s.Quartal));
            if (inLetztemHalbjahr)
            {
                var belegteKategorien = enrollmentsArray
                    .Where(e => e.ProfundumInstanz?.Profundum.Kategorie.ProfilProfundum ?? false)
                    .Select(e => e.ProfundumInstanz!.Profundum.Kategorie.Id)
                    .ToHashSet();
                belegteKategorien.UnionWith(wuenscheArray
                    .Where(w => w.ProfundumDefinition.Kategorie.ProfilProfundum)
                    .Select(w => w.ProfundumDefinition.Kategorie.Id));

                if (AlleProfilKategorien.Except(belegteKategorien).Any())
                {
                    return RuleStatus.Invalid(
                        "Es müssen bis Ende Klasse 10 alle Profile belegt worden sein - bitte ein Profundum aus jedem fehlenden Profil in die Einwahl aufnehmen.");
                }
            }
        }

        return RuleStatus.Valid;
    }

    /// <inheritdoc/>
    public void AddConstraints(Person student,
        int klasse,
        IEnumerable<ProfundumSlot> slots,
        IEnumerable<ProfundumBelegWunsch> wuensche,
        Dictionary<(ProfundumSlot s, ProfundumInstanz i), BoolVar> belegVars,
        Dictionary<ProfundumSlot, BoolVar> personNotEnrolledVars,
        CpModel model,
        LinearExprBuilder objective)
    {
        var slotsArray = slots as ProfundumSlot[] ?? slots.ToArray();

        foreach (var (k, v) in belegVars)
        {
            if (k.i.Profundum.Kategorie.ProfilProfundum && !IsProfilPflichtig(klasse, k.s.Quartal))
            {
                objective.AddTerm(v, -20000);
            }
        }

        foreach (var einwahlzeitraumGroup in belegVars
                     .Where(x => x.Key.i.Profundum.Kategorie.ProfilProfundum)
                     .GroupBy(x => x.Key.s.EinwahlZeitraum.Id))
        {
            var profundenActive = new List<BoolVar>();
            foreach (var profundumGroup in einwahlzeitraumGroup.GroupBy(x => x.Key.i.Profundum.Id))
            {
                var vars = profundumGroup.Select(x => x.Value).ToList();
                var active = model.NewBoolVar($"profilActive-{student.Id}-{einwahlzeitraumGroup.Key}-{profundumGroup.Key}");
                model.AddMaxEquality(active, vars);
                profundenActive.Add(active);
            }

            if (profundenActive.Count <= 1)
                continue;

            var count = model.NewIntVar(0, profundenActive.Count, $"profilCount-{student.Id}-{einwahlzeitraumGroup.Key}");
            model.Add(count == LinearExpr.Sum(profundenActive));
            var excess = model.NewIntVar(0, profundenActive.Count, $"profilExcess-{student.Id}-{einwahlzeitraumGroup.Key}");
            model.Add(excess >= count - 1);
            objective.AddTerm(excess, -20000);
        }

        foreach (var group in slotsArray.GroupBy(s => s.EinwahlZeitraum.Id))
        {
            if (!group.Any(s => IsProfilPflichtig(klasse, s.Quartal)))
                continue;

            var profilVars = belegVars
                .Where(x => x.Key.s.EinwahlZeitraum.Id == group.Key && x.Key.i.Profundum.Kategorie.ProfilProfundum)
                .Select(x => x.Value)
                .ToList();
            if (profilVars.Count == 0)
                continue;

            var hasProfil = model.NewBoolVar($"hasProfil-{student.Id}-{group.Key}");
            model.AddMaxEquality(hasProfil, profilVars);
            objective.AddTerm(hasProfil.Not(), -10000);
        }
    }

    private bool IsProfilPflichtig(int klasse, ProfundumQuartal quartal)
    {
        var profilQuartale = _profundumConfiguration.Value.ProfilPflichtigkeit.GetValueOrDefault(klasse);
        return profilQuartale is not null && profilQuartale.Contains(quartal);
    }

    /// <inheritdoc/>
    public IEnumerable<MatchingWarning> GetWarnings(Person student, Func<DateTime, int> klasseAsOf, IEnumerable<ProfundumSlot> slots, IEnumerable<ProfundumEinschreibung> enrollments)
    {
        var slotsArray = slots as ProfundumSlot[] ?? slots.ToArray();
        var enrollmentsArray = enrollments as ProfundumEinschreibung[] ?? enrollments.ToArray();
        var profilEnrollments = enrollmentsArray.Where(e => e.ProfundumInstanz?.Profundum.Kategorie.ProfilProfundum ?? false).ToArray();

        DateTime AsOfForZeitraum(IReadOnlyCollection<ProfundumSlot> zeitraumSlots) =>
            enrollmentsArray.Where(e => zeitraumSlots.Contains(e.Slot)).Select(e => e.CreatedAt)
                .DefaultIfEmpty(DateTime.UtcNow).Min();

        var warnings = new List<MatchingWarning>();

        foreach (var group in slotsArray.GroupBy(s => s.EinwahlZeitraum.Id))
        {
            var zeitraumSlots = group.ToArray();
            var klasseForZeitraum = klasseAsOf(AsOfForZeitraum(zeitraumSlots));
            var profilErlaubt = zeitraumSlots.Any(s => IsProfilPflichtig(klasseForZeitraum, s.Quartal));
            var hatProfilInZeitraum = profilEnrollments.Any(e => zeitraumSlots.Contains(e.Slot));
            if (profilErlaubt && !hatProfilInZeitraum)
            {
                warnings.Add(new MatchingWarning($"Profilpflicht nicht erfüllt ({zeitraumSlots[0].Jahr})."));
            }
        }

        var mehrfachBelegt = profilEnrollments
            .GroupBy(e => e.Slot.EinwahlZeitraum.Id)
            .Any(g => g.Select(e => e.ProfundumInstanz!.Profundum.Id).Distinct().Count() > 1);
        if (mehrfachBelegt)
        {
            warnings.Add(new MatchingWarning("Mehr als ein Profilprofundum im selben Einwahlzeitraum belegt."));
        }

        var grade10Quartale = _profundumConfiguration.Value.ProfilPflichtigkeit.GetValueOrDefault(10) ?? [];
        var amEndeKlasse10 = slotsArray
            .GroupBy(s => s.EinwahlZeitraum.Id)
            .Select(g => g.ToArray())
            .Any(zeitraumSlots => klasseAsOf(AsOfForZeitraum(zeitraumSlots)) == 10
                                   && zeitraumSlots.Any(s => grade10Quartale.Contains(s.Quartal)));
        if (amEndeKlasse10)
        {
            var belegteKategorien = profilEnrollments.Select(e => e.ProfundumInstanz!.Profundum.Kategorie.Id).ToHashSet();
            if (AlleProfilKategorien.Except(belegteKategorien).Any())
            {
                warnings.Add(new MatchingWarning("Nicht alle Profil-Kategorien bis Ende Klasse 10 belegt."));
            }
        }

        return warnings;
    }
}
