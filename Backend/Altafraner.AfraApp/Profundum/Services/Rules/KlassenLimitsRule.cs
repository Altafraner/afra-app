using Altafraner.AfraApp.Profundum.Configuration;
using Altafraner.AfraApp.Profundum.Domain.Contracts.Rules;
using Altafraner.AfraApp.Profundum.Domain.Models;
using Altafraner.AfraApp.User.Domain.Models;
using Altafraner.AfraApp.User.Services;
using Google.OrTools.Sat;
using Microsoft.Extensions.Options;

namespace Altafraner.AfraApp.Profundum.Services.Rules;

///
public class KlassenLimitsRule : IProfundumIndividualRule
{
    private readonly UserService _userService;
    private readonly IOptions<ProfundumConfiguration> _profundumConfiguration;

    ///
    public KlassenLimitsRule(UserService userService, IOptions<ProfundumConfiguration> profundumConfiguration)
    {
        _userService = userService;
        _profundumConfiguration = profundumConfiguration;
    }

    /// <inheritdoc/>
    public RuleStatus CheckForSubmission(Person student,
        IEnumerable<ProfundumSlot> slots,
            IEnumerable<ProfundumBelegWunsch> wuensche)
    {
        return RuleStatus.Valid;
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
        var klasse = _userService.GetKlassenstufe(student);

        foreach (var (k, v) in belegVars)
        {
            var (s, i) = k;
            var minKlasse = i.Profundum.MinKlasse;
            var maxKlasse = i.Profundum.MaxKlasse;
            if (minKlasse is not null && klasse < minKlasse)
            {
                objective.AddTerm(v, -10000);
            }
            if (maxKlasse is not null && klasse > maxKlasse)
            {
                objective.AddTerm(v, -10000);
            }
        }
    }
}
