using Altafraner.AfraApp.Profundum.Domain.Models;
using Altafraner.AfraApp.User.Domain.Models;
using Google.OrTools.Sat;

namespace Altafraner.AfraApp.Profundum.Domain.Contracts.Rules;

///
public interface IProfundumIndividualRule
{
    ///
    RuleStatus CheckForSubmission(Person student,
    IEnumerable<ProfundumSlot> slots,
    IEnumerable<ProfundumBelegWunsch> wuensche);

    /// <summary>
    ///     Add constraints to matching solver
    /// </summary>
    void AddConstraints(Person student,
        IEnumerable<ProfundumSlot> slots,
        IEnumerable<ProfundumBelegWunsch> wuensche,
        Dictionary<(Person, ProfundumSlot, ProfundumInstanz), BoolVar> belegVars,
        IEnumerable<BoolVar> personNotEnrolledVars,
        CpModel model);
}
