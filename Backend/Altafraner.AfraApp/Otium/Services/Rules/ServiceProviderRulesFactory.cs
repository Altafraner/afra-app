using Altafraner.AfraApp.Otium.Domain.Contracts.Rules;
using Altafraner.AfraApp.Otium.Domain.Contracts.Services;

namespace Altafraner.AfraApp.Otium.Services.Rules;

/// <inheritdoc />
public class ServiceProviderRulesFactory : IRulesFactory
{
    private readonly IEnumerable<IBlockRule> _block;
    private readonly IEnumerable<IIndependentRule> _independent;
    private readonly IEnumerable<IWeekRule> _week;

    ///
    public ServiceProviderRulesFactory(
        IEnumerable<IBlockRule> block,
        IEnumerable<IIndependentRule> independent,
        IEnumerable<IWeekRule> week
    )
    {
        _block = block;
        _independent = independent;
        _week = week;
    }

    /// <inheritdoc />
    public IReadOnlyList<IIndependentRule> GetIndependentRules()
    {
        return _independent.ToList();
    }

    /// <inheritdoc />
    public IReadOnlyList<IBlockRule> GetBlockRules()
    {
        return _block.ToList();
    }

    /// <inheritdoc />
    public IReadOnlyList<IWeekRule> GetWeekRules()
    {
        return _week.ToList();
    }
}
