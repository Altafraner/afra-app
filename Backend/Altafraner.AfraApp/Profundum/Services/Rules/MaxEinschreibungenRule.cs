using Altafraner.AfraApp.Profundum.Domain.Contracts.Rules;
using Altafraner.AfraApp.Profundum.Domain.Models;
using Altafraner.AfraApp.User.Domain.Models;
using Google.OrTools.Sat;
using Microsoft.EntityFrameworkCore;

namespace Altafraner.AfraApp.Profundum.Services.Rules;

/// <summary>
///     only a limited number of students may enroll to the profundum at a time
/// </summary>
public class MaxEinschreibungenRule : IProfundumAggregateRule
{
    private readonly AfraAppContext _dbContext;

    ///
    public MaxEinschreibungenRule(AfraAppContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <inheritdoc/>
    public void AddConstraints(
        IEnumerable<ProfundumSlot> slots,
        IEnumerable<Person> students,
        IEnumerable<ProfundumBelegWunsch> wuensche,
        Dictionary<(Person p, ProfundumSlot s, ProfundumInstanz i), BoolVar> belegVars,
        CpModel model
    )
    {
        var angebote = _dbContext
            .ProfundaInstanzen.Include(pi => pi.Slots)
            .Include(pi => pi.Profundum)
            .ToArray()
            .Where(pi => pi.Slots.Any(s => slots.Any(sl => sl.Id == s.Id)))
            .ToArray();

        foreach (var angebot in angebote.Where(e => e.MaxEinschreibungen.HasValue))
        foreach (var s in angebot.Slots)
        {
            var v = belegVars
                .Where(x => x.Key.i == angebot)
                .Where(x => x.Key.s == s)
                .Select(x => x.Value);
            model.Add(LinearExpr.Sum(v) <= angebot.MaxEinschreibungen!.Value);
        }
    }
}
