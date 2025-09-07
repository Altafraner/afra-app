using Afra_App.Profundum.Configuration;
using Afra_App.Profundum.Domain.Models;
using Afra_App.User.Domain.Models;
using Afra_App.User.Services;
using Google.OrTools.Sat;
using Microsoft.Extensions.Options;
using Afra_App.Profundum.Domain.Contracts.Rules;

namespace Afra_App.Profundum.Services.Rules;

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
        var profilPflichtig = isProfilPflichtig(student, slots.Select(s => s.Quartal));
        if (profilPflichtig)
        {
            if (!wuensche.Any(w => w.ProfundumInstanz.Profundum.Kategorie.ProfilProfundum))
            {
                return RuleStatus.Invalid("Profilprofundum ist nicht in Einwahl enthalten.");
            }
        }
        return RuleStatus.Valid;
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
        var profilPflichtig = isProfilPflichtig(student, slots.Select(s => s.Quartal));
        if (profilPflichtig)
        {
            var profilWuensche = wuensche.Where(b => b.ProfundumInstanz.Profundum.Kategorie.ProfilProfundum);
            var profilWuenscheVars = profilWuensche.Select(b => wuenscheVariables[b]);
            model.AddAtLeastOne(profilWuenscheVars.Append(personNotEnrolledVar));
        }
    }

    private bool isProfilPflichtig(Person student, IEnumerable<ProfundumQuartal> quartale)
    {
        var klasse = _userService.GetKlassenstufe(student);
        var profilQuartale = _profundumConfiguration.Value.ProfilPflichtigkeit.GetValueOrDefault(klasse);
        if (profilQuartale is null)
        {
            return false;
        }
        return profilQuartale.Intersect(quartale).Any();
    }
}
