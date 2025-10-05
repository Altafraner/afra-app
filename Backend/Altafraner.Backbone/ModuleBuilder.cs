using Altafraner.Backbone.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Altafraner.Backbone;

public sealed class ModuleBuilder
{
    internal ModuleCatalog Catalog { get; } = new();
    private readonly IServiceCollection _serviceCollection;

    public ModuleBuilder(IServiceCollection serviceCollection)
    {
        _serviceCollection = serviceCollection;
    }

    public ModuleBuilder AddModule<T>() where T : IModule
    {
        if (typeof(T).IsAssignableTo(typeof(IModule<>)))
            throw new InvalidOperationException(
                "You must add modules that implement IModule<TConfig> with their respective configuration!");
        Catalog.AddModule<T>();
        return this;
    }

    public ModuleBuilder AddModuleAndConfigure<TModule, TConfig>(Action<TConfig>? config = null)
        where TModule : IModule<TConfig>
        where TConfig : class
    {
        _serviceCollection.AddOptions<TConfig>().Configure(config ?? (_ => { }));
        Catalog.AddModule<TModule>();
        return this;
    }
}