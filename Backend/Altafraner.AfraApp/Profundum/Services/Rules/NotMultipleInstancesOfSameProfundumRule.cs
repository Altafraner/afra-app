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
        Dictionary<(Person, ProfundumSlot, ProfundumInstanz), BoolVar> belegVars,
        IEnumerable<BoolVar> personNotEnrolledVars,
        CpModel model
        )
    {
        var personen = belegVars.Keys.ToArray().Select(x => x.Item1).Distinct().ToArray();
        var profundaInstanzen = belegVars.Keys.ToArray().Select(x => x.Item3).Distinct().ToArray();
        var profundaDefinitionen = profundaInstanzen.Select(p => p.Profundum).Distinct().ToArray();

        var instanceActive = new Dictionary<(Person, ProfundumInstanz), BoolVar>();
        foreach (var person in personen)
        {
            foreach (var instanz in profundaInstanzen)
            {
                var varsForInstance = belegVars
                    .Where(kv =>
                        kv.Key.Item1 == person &&
                        kv.Key.Item3 == instanz)
                    .Select(kv => kv.Value)
                    .ToArray();

                if (varsForInstance.Length == 0)
                    continue;

                var active = model.NewBoolVar(
                    $"active_{person.Id}_{instanz.Id}");

                instanceActive[(person, instanz)] = active;

                foreach (var v in varsForInstance)
                    model.AddImplication(v, active);

                model.Add(LinearExpr.Sum(varsForInstance) >= active);
            }
        }

        foreach (var person in personen)
        {
            var instanzenByDefinition = instanceActive.Keys
                .Where(k => k.Item1 == person)
                .GroupBy(k => k.Item2.Profundum);

            foreach (var defGroup in instanzenByDefinition)
            {
                var actives = defGroup
                    .Select(k => instanceActive[k])
                    .ToArray();

                if (actives.Length > 1)
                {
                    model.Add(LinearExpr.Sum(actives) <= 1);
                }
            }
        }
    }
}
