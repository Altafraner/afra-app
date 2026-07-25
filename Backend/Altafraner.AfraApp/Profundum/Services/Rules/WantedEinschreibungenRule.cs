using Altafraner.AfraApp.Profundum.Configuration;
using Altafraner.AfraApp.Profundum.Domain.Contracts.Rules;
using Altafraner.AfraApp.Profundum.Domain.Models;
using Altafraner.AfraApp.User.Domain.Models;
using Google.OrTools.Sat;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Altafraner.AfraApp.Profundum.Services.Rules;

/// <summary>
///     Softly nudges the solver toward an Instanz's organizer-chosen target size
///     (<see cref="ProfundumInstanz.WantedEinschreibungen"/>), if set. Unlike <see cref="MaxEinschreibungenRule"/>'s
///     hard capacity cap, this is a preference, not a physical limit: the penalty weight
///     (<see cref="ProfundumConfiguration.WantedEinschreibungenStrafFaktor"/>) is kept small relative to the wish
///     reward so a course's preferred size only breaks ties between otherwise-similar outcomes and never comes at
///     the expense of a student's ranked wish.
/// </summary>
public class WantedEinschreibungenRule : IProfundumAggregateRule
{
    private readonly IOptions<ProfundumConfiguration> _config;
    private readonly AfraAppContext _dbContext;

    ///
    public WantedEinschreibungenRule(AfraAppContext dbContext, IOptions<ProfundumConfiguration> config)
    {
        _dbContext = dbContext;
        _config = config;
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
        var strafFaktor = _config.Value.WantedEinschreibungenStrafFaktor;
        if (strafFaktor <= 0) return;

        var angebote = _dbContext.ProfundaInstanzen
            .Include(pi => pi.Slots)
            .ToArray()
            .Where(pi => pi.Slots.Any(s => slots.Any(sl => sl.Id == s.Id)))
            .ToArray();

        foreach (var angebot in angebote.Where(e => e.WantedEinschreibungen.HasValue))
        foreach (var s in angebot.Slots)
        {
            var vars = belegVars.Where(x => x.Key.i == angebot)
                .Where(x => x.Key.s == s)
                .Select(x => x.Value)
                .ToArray();
            if (vars.Length == 0) continue;

            var wanted = angebot.WantedEinschreibungen!.Value;
            var count = model.NewIntVar(0, vars.Length, $"wanted-count-{angebot.Id}-{s.Id}");
            model.Add(count == LinearExpr.Sum(vars));

            var deviation = model.NewIntVar(0, Math.Max(vars.Length, wanted),
                $"wanted-deviation-{angebot.Id}-{s.Id}");
            model.Add(deviation >= count - wanted);
            model.Add(deviation >= wanted - count);
            objective.AddTerm(deviation, -strafFaktor);
        }
    }
}
