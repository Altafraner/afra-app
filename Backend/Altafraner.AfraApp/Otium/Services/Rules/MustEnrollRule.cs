using Altafraner.AfraApp.Domain.TimeInterval;
using Altafraner.AfraApp.Otium.Domain.Contracts.Rules;
using Altafraner.AfraApp.Otium.Domain.Models;
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
    public ValueTask<RuleStatus> IsValidAsync(Person person, Block block,
        IEnumerable<OtiumEinschreibung> einschreibungen)
    {
        var schema = _blockHelper.Get(block.SchemaId)!;
        if (!schema.Verpflichtend) return new ValueTask<RuleStatus>(RuleStatus.Valid);

        var timeline = new Timeline<TimeOnly>();
        foreach (var einschreibung in einschreibungen) timeline.Add(einschreibung.Interval);

        var intervals = timeline.GetIntervals();

        if (intervals.Count == 0)
            return new ValueTask<RuleStatus>(
                RuleStatus.Invalid($"Fehlende Einschreibung für den Block „{schema.Bezeichnung}“"));

        var sum = intervals.Aggregate(TimeSpan.Zero,
            (current, interval) => current + interval.Intersection(schema.Interval)?.Duration ?? TimeSpan.Zero);
        var missing = schema.Interval.Duration - sum;
        if (missing > schema.Interval.Duration * 0.05)
            return new ValueTask<RuleStatus>(
                RuleStatus.Invalid($"Für den Block „{schema.Bezeichnung}“ nicht durchgehend eingeschrieben."));

        return new ValueTask<RuleStatus>(RuleStatus.Valid);
    }
}
