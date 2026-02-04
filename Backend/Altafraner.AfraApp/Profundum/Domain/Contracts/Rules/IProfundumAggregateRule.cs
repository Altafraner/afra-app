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
    ///     Add constraints to matching solver
    /// </summary>
    void AddConstraints(
        IEnumerable<ProfundumSlot> slots,
        IEnumerable<Person> students,
        IEnumerable<ProfundumBelegWunsch> wuensche,
        Dictionary<(Person p, ProfundumSlot s, ProfundumInstanz i), BoolVar> belegVars,
        CpModel model
    );
}
