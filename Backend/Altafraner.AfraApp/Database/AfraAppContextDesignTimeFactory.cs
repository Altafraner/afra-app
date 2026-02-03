using Altafraner.Backbone;
using Microsoft.EntityFrameworkCore.Design;

// ReSharper disable UnusedType.Global

namespace Altafraner.AfraApp;

/// <summary>
///     This is a factory used for design time creation of the AfraAppContext. This will be used by the ef core design
///     tools.
/// </summary>
public class AfraAppContextDesignTimeFactory : IDesignTimeDbContextFactory<AfraAppContext>
{
    /// <inheritdoc />
    public AfraAppContext CreateDbContext(string[] args)
    {
        // We'll create the service here exactly the way we would in the normal application.
        var builder = WebApplication.CreateSlimBuilder(new WebApplicationOptions { Args = args });
        builder.UseAltafranerBackbone(backbone => backbone.AddModule<DatabaseModule>());
        var app = builder.Build();
        var scope = app.Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<AfraAppContext>();
    }
}
