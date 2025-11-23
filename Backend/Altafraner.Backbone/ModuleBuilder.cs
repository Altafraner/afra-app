using Altafraner.Backbone.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Altafraner.Backbone;

/// <summary>
///     A module manager for the altafraner backbone
/// </summary>
public sealed class ModuleBuilder
{
    internal ModuleCatalog Catalog { get; } = new();
    private readonly IServiceCollection _serviceCollection;

    ///
    public ModuleBuilder(IServiceCollection serviceCollection)
    {
        _serviceCollection = serviceCollection;
    }

    /// <summary>
    ///     Adds a module to the altafraner backbone
    /// </summary>
    /// <typeparam name="T">The type of the module</typeparam>
    /// <exception cref="InvalidOperationException">
    ///     You tried to register a module that needs configuration without configuring
    ///     it
    /// </exception>
    public ModuleBuilder AddModule<T>() where T : IModule
    {
        if (typeof(T).IsAssignableTo(typeof(IModule<>)))
            throw new InvalidOperationException(
                "You must add modules that implement IModule<TConfig> with their respective configuration!");
        Catalog.AddModule<T>();
        return this;
    }

    /// <summary>
    ///     Adds a module to the altafraner backbone and configures it
    /// </summary>
    /// <param name="config">The module configuration</param>
    /// <typeparam name="TModule">The module type</typeparam>
    /// <typeparam name="TConfig">The configuration type</typeparam>
    public ModuleBuilder AddModuleAndConfigure<TModule, TConfig>(Action<TConfig>? config = null)
        where TModule : IModule<TConfig>
        where TConfig : class
    {
        _serviceCollection.AddOptions<TConfig>().Configure(config ?? (_ => { }));
        Catalog.AddModule<TModule>();
        return this;
    }
}
