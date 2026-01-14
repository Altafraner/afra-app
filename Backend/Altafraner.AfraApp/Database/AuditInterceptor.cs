using Altafraner.AfraApp.Backbone.Authorization;
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
        InterceptionResult<int> result)
    {
        ApplyAudit(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    /// <inheritdoc/>
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        ApplyAudit(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private Guid? GetUserId()
    {
        var ctx = _http.HttpContext;
        if (ctx == null || !ctx.User.Identity?.IsAuthenticated == true)
            return null;
        var claim = ctx.User.Claims.FirstOrDefault(c => c.Type == AfraAppClaimTypes.Id);
        return claim == null ? null : Guid.Parse(claim.Value);
    }

    private void ApplyAudit(DbContext? context)
    {
        if (context == null) return;

        Guid? userId = GetUserId();

        var now = DateTime.UtcNow;

        foreach (var entry in context.ChangeTracker.Entries())
        {
            if (entry.Entity is IHasUserTracking audit)
            {
                if (entry.State == EntityState.Added)
                    audit.CreatedById = userId;

                if (entry.State is EntityState.Added or EntityState.Modified)
                    audit.LastModifiedById = userId;
            }
        }
    }
}

