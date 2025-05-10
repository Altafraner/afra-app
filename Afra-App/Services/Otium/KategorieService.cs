using Afra_App.Data;
using Afra_App.Data.Otium;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;

namespace Afra_App.Services.Otium;

/// <summary>
///     A service for handling categories.
/// </summary>
public class KategorieService
{
    private readonly HybridCache _cache;
    private readonly AfraAppContext _context;
    private readonly ILogger _logger;

    /// <summary>
    ///     Constructor for the KategorieService. Usually called by the DI container.
    /// </summary>
    public KategorieService(AfraAppContext context, ILogger<KategorieService> logger, HybridCache cache)
    {
        _context = context;
        _logger = logger;
        _cache = cache;
    }

    /// <summary>
    ///     Return all required categories.
    /// </summary>
    public async Task<List<Kategorie>> GetRequiredKategorienAsync()
    {
        return await _context.OtiaKategorien
            .AsNoTracking()
            .Include(k => k.Children)
            .Include(kategorie => kategorie.Parent)
            .Where(k => k.Required)
            .ToListAsync();
    }

    /// <summary>
    ///     Return all categories in a tree as an async enumerable.
    /// </summary>
    public IAsyncEnumerable<Kategorie> GetKategorienTreeAsyncEnumerable()
    {
        return _context.OtiaKategorien.AsNoTracking()
            .Include(k => k.Children)
            .Where(k => k.Parent == null)
            .AsAsyncEnumerable();
    }

    /// <summary>
    ///     Gets a list of all transitive categories of a category.
    /// </summary>
    /// <param name="kategorie">The kategorie to find all parents for.</param>
    /// <returns>An Async Enumerable containing a kategorie and all its parents.</returns>
    public async IAsyncEnumerable<Guid> GetTransitiveKategoriesIdsAsyncEnumerable(Kategorie kategorie)
    {
        // Extra variable needed to avoid null reference exception
        var currentCategory = await _context.OtiaKategorien.FindAsync(kategorie.Id);

        while (currentCategory is not null)
        {
            yield return currentCategory.Id;
            currentCategory = await GetParentAsync(currentCategory);
        }
    }

    /// <summary>
    /// Traverses the category tree upwards and returns the first required category.
    /// </summary>
    /// <param name="kategorie">The category to get the required parent from</param>
    /// <returns>the first required parent if exists; Otherwise, null.</returns>
    // This is handled by the reference.LoadAsync() call
    public async Task<Guid?> GetRequiredParentIdAsync(Kategorie kategorie)
    {
        return await _cache.GetOrCreateAsync($"otium-kategorie-required-parent-{kategorie.Id}",
            async _ => await FetchRequiredParentAsync(kategorie),
            tags: ["otium-kategorie", "otium"]);
    }

    private async Task<Guid?> FetchRequiredParentAsync(Kategorie kategorie)
    {
        var current = await _context.OtiaKategorien.FindAsync(kategorie.Id);
        while (current is not null)
        {
            if (current.Required)
            {
                return current.Id;
            }

            current = await GetParentAsync(current);
        }

        return null;
    }

    private async Task<Kategorie?> GetParentAsync(Kategorie kategorie)
    {
        await _context.Entry(kategorie).Reference(c => c.Parent).LoadAsync();
        return kategorie.Parent;
    }
}
