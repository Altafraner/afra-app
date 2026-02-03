using Altafraner.AfraApp.User.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Altafraner.AfraApp;

/// <summary>
///     An Interceptor saving users who created or modified a tuple
/// </summary>
public class AuditInterceptor : SaveChangesInterceptor
{
    private readonly IHttpContextAccessor _http;

    ///
    public AuditInterceptor(IHttpContextAccessor http)
    {
        _http = http;
    }

    /// <inheritdoc/>
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result
    )
    {
        ApplyAudit(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    /// <inheritdoc/>
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default
    )
    {
        ApplyAudit(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private Guid? GetUserId()
    {
        var ctx = _http.HttpContext;
        if (ctx is null)
            return null;
        Guid? userId;
        try
        {
            userId = UserAccessor.GetUserIdOrThrow(ctx);
        }
        catch (InvalidOperationException)
        {
            userId = null;
        }
        return userId;
    }

    private void ApplyAudit(DbContext? context)
    {
        if (context == null)
            return;
        var userId = GetUserId();

        foreach (var entry in context.ChangeTracker.Entries())
        {
            if (entry.Entity is not IHasUserTracking audit)
                continue;

            if (entry.State == EntityState.Added)
                audit.CreatedById = userId;
            if (entry.State is EntityState.Added or EntityState.Modified)
                audit.LastModifiedById = userId;
        }
    }
}
