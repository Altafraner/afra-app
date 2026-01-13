using Altafraner.AfraApp.Profundum.Domain.DTO;
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
    IEnumerable<ProfundumEinschreibung> enrollments,
    IEnumerable<ProfundumBelegWunsch> wuensche);

    /// <summary>
    ///     Add constraints to matching solver
    /// </summary>
    void AddConstraints(Person student,
        IEnumerable<ProfundumSlot> slots,
        IEnumerable<ProfundumBelegWunsch> wuensche,
        Dictionary<(ProfundumSlot s, ProfundumInstanz i), BoolVar> belegVars,
        Dictionary<ProfundumSlot, BoolVar> personNotEnrolledVars,
        CpModel model,
        LinearExprBuilder objective);

    /// <summary>
    /// Gets warnings for a student
    /// </summary>
    /// <param name="student">The student to get warnings for</param>
    /// <param name="slots">The slots the student can enroll for</param>
    /// <param name="enrollments">The students enrollments</param>
    /// <returns></returns>
    IEnumerable<MatchingWarning> GetWarnings(Person student,
        IEnumerable<ProfundumSlot> slots,
        IEnumerable<ProfundumEinschreibung> enrollments);
}
