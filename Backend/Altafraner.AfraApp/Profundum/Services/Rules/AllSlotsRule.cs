using Altafraner.AfraApp.Profundum.Domain.Contracts.Rules;
using Altafraner.AfraApp.Profundum.Domain.DTO;
using Altafraner.AfraApp.Profundum.Domain.Models;
using Altafraner.AfraApp.User.Domain.Models;
using Google.OrTools.Sat;
using Microsoft.EntityFrameworkCore;

namespace Altafraner.AfraApp.Profundum.Services.Rules;

///
public class AllSlotsRule : IProfundumIndividualRule
{
    /// <inheritdoc/>
    public RuleStatus CheckForSubmission(Person student,
        IEnumerable<ProfundumSlot> slots,
    IEnumerable<ProfundumEinschreibung> enrollments,
            IEnumerable<ProfundumBelegWunsch> wuensche)
        => RuleStatus.Valid;

    /// <inheritdoc/>
    public void AddConstraints(Person student,
        IEnumerable<ProfundumSlot> slots,
        IEnumerable<ProfundumBelegWunsch> wuensche,
        Dictionary<(ProfundumSlot s, ProfundumInstanz i), BoolVar> belegVars,
        Dictionary<ProfundumSlot, BoolVar> personNotEnrolledVars,
        CpModel model,
        LinearExprBuilder objective)
    {
        var angebote = belegVars.Keys.Select(x => x.i).Distinct();
        foreach (var i in angebote)
        {
            var psVars = i.Slots.Select(s => belegVars[(s, i)]).ToArray();
            foreach (var (v, w) in psVars.Zip(psVars.Skip(1)))
            {
                var ineq = model.NewBoolVar($"{new Guid()}");
                model.Add(v != w).OnlyEnforceIf(ineq);
                model.Add(v == w).OnlyEnforceIf(ineq.Not());
                objective.AddTerm(ineq, -5000);
            }
        }
    }

    /// <inheritdoc/>
    public IEnumerable<MatchingWarning> GetWarnings(Person student, IEnumerable<ProfundumSlot> slots, IEnumerable<ProfundumEinschreibung> enrollments)
    {
        List<MatchingWarning> warnings = [];
        var angebote = enrollments
            .Where(p => p.ProfundumInstanz is not null)
            .Select(x => x.ProfundumInstanz!).Distinct();
        foreach (var i in angebote)
        {
            var belegtSlots = enrollments.Where(e => e.ProfundumInstanz == i).Select(e => e.Slot).ToArray();
            foreach (var b in belegtSlots.Except(i.Slots))
            {
                warnings.Add(new MatchingWarning($"Einschreibung in {i.Profundum.Bezeichnung}. Obwohl es in {b} nicht stattfindet. Vermutlich wurde das Profundum nach der Wahl geändert."));
            }
            foreach (var b in i.Slots.Except(belegtSlots))
            {
                warnings.Add(new MatchingWarning($"Der Einschreibung in {i.Profundum.Bezeichnung} fehlt der Slot {b}."));
            }
        }
        return warnings;
    }
}
