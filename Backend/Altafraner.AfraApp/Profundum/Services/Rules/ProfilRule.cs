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
            ProfundumEinwahlZeitraum einwahlZeitraum,
            IEnumerable<ProfundumBelegWunsch> wuensche)
    {
        var slots = einwahlZeitraum.Slots.ToArray();
        var profilPflichtig = IsProfilPflichtig(student, slots.Select(s => s.Quartal));
        if (!profilPflichtig) return RuleStatus.Valid;
        return wuensche.Any(w => w.ProfundumInstanz.Profundum.Kategorie.ProfilProfundum)
            ? RuleStatus.Valid
            : RuleStatus.Invalid("Profilprofundum ist nicht in Einwahl enthalten.");
    }

    /// <inheritdoc/>
    public void AddConstraints(Person student,
        ProfundumEinwahlZeitraum einwahlZeitraum,
        IEnumerable<ProfundumBelegWunsch> wuensche,
        Dictionary<ProfundumBelegWunsch, BoolVar> wuenscheVariables,
        BoolVar personNotEnrolledVar,
        CpModel model
        )
    {
        var slots = einwahlZeitraum.Slots.ToArray();
        var profilPflichtig = IsProfilPflichtig(student, slots.Select(s => s.Quartal));
        if (!profilPflichtig) return;
        var profilWuensche = wuensche.Where(b => b.ProfundumInstanz.Profundum.Kategorie.ProfilProfundum);
        var profilWuenscheVars = profilWuensche.Select(b => wuenscheVariables[b]);
        model.AddAtLeastOne(profilWuenscheVars.Append(personNotEnrolledVar));
    }

    private bool IsProfilPflichtig(Person student, IEnumerable<ProfundumQuartal> quartale)
    {
        var klasse = _userService.GetKlassenstufe(student);
        var profilQuartale = _profundumConfiguration.Value.ProfilPflichtigkeit.GetValueOrDefault(klasse);
        return profilQuartale is not null && profilQuartale.Intersect(quartale).Any();
    }
}
