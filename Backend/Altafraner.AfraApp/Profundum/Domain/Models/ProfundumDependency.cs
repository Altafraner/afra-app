namespace Altafraner.AfraApp.Profundum.Domain.Models;

/// <summary>
/// Represents a directed dependency edge between Profunda.
/// </summary>
public class ProfundaInstanzDependency
{
    public Guid DependencyId { get; set; }

    public Guid DependantId { get; set; }
}
