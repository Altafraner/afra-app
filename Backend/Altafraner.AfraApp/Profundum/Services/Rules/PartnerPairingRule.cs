using Altafraner.AfraApp.Profundum.Domain.Contracts.Rules;
using Altafraner.AfraApp.Profundum.Domain.Models;
using Altafraner.AfraApp.User.Domain.Models;
using Google.OrTools.Sat;
using Microsoft.EntityFrameworkCore;

namespace Altafraner.AfraApp.Profundum.Services.Rules;

/// <summary>
///     Strongly prefers every confirmed team-partner pairing (<see cref="ProfundumPartnerWunsch" />) land on the
///     same Instanz - both students in the same offered section of the paired Definition, or neither, unless
///     honoring that would require sacrificing something else entirely (e.g. a fixed enrollment from an already-
///     finalized period, or a hard rule gate like Profil's Freigabe) - in which case the solver may split the pair
///     as a last resort, and <see cref="ProfundumMatchingService.GetAllEnrollmentsAsync" /> already flags any such
///     drift as a warning. This only synchronizes placement, it never forces either student into the topic: if one
///     partner isn't competitive for (or never ranked) the Definition, every one of their vars for it is 0, which
///     trivially satisfies the preference by pinning the other partner's vars to 0 too. The actual "wants to
///     enroll" signal still comes from each student's own <see cref="ProfundumBelegWunsch" />, same as anyone else.
/// </summary>
public class PartnerPairingRule : IProfundumAggregateRule
{
    /// <summary>
    ///     Weighed clearly heavier than the other soft rules' penalties (typically -5000/-10000) so the solver only
    ///     splits a pair as a genuine last resort, not as a routine trade-off against smaller preferences.
    /// </summary>
    private const int MismatchPenalty = -20000;

    private readonly AfraAppContext _dbContext;

    ///
    public PartnerPairingRule(AfraAppContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <inheritdoc/>
    public void AddConstraints(
        IEnumerable<ProfundumSlot> slots,
        IEnumerable<Person> students,
        IEnumerable<ProfundumBelegWunsch> wuensche,
        Dictionary<(Person p, ProfundumSlot s, ProfundumInstanz i), BoolVar> belegVars,
        CpModel model,
        LinearExprBuilder objective)
    {
        var slotIds = (slots as ProfundumSlot[] ?? slots.ToArray()).Select(s => s.Id).ToHashSet();
        var studentsArray = students as Person[] ?? students.ToArray();

        var pairings = _dbContext.ProfundumPartnerWuensche
            .Include(w => w.EinwahlZeitraum).ThenInclude(z => z.Slots)
            .Where(w => w.EinwahlZeitraum.Slots.Any(s => slotIds.Contains(s.Id)))
            .ToArray();

        foreach (var pairing in pairings)
        {
            var personA = studentsArray.FirstOrDefault(p => p.Id == pairing.PersonAId);
            var personB = studentsArray.FirstOrDefault(p => p.Id == pairing.PersonBId);
            if (personA is null || personB is null)
            {
                continue;
            }

            foreach (var (key, varA) in belegVars.Where(x => x.Key.p == personA && x.Key.i.Profundum.Id == pairing.ProfundumDefinitionId))
            {
                if (belegVars.TryGetValue((personB, key.s, key.i), out var varB))
                {
                    var mismatch = model.NewBoolVar($"pairing-mismatch-{personA.Id}-{personB.Id}-{key.s.Id}-{key.i.Id}");
                    model.Add(varA != varB).OnlyEnforceIf(mismatch);
                    model.Add(varA == varB).OnlyEnforceIf(mismatch.Not());
                    objective.AddTerm(mismatch, MismatchPenalty);
                }
            }
        }
    }
}
