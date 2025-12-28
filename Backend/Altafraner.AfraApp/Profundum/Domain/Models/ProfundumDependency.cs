namespace Altafraner.AfraApp.Profundum.Domain.Models;

/// <summary>
/// Represents a directed dependency edge between Profunda *definitions*.
/// </summary>
public class ProfundaDefinitionDependency
{
    /// <summary>
    /// The ProfundumDefinition that must be completed *before* the dependant.
    /// </summary>
    public Guid DependencyId { get; set; }

    /// <summary>
    /// The ProfundumDefinition that depends on <see cref="DependencyId"/>.
    /// </summary>
    public Guid DependantId { get; set; }
}
