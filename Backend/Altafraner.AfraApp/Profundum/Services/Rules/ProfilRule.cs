using Altafraner.AfraApp.Profundum.Configuration;
using Altafraner.AfraApp.Profundum.Domain.Contracts.Rules;
using Altafraner.AfraApp.Profundum.Domain.DTO;
using Altafraner.AfraApp.Profundum.Domain.Models;
using Altafraner.AfraApp.User.Domain.Models;
using Altafraner.AfraApp.User.Services;
using Google.OrTools.Sat;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Altafraner.AfraApp.Profundum.Services.Rules;

///
public class ProfilRule : IProfundumIndividualRule
{
    private readonly AfraAppContext _dbContext;
    private readonly UserService _userService;
    private readonly IOptions<ProfundumConfiguration> _profundumConfiguration;
    private readonly IMemoryCache _cache;

    ///
    public ProfilRule(AfraAppContext dbContext, UserService userService, IOptions<ProfundumConfiguration> profundumConfiguration, IMemoryCache cache)
    {
        _dbContext = dbContext;
        _userService = userService;
        _profundumConfiguration = profundumConfiguration;
        _cache = cache;
    }

    /// <inheritdoc/>
    public RuleStatus CheckForSubmission(Person student,
        IEnumerable<ProfundumSlot> slots,
        IEnumerable<ProfundumEinschreibung> enrollments,
        IEnumerable<ProfundumBelegWunsch> wuensche)
    {
        var profilPflichtig = slots.Any(s => IsProfilPflichtig(student, s.Quartal));
        if (!profilPflichtig)
        {
            return RuleStatus.Valid;
        }

        if (IsProfilRegelBefreit(student))
        {
            return RuleStatus.Valid;
        }

        if (enrollments.Any(w => w.ProfundumInstanz?.Profundum?.Kategorie?.ProfilProfundum ?? false))
        {
            if (wuensche.Any(w => w.ProfundumInstanz.Profundum.Kategorie.ProfilProfundum))
            {
                return RuleStatus.Invalid("Profil bereits belegt.");
            }
            return RuleStatus.Valid;
        }
        if (wuensche.Any(w => w.ProfundumInstanz.Profundum.Kategorie.ProfilProfundum))
        {
            return RuleStatus.Valid;
        }

        return RuleStatus.Invalid("Profilprofundum ist nicht in Einwahl enthalten.");
    }

    /// <inheritdoc/>
    public void AddConstraints(Person student,
        IEnumerable<ProfundumSlot> slots,
        IEnumerable<ProfundumBelegWunsch> wuensche,
        Dictionary<(ProfundumSlot s, ProfundumInstanz i), BoolVar> belegVars,
        Dictionary<ProfundumSlot, BoolVar> personNotEnrolledVars,
        CpModel model,
        LinearExprBuilder objective)
    {
        if (IsProfilRegelBefreit(student))
        {
            return;
        }

        var pflichtQuartale = slots
            .Where(s => IsProfilPflichtig(student, s.Quartal))
        .GroupBy(s => (s.Jahr, s.Quartal));

        foreach (var quartalGroup in slots
            .Where(s => IsProfilPflichtig(student, s.Quartal))
            .GroupBy(s => s.Quartal))
        {
            var profilVars = belegVars
                .Where(x => quartalGroup.Contains(x.Key.s))
                .Where(x => x.Key.i.Profundum.Kategorie.ProfilProfundum)
                .Select(x => x.Value)
                .ToList();
            var hasProfil = model.NewBoolVar($"hasProfil-{student.Id}-{quartalGroup.Key}");
            model.AddMaxEquality(hasProfil, profilVars);
            objective.AddTerm(hasProfil.Not(), -4000);
        }

        {
            var profilPflichtig = slots.Any(s => IsProfilPflichtig(student, s.Quartal));
            var profilVars = belegVars
                .Where(x => x.Key.i.Profundum.Kategorie.ProfilProfundum)
                .Select(x => x.Value)
                .ToList();
            var hasProfil = model.NewBoolVar($"hasProfil-{student.Id}");
            model.AddMaxEquality(hasProfil, profilVars);
            objective.AddTerm(hasProfil.Not(), -10000);
        }

        foreach (var (k, v) in belegVars)
        {
            // Profil im falschen Quartal
            if (!IsProfilZulaessig(student, k.s.Quartal)
             && !IsProfilPflichtig(student, k.s.Quartal)
             && k.i.Profundum.Kategorie.ProfilProfundum)
            {
                objective.AddTerm(v, -1000);
            }

            // Profil im ganzen Jahr unzulässig
            var profilZulässig = slots.Any(s =>
                    IsProfilPflichtig(student, s.Quartal)
                    || IsProfilZulaessig(student, s.Quartal));
            if (!profilZulässig && k.i.Profundum.Kategorie.ProfilProfundum)
            {
                objective.AddTerm(v, -10000);
            }
        }
    }

    private bool IsProfilZulaessig(Person student, ProfundumQuartal quartal)
    {
        var klasse = student.Gruppe;
        if (klasse is null) return false;

        var profilQuartale = _profundumConfiguration.Value.ProfilZulassung.GetValueOrDefault(klasse);
        if (profilQuartale is null) return false;

        var ret = profilQuartale.Contains(quartal);
        return ret;
    }

    private bool IsProfilRegelBefreit(Person student)
        => _cache.GetOrCreate($"profundum:befreiung:{student.Id}",
                _ => _dbContext.ProfundumProfilBefreiungen.Any(pb => pb.BetroffenePerson == student));

    private bool IsProfilPflichtig(Person student, ProfundumQuartal quartal)
    {

        var klasse = _userService.GetKlassenstufe(student);
        var profilQuartale = _profundumConfiguration.Value.ProfilPflichtigkeit.GetValueOrDefault(klasse);
        return profilQuartale is not null && profilQuartale.Contains(quartal);
    }

    /// <inheritdoc/>
    public IEnumerable<MatchingWarning> GetWarnings(Person student, IEnumerable<ProfundumSlot> slots, IEnumerable<ProfundumEinschreibung> enrollments)
    {
        if (IsProfilRegelBefreit(student))
        {
            return [new MatchingWarning("Person ist von der Profilregel ausgenommen worden. Anforderungen prüfen!")];
        }

        List<MatchingWarning> warnings = [];
        var profilPflichtig = slots.Any(s => IsProfilPflichtig(student, s.Quartal));
        if (profilPflichtig && !enrollments.Any(e => e.BetroffenePerson == student
                     && e.ProfundumInstanz!.Profundum.Kategorie.ProfilProfundum))
        {
            warnings.Add(new MatchingWarning("Profilpflicht nicht erfüllt."));
        }

        warnings.AddRange(slots.Select(s => (s.Jahr, s.Quartal)).Distinct()
            .Where((x => !IsProfilPflichtig(student, x.Quartal) && !IsProfilZulaessig(student, x.Quartal)))
            .Where(x => enrollments.Any(e => e.BetroffenePerson == student
                     && e.Slot.Jahr == x.Jahr && e.Slot.Quartal == x.Quartal
                     && e.ProfundumInstanz!.Profundum.Kategorie.ProfilProfundum))
            .Select(x => new MatchingWarning($"Profil nicht erlaubt für {student.Gruppe} in {x.Quartal}")));

        return warnings;
    }
}
