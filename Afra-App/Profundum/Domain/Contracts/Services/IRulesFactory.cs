using Afra_App.Profundum.Domain.Contracts.Rules;

namespace Afra_App.Profundum.Domain.Contracts.Services;

/// <summary>
///    Factory to get all available rules.
/// </summary>
public interface IRulesFactory
{
    ///
    IReadOnlyList<IProfundumIndividualRule> GetIndividualRules();

    ///
    IReadOnlyList<IProfundumAggregateRule> GetAggregateRules();
}
