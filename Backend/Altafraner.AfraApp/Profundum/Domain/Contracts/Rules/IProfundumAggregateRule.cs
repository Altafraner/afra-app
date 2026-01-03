using Altafraner.AfraApp.Profundum.Domain.Models;
using Altafraner.AfraApp.User.Domain.Models;
using Google.OrTools.Sat;

namespace Altafraner.AfraApp.Profundum.Domain.Contracts.Rules;

///
public interface IProfundumAggregateRule
{
    /// <summary>
    ///     Add constraints to matching solver
    /// </summary>
    void AddConstraints(
        IEnumerable<ProfundumSlot> slots,
        IEnumerable<Person> students,
        IEnumerable<ProfundumBelegWunsch> wuensche,
        Dictionary<ProfundumBelegWunsch, BoolVar> wuenscheVariables,
        CpModel model);
}
