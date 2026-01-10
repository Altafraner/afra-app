using Altafraner.AfraApp.Profundum.Domain.Contracts.Rules;
using Altafraner.AfraApp.Profundum.Domain.DTO;
using Altafraner.AfraApp.Profundum.Domain.Models;
using Altafraner.AfraApp.User.Domain.Models;
using Google.OrTools.Sat;
using Microsoft.EntityFrameworkCore;

namespace Altafraner.AfraApp.Profundum.Services.Rules;

///
public class DependencyRule : IProfundumIndividualRule
{
    private readonly AfraAppContext _dbContext;

    ///
    public DependencyRule(AfraAppContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <inheritdoc/>
    public RuleStatus CheckForSubmission(Person student,
        IEnumerable<ProfundumSlot> slots,
    IEnumerable<ProfundumEinschreibung> enrollments,
            IEnumerable<ProfundumBelegWunsch> wuensche)
    {
        foreach (var w in wuensche)
        {
            var dep = _dbContext.Profunda.Include(p => p.Dependencies).Where(p => p == w.ProfundumInstanz.Profundum).FirstOrDefault()!.Dependencies;
            var depViol = dep.Where(d => !enrollments.Any(e => e?.ProfundumInstanz?.Profundum == d));
            if (depViol.Any())
            {
                return RuleStatus.Invalid($"{w.ProfundumInstanz.Profundum.Bezeichnung} setzt {depViol.First().Bezeichnung} voraus.");
            }
        }
        return RuleStatus.Valid;
    }

    /// <inheritdoc/>
    public void AddConstraints(Person student,
        IEnumerable<ProfundumSlot> slots,
        IEnumerable<ProfundumBelegWunsch> wuensche,
        Dictionary<(ProfundumSlot, ProfundumInstanz), BoolVar> belegVars,
        Dictionary<ProfundumSlot, BoolVar> personNotEnrolledVars,
        CpModel model,
        LinearExprBuilder objective)
    {
    }

    /// <inheritdoc/>
    public IEnumerable<MatchingWarning> GetWarnings(Person student, IEnumerable<ProfundumSlot> slots, IEnumerable<ProfundumEinschreibung> enrollments)
    {
        foreach (var e in enrollments.Where(e => e.ProfundumInstanz is not null))
        {
            foreach (var d in e.ProfundumInstanz!.Profundum.Dependencies)
            {
                if (!enrollments.Any(x => x.ProfundumInstanz?.Profundum == d))
                {
                    yield return new MatchingWarning($"Vorbedingung {d.Bezeichnung} für {e.ProfundumInstanz.Profundum.Bezeichnung} nicht erfüllt.");
                }
            }
        }
    }
}
