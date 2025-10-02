using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Altafraner.AfraApp.Backbone.Extensions;

/// <summary>
/// A static class that provides extension methods for the <see cref="WebApplication"/> class.
/// </summary>
public static class RouteBuilderExtension
{
    /// <summary>
    /// Adds the backbone middleware to the application pipeline.
    /// </summary>
    public static void UseBackbone(this WebApplication app)
    {
        app.UseAuthentication();
        app.UseAuthorization();
        using var scope = app.Services.CreateScope();
        using var context = scope.ServiceProvider.GetService<AfraAppContext>()!;

        if (!context.Database.GetPendingMigrations().Any()) return;
        if (!app.Configuration.GetValue<bool>("MigrateOnStartup"))
        {
            throw new ValidationException("The database is not up to date. Please run the migrations.");
        }

        app.Logger.LogInformation("Migrating database");
        context.Database.Migrate();
        app.Logger.LogInformation("Database migrated");
    }
}
