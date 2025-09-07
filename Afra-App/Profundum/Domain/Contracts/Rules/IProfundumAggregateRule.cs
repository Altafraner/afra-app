using Afra_App.Profundum.Domain.Models;
using Afra_App.User.Domain.Models;
using Google.OrTools.Sat;

namespace Afra_App.Profundum.Domain.Contracts.Rules;

///
public interface IProfundumAggregateRule
{
    /// <summary>
    ///     Add constraints to matching solver
    /// </summary>
    public void AddConstraints(
        ProfundumEinwahlZeitraum einwahlZeitraum,
        IEnumerable<Person> students,
        IEnumerable<ProfundumBelegWunsch> wuensche,
        Dictionary<ProfundumBelegWunsch, BoolVar> wuenscheVariables,
        CpModel model);
}
