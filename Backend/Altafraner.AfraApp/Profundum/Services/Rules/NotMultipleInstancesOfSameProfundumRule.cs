using Altafraner.AfraApp.Profundum.Configuration;
using Altafraner.AfraApp.Profundum.Domain.Models;
using Altafraner.AfraApp.User.Domain.Models;
using Altafraner.AfraApp.User.Services;
using Google.OrTools.Sat;
using Microsoft.Extensions.Options;
using Altafraner.AfraApp.Profundum.Domain.Contracts.Rules;

namespace Altafraner.AfraApp.Profundum.Services.Rules;

///
public class NotMultipleInstancesOfSameProfundumRule : IProfundumIndividualRule
{
    private readonly UserService _userService;
    private readonly IOptions<ProfundumConfiguration> _profundumConfiguration;

    ///
    public NotMultipleInstancesOfSameProfundumRule(UserService userService, IOptions<ProfundumConfiguration> profundumConfiguration)
    {
        _userService = userService;
        _profundumConfiguration = profundumConfiguration;
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

        var profundaDefinitionenIds = wuensche
            .Where(b => b.BetroffenePerson.Id == student.Id)
            .Select(b => b.ProfundumInstanz.Profundum.Id)
            .ToHashSet();

        foreach (var defId in profundaDefinitionenIds)
        {
            var psBeleg = wuensche
                .Where(b => b.ProfundumInstanz.Profundum.Id == defId);
            var psBelegVar = psBeleg.Select(b => wuenscheVariables[b]).ToArray();
            model.AddAtMostOne(psBelegVar);
        }
    }
}
