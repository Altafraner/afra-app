namespace Altafraner.AfraApp.Profundum.Domain.Models;

/// <summary>
/// Represents a directed dependency edge between Profunda.
/// </summary>
public class ProfundaInstanzDependency
{
    /// <summary>
    /// The identifier of the Profunda that is being depended on.
    /// </summary>
    public Guid DependencyId { get; set; }

    /// <summary>
    /// The identifier of the Profunda that depends on <see cref="DependencyId"/>.
    /// </summary>
    public Guid DependantId { get; set; }
}
