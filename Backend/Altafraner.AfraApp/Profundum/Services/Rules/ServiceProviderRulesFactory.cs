using Altafraner.AfraApp.Profundum.Domain.Contracts.Rules;
using Altafraner.AfraApp.Profundum.Domain.Contracts.Services;

namespace Altafraner.AfraApp.Profundum.Services.Rules;

/// <inheritdoc />
public class ServiceProviderRulesFactory : IRulesFactory
{
    private readonly IEnumerable<IProfundumAggregateRule> _aggregate;
    private readonly IEnumerable<IProfundumIndividualRule> _individual;

    ///
    public ServiceProviderRulesFactory(
        IEnumerable<IProfundumAggregateRule> aggregate,
        IEnumerable<IProfundumIndividualRule> individual
    )
    {
        _aggregate = aggregate;
        _individual = individual;
    }

    /// <inheritdoc />
    public IReadOnlyList<IProfundumIndividualRule> GetIndividualRules()
    {
        return _individual.ToList();
    }

    /// <inheritdoc />
    public IReadOnlyList<IProfundumAggregateRule> GetAggregateRules()
    {
        return _aggregate.ToList();
    }
}
