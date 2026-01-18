using Altafraner.AfraApp.Otium.Domain.Models;
using Altafraner.AfraApp.Schuljahr.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Altafraner.AfraApp.Otium.Services;

/// <summary>
///     A service for handling categories.
/// </summary>
public class KategorieService
{
    private readonly IMemoryCache _cache;
    private readonly AfraAppContext _dbContext;

    /// <summary>
    ///     Constructor for the KategorieService. Usually called by the DI container.
    /// </summary>
    public KategorieService(AfraAppContext dbContext, IMemoryCache cache)
    {
        _dbContext = dbContext;
        _cache = cache;
    }

    /// <summary>
    ///     Gets a category by its ID.
    /// </summary>
    public async Task<OtiumKategorie?> GetKategorieByIdAsync(Guid id)
    {
        return await _dbContext.OtiaKategorien.FindAsync(id);
    }

    /// <summary>
    ///     Return all required categories.
    /// </summary>
    public async Task<List<Guid>> GetRequiredKategorienIdsAsync(Wochentyp typ)
    {
        return await _cache.GetOrCreateAsync($"otium-kategorie-required-{typ}",
            async _ => await FetchRequiredKategorienAsync(typ)) ?? throw new Exception(
            "Somehow we could neither fetch nor retrieve from cache the required categories. This should never happen.");
    }

    private async Task<List<Guid>> FetchRequiredKategorienAsync(Wochentyp typ)
    {
        return await _dbContext.OtiaKategorien
            .AsNoTracking()
            .Include(k => k.Children)
            .Include(kategorie => kategorie.Parent)
            .Where(k => k.RequiredIn.Contains(typ))
            .Select(k => k.Id)
            .ToListAsync();
    }

    /// <summary>
    ///     Return all categories in a tree as an async enumerable.
    /// </summary>
    public IAsyncEnumerable<OtiumKategorie> GetKategorienTreeAsyncEnumerable()
    {
        return _dbContext.OtiaKategorien.AsNoTracking()
            .Include(k => k.Children)
            .Where(k => k.Parent == null)
            .AsAsyncEnumerable();
    }

    /// <summary>
    ///     Gets a list of all transitive categories of a category.
    /// </summary>
    /// <param name="kategorie">The kategorie to find all parents for.</param>
    /// <returns>An Async Enumerable containing a kategorie and all its parents.</returns>
    public async IAsyncEnumerable<Guid> GetTransitiveKategoriesIdsAsyncEnumerable(OtiumKategorie kategorie)
    {
        // Extra variable needed to avoid null reference exception
        var currentCategory = await _dbContext.OtiaKategorien.FindAsync(kategorie.Id);

        while (currentCategory is not null)
        {
            yield return currentCategory.Id;
            currentCategory = await GetParentAsync(currentCategory);
        }
    }

    /// <summary>
    ///     Traverses the category tree upwards and returns the first required category.
    /// </summary>
    /// <param name="kategorie">The category to get the required parent from</param>
    /// <param name="typ">the type of week this is for</param>
    /// <returns>the first required parent if exists; Otherwise, null.</returns>
    public async Task<Guid?> GetRequiredParentIdAsync(OtiumKategorie kategorie, Wochentyp typ)
    {
        return await _cache.GetOrCreateAsync($"otium-kategorie-required-parent-{typ}-{kategorie.Id}",
            async _ => await FetchRequiredParentAsync(kategorie, typ));
    }

    private async Task<Guid?> FetchRequiredParentAsync(OtiumKategorie kategorie, Wochentyp typ)
    {
        var current = await _dbContext.OtiaKategorien.FindAsync(kategorie.Id);
        while (current is not null)
        {
            if (current.RequiredIn.Contains(typ))
                return current.Id;

            current = await GetParentAsync(current);
        }

        return null;
    }

    private async Task<OtiumKategorie?> GetParentAsync(OtiumKategorie kategorie)
    {
        await _dbContext.Entry(kategorie).Reference(c => c.Parent).LoadAsync();
        return kategorie.Parent;
    }
}
