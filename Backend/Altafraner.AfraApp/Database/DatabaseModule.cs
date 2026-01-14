using System.ComponentModel.DataAnnotations;
using Altafraner.Backbone.Abstractions;
using Altafraner.Backbone.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Altafraner.AfraApp;

/// <summary>
/// A module to configure the database
/// </summary>
public class DatabaseModule : IModule
{
    /// <inheritdoc />
    public void ConfigureServices(IServiceCollection services, IConfiguration config, IHostEnvironment env)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<AuditInterceptor>();
        services.AddScoped<TimestampInterceptor>();
        services.AddDbContext<AfraAppContext>((sp, options) =>
        {
            options.AddInterceptors(
                sp.GetRequiredService<TimestampInterceptor>(),
                sp.GetRequiredService<AuditInterceptor>()
            );
            options.UseNpgsql(config.GetConnectionString("DefaultConnection"),
                AfraAppContext.ConfigureNpgsql);
            options.ConfigureWarnings(w => w.Throw(RelationalEventId.MultipleCollectionIncludeWarning));
        });
    }

    /// <inheritdoc />
    public async Task InitializeAsync(WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        await using var context = scope.ServiceProvider.GetService<AfraAppContext>()!;

        if (!(await context.Database.GetPendingMigrationsAsync()).Any()) return;
        if (!app.Configuration.GetValue<bool>("MigrateOnStartup"))
        {
            throw new ValidationException("The database is not up to date. Please run the migrations.");
        }

        app.Logger.LogInformation("Migrating database");
        await context.Database.MigrateAsync();
        app.Logger.LogInformation("Database migrated");
    }
}
