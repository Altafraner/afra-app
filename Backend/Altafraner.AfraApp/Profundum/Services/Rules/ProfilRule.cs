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
            IEnumerable<ProfundumBelegWunsch> wuensche)
    {
        var profilPflichtig = IsProfilPflichtig(student, slots.Select(s => s.Quartal));
        if (!profilPflichtig) return RuleStatus.Valid;
        return wuensche.Any(w => w.ProfundumInstanz.Profundum.Kategorie.ProfilProfundum)
            ? RuleStatus.Valid
            : RuleStatus.Invalid("Profilprofundum ist nicht in Einwahl enthalten.");
    }

    /// <inheritdoc/>
    public void AddConstraints(Person student,
        IEnumerable<ProfundumSlot> slots,
        IEnumerable<ProfundumBelegWunsch> wuensche,
        Dictionary<(Person, ProfundumSlot, ProfundumInstanz), BoolVar> belegVars,
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
                .Where(x => x.Key.Item1 == student)
                .Where(x => quartalGroup.Contains(x.Key.Item2))
                .Where(x => x.Key.Item3.Profundum.Kategorie.ProfilProfundum)
                .Select(x => x.Value)
                .ToList();

            var notEnrolledVars = quartalGroup
                .Select(s => personNotEnrolledVars[s]);

            model.AddAtLeastOne(profilVars.Concat(notEnrolledVars));
        }

        foreach (var (k, v) in belegVars)
        {
            if (!IsProfilZulaessig(k.Item1, k.Item2.Quartal)
             && !IsProfilPflichtig(k.Item1, k.Item2.Quartal)
             && k.Item3.Profundum.Kategorie.ProfilProfundum)
            {
                model.Add(v == 0);
            }
        }
    }

    public bool IsProfilZulaessig(Person student, ProfundumQuartal quartal)
    {
        var klasse = student.Gruppe;
        if (klasse is null) return false;

        var profilQuartale = _profundumConfiguration.Value.ProfilZulassung.GetValueOrDefault(klasse);
        if (profilQuartale is null) return false;

        var ret = profilQuartale.Contains(quartal);
        return ret;
    }

    private bool IsProfilPflichtig(Person student, IEnumerable<ProfundumQuartal> quartale)
    {
        var klasse = _userService.GetKlassenstufe(student);
        var profilQuartale = _profundumConfiguration.Value.ProfilPflichtigkeit.GetValueOrDefault(klasse);
        return profilQuartale is not null && profilQuartale.Intersect(quartale).Any();
    }

    private bool IsProfilPflichtig(Person student, ProfundumQuartal quartal)
    {
        var klasse = _userService.GetKlassenstufe(student);
        var profilQuartale = _profundumConfiguration.Value.ProfilPflichtigkeit.GetValueOrDefault(klasse);
        return profilQuartale is not null && profilQuartale.Contains(quartal);
    }
}
