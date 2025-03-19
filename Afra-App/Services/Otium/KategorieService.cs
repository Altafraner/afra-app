using Afra_App.Data;
using Afra_App.Data.Otium;
using Microsoft.EntityFrameworkCore;

namespace Afra_App.Services.Otium;

/// <summary>
///     A service for handling categories.
/// </summary>
public class KategorieService
{
    private readonly AfraAppContext _context;
    private readonly ILogger _logger;

    /// <summary>
    ///     Constructor for the KategorieService. Usually called by the DI container.
    /// </summary>
    public KategorieService(AfraAppContext context, ILogger<KategorieService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    ///     Return all categories.
    /// </summary>
    public async Task<List<Kategorie>> GetKategorienAsync(bool onlyRequired = false)
    {
        return await GenerateKategorieQuery(onlyRequired)
            .AsNoTracking()
            .ToListAsync();
    }

    /// <summary>
    ///     Return all categories.
    /// </summary>
    public async Task<List<Kategorie>> GetKategorienTrackingAsync(bool onlyRequired = false)
    {
        return await GenerateKategorieQuery(onlyRequired)
            .ToListAsync();
    }

    /// <summary>
    ///     Return all categories as an async enumerable.
    /// </summary>
    public IAsyncEnumerable<Kategorie> GetKategorienAsyncEnumerable(bool onlyRequired = false)
    {
        return GenerateKategorieQuery(onlyRequired)
            .AsNoTracking()
            .AsAsyncEnumerable();
    }

    private IQueryable<Kategorie> GenerateKategorieQuery(bool onlyRequired = false)
    {
        return _context.OtiaKategorien
            .Include(k => k.Children)
            .Include(kategorie => kategorie.Parent)
            .Where(k => !onlyRequired || k.Required);
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
    ///     Checks if a category or any of its parents is in a list of categories.
    /// </summary>
    /// <param name="kategorie">The category to check</param>
    /// <param name="availableKategorien">The list of kategories to check against</param>
    /// <returns></returns>
    public async Task<bool> IsKategorieTransitiveInListAsync(Kategorie kategorie,
        List<Kategorie> availableKategorien)
    {
        ArgumentNullException.ThrowIfNull(availableKategorien);

        var currentCategory = kategorie;
        while (currentCategory is not null)
        {
            if (availableKategorien.Contains(currentCategory))
                return true;
            currentCategory = await GetParentAsync(currentCategory);
        }

        return false;
    }

    /// <summary>
    ///     Gets a list of all transitive categories of a category.
    /// </summary>
    /// <param name="kategorie">The kategorie to find all parents for.</param>
    /// <param name="tracking">Whether the <see cref="Kategorie" /> entry is tracking.</param>
    /// <returns>An Async Enumerable containing a kategorie and all its parents.</returns>
    public async IAsyncEnumerable<Guid> GetTransitiveKategoriesIdsAsyncEnumerable(Kategorie kategorie,
        bool tracking = true)
    {
        var transitiveKategories = GetTransitiveKategoriesAsyncEnumerable(kategorie, tracking);

        await foreach (var transitiveKategory in transitiveKategories) yield return transitiveKategory.Id;
    }

    /// <summary>
    ///     Gets a list of all transitive categories of a category.
    /// </summary>
    /// <param name="kategorie">The kategorie to find all parents for.</param>
    /// <param name="tracking">Whether the <see cref="Kategorie" /> entry is tracking.</param>
    /// <returns>An Async Enumerable containing a kategorie and all its parents.</returns>
    public async IAsyncEnumerable<Kategorie> GetTransitiveKategoriesAsyncEnumerable(Kategorie kategorie,
        bool tracking = true)
    {
        // Extra variable needed to avoid null reference exception
        var currentCategory = tracking ? kategorie : await _context.OtiaKategorien.FindAsync(kategorie.Id);

        while (currentCategory is not null)
        {
            yield return currentCategory;
            currentCategory = await GetParentAsync(currentCategory);
        }
    }

    private async Task<Kategorie?> GetParentAsync(Kategorie kategorie)
    {
        await _context.Entry(kategorie).Reference(c => c.Parent).LoadAsync();
        return kategorie.Parent;
    }
}