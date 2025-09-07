using Altafraner.AfraApp.Profundum.Domain.Models;
using Altafraner.AfraApp.User.Domain.Models;
using Google.OrTools.Sat;

namespace Altafraner.AfraApp.Profundum.Domain.Contracts.Rules;

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
