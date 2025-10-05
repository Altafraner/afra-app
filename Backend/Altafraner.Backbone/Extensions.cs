using Altafraner.Backbone.Abstractions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Altafraner.Backbone;

public static class Extensions
{
    public static void UseAltafranerBackbone(this IHostApplicationBuilder builder,
        Action<ModuleBuilder>? configure = null)
    {
        var moduleBuilder = new ModuleBuilder(builder.Services);
        configure?.Invoke(moduleBuilder);

        var sortedModules = moduleBuilder.Catalog.GetOrderedModules();

        var modules = new List<IModule>(sortedModules.Count);
        foreach (var moduleType in sortedModules)
        {
            if (Activator.CreateInstance(moduleType) is not IModule module) continue;
            modules.Add(module);
            module.ConfigureServices(builder.Services, builder.Configuration, builder.Environment);
        }

        builder.Services.AddSingleton<IReadOnlyList<IModule>>(modules);
    }

    public static void AddAltafranerMiddleware(this WebApplication app)
    {
        var modules = app.Services.GetRequiredService<IReadOnlyList<IModule>>();
        foreach (var m in modules) m.BeforeConfigure(app);
    }

    public static void MapAltafranerBackbone(this WebApplication app)
    {
        var modules = app.Services.GetRequiredService<IReadOnlyList<IModule>>();
        foreach (var m in modules) m.Configure(app);
    }

    public static async Task WarmupAltafranerBackbone(this WebApplication app)
    {
        var modules = app.Services.GetRequiredService<IReadOnlyList<IModule>>();
        foreach (var m in modules) await m.InitializeAsync(app);
    }
}
