using Altafraner.AfraApp.Profundum.Domain.Models;
using Altafraner.AfraApp.User.Domain.Models;
using Google.OrTools.Sat;

namespace Altafraner.AfraApp.Profundum.Domain.Contracts.Rules;

/// <summary>
///     A profundum rule that is not constrained just by the enrollments of a single student
/// </summary>
public interface IProfundumAggregateRule
{
    /// <summary>
    ///     Add constraints to matching solver. <paramref name="objective" /> lets a rule prefer an outcome
    ///     without hard-blocking it (add a penalty term instead of a <c>model.Add</c> constraint) - not every
    ///     aggregate rule needs this, but the option must exist for the ones that do.
    /// </summary>
    void AddConstraints(
        IEnumerable<ProfundumSlot> slots,
        IEnumerable<Person> students,
        IEnumerable<ProfundumBelegWunsch> wuensche,
        Dictionary<(Person p, ProfundumSlot s, ProfundumInstanz i), BoolVar> belegVars,
        CpModel model,
        LinearExprBuilder objective);
}
