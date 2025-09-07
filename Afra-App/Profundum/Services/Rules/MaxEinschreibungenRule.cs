using Afra_App.Profundum.Configuration;
using Microsoft.EntityFrameworkCore;
using Afra_App.Profundum.Domain.Models;
using Afra_App.User.Domain.Models;
using Afra_App.User.Services;
using Google.OrTools.Sat;
using Microsoft.Extensions.Options;
using Afra_App.Profundum.Domain.Contracts.Rules;

namespace Afra_App.Profundum.Services.Rules;

///
public class MaxEinschreibungenRule : IProfundumAggregateRule
{
    private readonly UserService _userService;
    private readonly IOptions<ProfundumConfiguration> _profundumConfiguration;
    private readonly AfraAppContext _dbContext;

    ///
    public MaxEinschreibungenRule(UserService userService,
            IOptions<ProfundumConfiguration> profundumConfiguration,
            AfraAppContext dbContext)
    {
        _userService = userService;
        _profundumConfiguration = profundumConfiguration;
        _dbContext = dbContext;
    }

    /// <inheritdoc/>
    public void AddConstraints(
        ProfundumEinwahlZeitraum einwahlZeitraum,
        IEnumerable<Person> students,
        IEnumerable<ProfundumBelegWunsch> wuensche,
        Dictionary<ProfundumBelegWunsch, BoolVar> wuenscheVariables,
        CpModel model)
    {
        var slots = einwahlZeitraum.Slots.ToArray();
        var angebote = _dbContext.ProfundaInstanzen
            .Include(pi => pi.Slots)
            .Include(pi => pi.Profundum)
            .ToArray()
            .Where(pi => pi.Slots.Any(s => slots.Any(sl => sl.Id == s.Id)))
            .ToArray();

        foreach (var a in angebote)
        {
            var angebotWuensche = wuensche.Where(b => b.ProfundumInstanz.Id == a.Id).ToArray();
            var angebotWuenscheVars = angebotWuensche.Select(w => wuenscheVariables[w]);
            if (a.MaxEinschreibungen is int max)
            {
                model.Add(LinearExpr.Sum(angebotWuenscheVars) <= max);
            }
        }
    }
}
