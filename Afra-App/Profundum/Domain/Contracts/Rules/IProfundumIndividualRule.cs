using Afra_App.Profundum.Domain.Models;
using Afra_App.User.Domain.Models;
using Google.OrTools.Sat;

namespace Afra_App.Profundum.Domain.Contracts.Rules;

///
public interface IProfundumIndividualRule
{
    ///
    public RuleStatus CheckForSubmission(Person student,
    ProfundumEinwahlZeitraum einwahlZeitraum,
    IEnumerable<ProfundumBelegWunsch> wuensche)
        => RuleStatus.Valid;

    /// <summary>
    ///     Add constraints to matching solver
    /// </summary>
    public void AddConstraints(Person student,
        ProfundumEinwahlZeitraum einwahlZeitraum,
        IEnumerable<ProfundumBelegWunsch> wuensche,
        Dictionary<ProfundumBelegWunsch, BoolVar> wuenscheVariables,
        BoolVar personNotEnrolledVar,
        CpModel model)
    { }
}
