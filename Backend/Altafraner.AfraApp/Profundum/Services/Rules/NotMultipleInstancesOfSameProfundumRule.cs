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
        var emsgs = wuensche.Where(w => IsAlreadyEnrolled(w.ProfundumDefinition, enrollments))
            .Select(w => $"{w.ProfundumDefinition.Bezeichnung} bereits belegt.")
            .ToArray();
        return emsgs.Length != 0 ? RuleStatus.Invalid(emsgs) : RuleStatus.Valid;
    }

    /// <inheritdoc/>
    public RuleStatus CheckDefinitionEligibility(Person student,
        ProfundumDefinition definition,
        IEnumerable<ProfundumSlot> slots,
        IEnumerable<ProfundumEinschreibung> enrollments)
        => IsAlreadyEnrolled(definition, enrollments)
            ? RuleStatus.Invalid($"{definition.Bezeichnung} bereits belegt.")
            : RuleStatus.Valid;

    private static bool IsAlreadyEnrolled(ProfundumDefinition definition, IEnumerable<ProfundumEinschreibung> enrollments)
        => enrollments.Any(e => e.ProfundumInstanz?.Profundum == definition);

    /// <inheritdoc/>
    public void AddConstraints(Person student,
        int klasse,
        IEnumerable<ProfundumSlot> slots,
        IEnumerable<ProfundumEinschreibung> enrollments,
        Dictionary<(ProfundumSlot s, ProfundumInstanz i), BoolVar> belegVars,
        CpModel model,
        LinearExprBuilder objective)
    {
        var profundaInstanzen = belegVars.Keys.ToArray().Select(x => x.i).Distinct().ToArray();

        var instanceActive = new Dictionary<ProfundumInstanz, BoolVar>();

        // mehrquartalige Profunda werden zu einer Variable
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
            // instanzen pro Definition
            var actives = defGroup
                .Select(k => instanceActive[k])
                .ToArray();

            if (actives.Length <= 1) continue;

            // Anzahl eingeschriebener Instanzen
            var count = model.NewIntVar(0, actives.Length, $"count_{student.Id}_{defGroup.Key.Id}");
            model.Add(count == LinearExpr.Sum(actives));
            var excess = model.NewIntVar(
                0,
                actives.Length,
                $"excess_{student.Id}_{defGroup.Key.Id}");
            model.AddMaxEquality(excess, [count - 1, LinearExpr.Constant(0)]);
            objective.AddTerm(excess, -5000);
        }
    }

    /// <inheritdoc/>
    public IEnumerable<MatchingWarning> GetWarnings(Person student, int klasse, IEnumerable<ProfundumSlot> slots, IEnumerable<ProfundumEinschreibung> enrollments)
    {
        var slotsArray = slots as ProfundumSlot[] ?? slots.ToArray();
        return enrollments.GroupBy(e => e.ProfundumInstanz!.Profundum)
            .Where(x => x.Select(x => x.ProfundumInstanz).Distinct().Count() > 1)
            .Where(x => x.Any(e => slotsArray.Contains(e.Slot)))
            .Select(x => new MatchingWarning($"Mehrere Instanzen desselben Profundums: {x.Key.Bezeichnung}"));
    }
}
