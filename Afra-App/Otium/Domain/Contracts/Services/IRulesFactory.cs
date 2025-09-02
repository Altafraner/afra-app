using Afra_App.Otium.Domain.Contracts.Rules;

namespace Afra_App.Otium.Domain.Contracts.Services;

/// <summary>
///    Factory to get all available rules.
/// </summary>
public interface IRulesFactory
{
    /// <summary>
    /// Gets all independent rules.
    /// </summary>
    IReadOnlyList<IIndependentRule> GetIndependentRules();

    /// <summary>
    /// Gets all block rules.
    /// </summary>
    IReadOnlyList<IBlockRule> GetBlockRules();

    /// <summary>
    /// Gets all week rules.
    /// </summary>
    IReadOnlyList<IWeekRule> GetWeekRules();
}
