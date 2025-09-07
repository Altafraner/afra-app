using System.Diagnostics.Contracts;

namespace Altafraner.AfraApp.Profundum.Domain.Contracts.Rules;

/// <summary>
///     Indicates the status of a rule check.
/// </summary>
public record struct RuleStatus
{
    ///
    public RuleStatus()
    {
    }

    /// <summary>
    ///     True, if the rule is fulfilled.
    /// </summary>
    public required bool IsValid { get; init; }

    /// <summary>
    ///     A complementary message, if the rule is not fulfilled.
    /// </summary>
    public IEnumerable<string> Messages { get; init; } = [];

    /// <summary>
    /// If true, other rules will be ignored. If multiple rules set this flag, the first one wins.
    /// </summary>
    public bool IgnoreOtherRules { get; init; } = false;

    [Pure] internal static RuleStatus Valid => new() { IsValid = true };

    [Pure]
    internal static RuleStatus Invalid(string message)
    {
        return new RuleStatus { IsValid = false, Messages = [message] };
    }
}
