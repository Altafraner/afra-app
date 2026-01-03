using Altafraner.AfraApp.Profundum.Domain.Contracts.Rules;
using Altafraner.AfraApp.Profundum.Domain.Models;
using Altafraner.AfraApp.User.Domain.Models;
using Google.OrTools.Sat;
using Microsoft.EntityFrameworkCore;

namespace Altafraner.AfraApp.Profundum.Services.Rules;

///
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
        Dictionary<ProfundumBelegWunsch, BoolVar> wuenscheVariables,
        CpModel model)
    {
        var wuenscheArray = wuensche as ProfundumBelegWunsch[] ?? wuensche.ToArray();
        var angebote = _dbContext.ProfundaInstanzen
            .Include(pi => pi.Slots)
            .Include(pi => pi.Profundum)
            .ToArray()
            .Where(pi => pi.Slots.Any(s => slots.Any(sl => sl.Id == s.Id)))
            .ToArray();

        foreach (var angebot in angebote)
        {
            var angebotWuensche = wuenscheArray.Where(b => b.ProfundumInstanz.Id == angebot.Id).ToArray();
            var angebotWuenscheVars = angebotWuensche.Select(w => wuenscheVariables[w]);
            if (angebot.MaxEinschreibungen is not { } max) continue;
            model.Add(LinearExpr.Sum(angebotWuenscheVars) <= max);
        }
    }
}
