using System.Diagnostics.Contracts;
using Altafraner.Backbone.Abstractions;

namespace Altafraner.Backbone;

internal sealed class ModuleCatalog
{
    private readonly Dictionary<Type, HashSet<Type>> _modules = [];

    public void AddModule<T>() where T : IModule
    {
        AddModuleWithDependencies(typeof(T));
    }

    private void AddModuleWithDependencies(Type module)
    {
        if (_modules.ContainsKey(module)) return;

        var dependencies = module.GetCustomAttributes(typeof(DependsOnAttribute), true)
            .Cast<DependsOnAttribute>()
            .Select(a => a.ModuleType)
            .ToHashSet();
        _modules.Add(module, dependencies);

        // Make sure dependencies are also included
        foreach (var dependency in dependencies.Where(dependency => !_modules.ContainsKey(dependency)))
            AddModuleWithDependencies(dependency);
    }

    [Pure]
    public IReadOnlyCollection<Type> GetOrderedModules() => ReverseTopologicalSort(_modules);

    [Pure]
    private static List<T> ReverseTopologicalSort<T>(Dictionary<T, HashSet<T>> dependencyGraph) where T : notnull
    {
        var nodes = dependencyGraph.ToDictionary(d => d.Key, d => d.Value.ToHashSet());
        var result = new List<T>();
        var nodesWithoutIncoming = new Stack<T>(FindNodesWithoutIncomingEdges());

        // While there are still nodes with no incoming edge we have not looked at
        while (nodesWithoutIncoming.Count != 0)
        {
            var currentNode = nodesWithoutIncoming.Pop();
            result.Add(currentNode);

            // For each node m with an incoming edge from the current
            foreach (var dependency in nodes[currentNode])
            {
                nodes[currentNode].Remove(dependency);
                if (!HasIncomingEdges(dependency))
                    nodesWithoutIncoming.Push(dependency);
            }
        }

        // Return module with no dependencies first.
        result.Reverse();

        // If graph has edges, then the graph has at least one cycle.
        return nodes.Any(node => node.Value.Count != 0)
            ? throw new InvalidOperationException("There are dependency cycles in this graph")
            : result;

        IEnumerable<T> FindNodesWithoutIncomingEdges()
        {
            return nodes
                .Where(incoming => !nodes.Values.Any(n => n.Contains(incoming.Key)))
                .Select(n => n.Key);
        }

        bool HasIncomingEdges(T receiving)
        {
            return nodes.Any(incoming => incoming.Value.Contains(receiving));
        }
    }
}
