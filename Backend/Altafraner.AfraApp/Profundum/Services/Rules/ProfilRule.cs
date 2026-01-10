using Altafraner.AfraApp.Profundum.Configuration;
using Altafraner.AfraApp.Profundum.Domain.Contracts.Rules;
using Altafraner.AfraApp.Profundum.Domain.Models;
using Altafraner.AfraApp.User.Domain.Models;
using Altafraner.AfraApp.User.Services;
using Google.OrTools.Sat;
using Microsoft.Extensions.Options;

namespace Altafraner.AfraApp.Profundum.Services.Rules;

///
public class ProfilRule : IProfundumIndividualRule
{
    private readonly UserService _userService;
    private readonly IOptions<ProfundumConfiguration> _profundumConfiguration;

    ///
    public ProfilRule(UserService userService, IOptions<ProfundumConfiguration> profundumConfiguration)
    {
        _userService = userService;
        _profundumConfiguration = profundumConfiguration;
    }

    /// <inheritdoc/>
    public RuleStatus CheckForSubmission(Person student,
        IEnumerable<ProfundumSlot> slots,
        IEnumerable<ProfundumEinschreibung> enrollments,
        IEnumerable<ProfundumBelegWunsch> wuensche)
    {
        var profilPflichtig = slots.Any(s => IsProfilPflichtig(student, s.Quartal));
        if (!profilPflichtig) return RuleStatus.Valid;
        return enrollments.Any(w => w.ProfundumInstanz?.Profundum?.Kategorie?.ProfilProfundum ?? false) ||
            wuensche.Any(w => w.ProfundumInstanz.Profundum.Kategorie.ProfilProfundum)
            ? RuleStatus.Valid
            : RuleStatus.Invalid("Profilprofundum ist nicht in Einwahl enthalten.");
    }

    /// <inheritdoc/>
    public void AddConstraints(Person student,
        IEnumerable<ProfundumSlot> slots,
        IEnumerable<ProfundumBelegWunsch> wuensche,
        Dictionary<(ProfundumSlot, ProfundumInstanz), BoolVar> belegVars,
        Dictionary<ProfundumSlot, BoolVar> personNotEnrolledVars,
        CpModel model,
        LinearExprBuilder objective)
    {
        var pflichtQuartale = slots
            .Where(s => IsProfilPflichtig(student, s.Quartal))
        .GroupBy(s => (s.Jahr, s.Quartal));

        foreach (var quartalGroup in slots
            .Where(s => IsProfilPflichtig(student, s.Quartal))
            .GroupBy(s => s.Quartal))
        {
            var profilVars = belegVars
                .Where(x => quartalGroup.Contains(x.Key.Item1))
                .Where(x => x.Key.Item2.Profundum.Kategorie.ProfilProfundum)
                .Select(x => x.Value)
                .ToList();
            var hasProfil = model.NewBoolVar($"hasProfil-{student.Id}-{quartalGroup.Key}");
            model.AddMaxEquality(hasProfil, profilVars);
            objective.AddTerm(hasProfil.Not(), -4000);
        }

        {
            var profilPflichtig = slots.Any(s => IsProfilPflichtig(student, s.Quartal));
            var profilVars = belegVars
                .Where(x => x.Key.Item2.Profundum.Kategorie.ProfilProfundum)
                .Select(x => x.Value)
                .ToList();
            var hasProfil = model.NewBoolVar($"hasProfil-{student.Id}");
            model.AddMaxEquality(hasProfil, profilVars);
            objective.AddTerm(hasProfil.Not(), -10000);
        }

        foreach (var (k, v) in belegVars)
        {
            // Profil im falschen Quartal
            if (!IsProfilZulaessig(student, k.Item1.Quartal)
             && !IsProfilPflichtig(student, k.Item1.Quartal)
             && k.Item2.Profundum.Kategorie.ProfilProfundum)
            {
                objective.AddTerm(v, -1000);
            }

            // Profil im ganzen Jahr unzulässig
            var profilZulässig = slots.Any(s =>
                    IsProfilPflichtig(student, s.Quartal)
                    || IsProfilZulaessig(student, s.Quartal));
            if (!profilZulässig && k.Item2.Profundum.Kategorie.ProfilProfundum)
            {
                objective.AddTerm(v, -10000);
            }
        }
    }

    /// <inheritdoc/>
    public bool IsProfilZulaessig(Person student, ProfundumQuartal quartal)
    {
        var klasse = student.Gruppe;
        if (klasse is null) return false;

        var profilQuartale = _profundumConfiguration.Value.ProfilZulassung.GetValueOrDefault(klasse);
        if (profilQuartale is null) return false;

        var ret = profilQuartale.Contains(quartal);
        return ret;
    }

    private bool IsProfilPflichtig(Person student, ProfundumQuartal quartal)
    {
        var klasse = _userService.GetKlassenstufe(student);
        var profilQuartale = _profundumConfiguration.Value.ProfilPflichtigkeit.GetValueOrDefault(klasse);
        return profilQuartale is not null && profilQuartale.Contains(quartal);
    }

    /// <inheritdoc/>
    public IEnumerable<string> GetWarnings(Person student, IEnumerable<ProfundumSlot> slots, IEnumerable<ProfundumEinschreibung> enrollments)
    {
        var keinProfundum = slots.Select(s => (s.Jahr, s.Quartal)).Distinct()
            .Where((x => IsProfilPflichtig(student, x.Quartal)))
            .Where(x => !enrollments.Any(e => e.BetroffenePerson == student
                     && e.Slot.Jahr == x.Jahr && e.Slot.Quartal == x.Quartal
                     && e.ProfundumInstanz!.Profundum.Kategorie.ProfilProfundum))
            .Select(x => $"Profil (Pflicht für {student.Gruppe}) fehlt in {x.Jahr}, {x.Quartal}");


        var profundumUnzulässig = slots.Select(s => (s.Jahr, s.Quartal)).Distinct()
            .Where((x => !IsProfilPflichtig(student, x.Quartal) && !IsProfilZulaessig(student, x.Quartal)))
            .Where(x => enrollments.Any(e => e.BetroffenePerson == student
                     && e.Slot.Jahr == x.Jahr && e.Slot.Quartal == x.Quartal
                     && e.ProfundumInstanz!.Profundum.Kategorie.ProfilProfundum))
            .Select(x => $"Profil nicht erlaubt für {student.Gruppe} in {x.Jahr}, {x.Quartal}");

        return keinProfundum.Union(profundumUnzulässig);
    }
}
