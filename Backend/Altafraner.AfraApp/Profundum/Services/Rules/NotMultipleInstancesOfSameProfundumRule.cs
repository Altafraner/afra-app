using Altafraner.AfraApp.Profundum.Domain.Contracts.Rules;
using Altafraner.AfraApp.Profundum.Domain.DTO;
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
        IEnumerable<ProfundumEinschreibung> enrollments,
        IEnumerable<ProfundumBelegWunsch> wuensche)
    {
        var emsgs = wuensche.Where(w => enrollments.Any(e => e.ProfundumInstanz?.Profundum == w.ProfundumInstanz.Profundum))
            .Select(w => $"{w.ProfundumInstanz.Profundum.Bezeichnung} bereits belegt.");
        if (emsgs.Any())
        {
            return RuleStatus.Invalid(emsgs);
        }
        return RuleStatus.Valid;
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
        var profundaInstanzen = belegVars.Keys.ToArray().Select(x => x.i).Distinct().ToArray();
        var profundaDefinitionen = profundaInstanzen.Select(p => p.Profundum).Distinct().ToArray();

        var instanceActive = new Dictionary<ProfundumInstanz, BoolVar>();
        foreach (var instanz in profundaInstanzen)
        {
            var varsForInstance = belegVars
                .Where(kv => kv.Key.i == instanz)
                .Select(kv => kv.Value)
                .ToArray();

            if (varsForInstance.Length == 0)
                continue;

            var active = model.NewBoolVar(
                $"active_{student.Id}_{instanz.Id}");

            instanceActive[instanz] = active;

            foreach (var v in varsForInstance)
                model.AddImplication(v, active);

            model.Add(LinearExpr.Sum(varsForInstance) >= active);
        }

        var instanzenByDefinition = instanceActive.Keys
            .GroupBy(k => k.Profundum);

        foreach (var defGroup in instanzenByDefinition)
        {
            var actives = defGroup
                .Select(k => instanceActive[k])
                .ToArray();

            if (actives.Length > 1)
            {
                var count = model.NewIntVar(0, actives.Length, $"count_{student.Id}_{defGroup.Key.Id}");
                model.Add(count == LinearExpr.Sum(actives));
                var excess = model.NewIntVar(
                    0, actives.Length, $"excess_{student.Id}_{defGroup.Key.Id}");
                model.Add(excess >= count - 1);
                model.Add(excess >= 0);
                objective.AddTerm(excess, -5000);
            }
        }
    }

    /// <inheritdoc/>
    public IEnumerable<MatchingWarning> GetWarnings(Person student, IEnumerable<ProfundumSlot> slots, IEnumerable<ProfundumEinschreibung> enrollments)
        => enrollments.GroupBy(e => e.ProfundumInstanz!.Profundum)
            .Where(x => x.Select(x => x.ProfundumInstanz).Distinct().Count() > 1)
            .Select(x => new MatchingWarning($"Mehrere Instanzen desselben Profundums: {x.Key.Bezeichnung}"));
}
