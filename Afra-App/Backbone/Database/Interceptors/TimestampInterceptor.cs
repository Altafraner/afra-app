using Afra_App.Backbone.Database.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Afra_App.Backbone.Database.Interceptors;

/// <summary>
///     An Interceptor saving created and modified times for entities
/// </summary>
public class TimestampInterceptor : SaveChangesInterceptor
{
    /// <inheritdoc/>
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        SetTimestamps(eventData.Context!);
        return base.SavingChanges(eventData, result);
    }

    /// <inheritdoc/>
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        SetTimestamps(eventData.Context!);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }


    private void SetTimestamps(DbContext context)
    {
        if (context == null) return;

        var entries = context.ChangeTracker.Entries<IHasTimestamps>();
        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = DateTime.UtcNow;
            }

            if (entry.State == EntityState.Added || entry.State == EntityState.Modified)
            {
                entry.Entity.LastModified = DateTime.UtcNow;
            }
        }
    }
}
