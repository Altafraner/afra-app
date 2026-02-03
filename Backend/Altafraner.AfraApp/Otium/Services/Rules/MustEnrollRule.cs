using Altafraner.AfraApp.Otium.Domain.Contracts.Rules;
using Altafraner.AfraApp.Otium.Domain.Models;
using Altafraner.AfraApp.Otium.Domain.Models.TimeInterval;
using Altafraner.AfraApp.Schuljahr.Domain.Models;
using Altafraner.AfraApp.User.Domain.Models;

namespace Altafraner.AfraApp.Otium.Services.Rules;

/// <summary>
///     Checks that the person is enrolled for a mandatory block.
/// </summary>
public class MustEnrollRule : IBlockRule
{
    private readonly BlockHelper _blockHelper;

    ///
    public MustEnrollRule(BlockHelper blockHelper)
    {
        _blockHelper = blockHelper;
    }

    /// <inheritdoc />
    public ValueTask<RuleStatus> IsValidAsync(
        Person person,
        Block block,
        IEnumerable<OtiumEinschreibung> einschreibungen
    )
    {
        var schema = _blockHelper.Get(block.SchemaId)!;
        if (!schema.Verpflichtend)
            return new ValueTask<RuleStatus>(RuleStatus.Valid);

        var timeline = new Timeline<TimeOnly>();
        foreach (var einschreibung in einschreibungen)
            timeline.Add(einschreibung.Interval);

        var intervals = timeline.GetIntervals();

        switch (intervals.Count)
        {
            case 0:
                return new ValueTask<RuleStatus>(
                    RuleStatus.Invalid(
                        $"Fehlende Einschreibung für den Block „{schema.Bezeichnung}“"
                    )
                );
            case 1 when !intervals[0].Contains(schema.Interval):
            case > 1:
                return new ValueTask<RuleStatus>(
                    RuleStatus.Invalid(
                        $"Für den Block „{schema.Bezeichnung}“ nicht durchgehend eingeschrieben."
                    )
                );
            default:
                return new ValueTask<RuleStatus>(RuleStatus.Valid);
        }
    }
}
