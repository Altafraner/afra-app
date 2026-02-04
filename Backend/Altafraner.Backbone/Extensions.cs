using Altafraner.Backbone.Abstractions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Altafraner.Backbone;

/// <summary>
///     Extension methods for working with the Altafraner Backbone
/// </summary>
public static class Extensions
{
    /// <summary>
    ///     Add the altafraner backbone to the project
    /// </summary>
    public static void UseAltafranerBackbone(
        this IHostApplicationBuilder builder,
        Action<ModuleBuilder>? configure = null
    )
    {
        var moduleBuilder = new ModuleBuilder(builder.Services);
        configure?.Invoke(moduleBuilder);

        var sortedModules = moduleBuilder.Catalog.GetOrderedModules();

        var modules = new List<IModule>(sortedModules.Count);
        foreach (var moduleType in sortedModules)
        {
            if (Activator.CreateInstance(moduleType) is not IModule module)
                continue;
            modules.Add(module);
            module.ConfigureServices(builder.Services, builder.Configuration, builder.Environment);
        }

        builder.Services.AddSingleton<IReadOnlyList<IModule>>(modules);
    }

    /// <summary>
    ///     Register the middleware from the altafraner backbone. This should be called before
    ///     <see cref="MapAltafranerBackbone" />
    /// </summary>
    public static void AddAltafranerMiddleware(this WebApplication app)
    {
        var modules = app.Services.GetRequiredService<IReadOnlyList<IModule>>();
        foreach (var m in modules)
            m.RegisterMiddleware(app);
    }

    /// <summary>
    ///     Map the endpoints from the altafraner backbone
    /// </summary>
    public static void MapAltafranerBackbone(this WebApplication app)
    {
        var modules = app.Services.GetRequiredService<IReadOnlyList<IModule>>();
        foreach (var m in modules)
            m.Configure(app);
    }

    /// <summary>
    ///     Initializes all components registered with the altafraner backbone
    /// </summary>
    /// <param name="app"></param>
    public static async Task WarmupAltafranerBackbone(this WebApplication app)
    {
        var modules = app.Services.GetRequiredService<IReadOnlyList<IModule>>();
        foreach (var m in modules)
            await m.InitializeAsync(app);
    }
}
