using Altafraner.AfraApp.Profundum.Domain.Contracts.Rules;
using Altafraner.AfraApp.Profundum.Domain.Models;
using Altafraner.AfraApp.User.Domain.Models;
using Google.OrTools.Sat;

namespace Altafraner.AfraApp.Profundum.Services.Rules;

///
public class NotMultipleInstancesOfSameProfundumRule : IProfundumIndividualRule
{
    /// <inheritdoc/>
    public RuleStatus CheckForSubmission(Person student,
        IEnumerable<ProfundumSlot> slots,
        IEnumerable<ProfundumBelegWunsch> wuensche)
        => RuleStatus.Valid;

    /// <inheritdoc/>
    public void AddConstraints(Person student,
        IEnumerable<ProfundumSlot> slots,
        IEnumerable<ProfundumBelegWunsch> wuensche,
        Dictionary<ProfundumBelegWunsch, BoolVar> wuenscheVariables,
        IEnumerable<BoolVar> personNotEnrolledVars,
        CpModel model
        )
    {
        var wuenscheArray = wuensche as ProfundumBelegWunsch[] ?? wuensche.ToArray();

        var profundaDefinitionenIds = wuenscheArray
            .Where(b => b.BetroffenePerson.Id == student.Id)
            .Select(b => b.ProfundumInstanz.Profundum.Id)
            .ToHashSet();

        foreach (var defId in profundaDefinitionenIds)
        {
            var psBeleg = wuenscheArray
                .Where(b => b.ProfundumInstanz.Profundum.Id == defId);
            var psBelegVar = psBeleg.Select(b => wuenscheVariables[b]).ToArray();
            model.AddAtMostOne(psBelegVar);
        }
    }
}
