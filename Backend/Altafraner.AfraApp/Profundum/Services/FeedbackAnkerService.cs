using Altafraner.AfraApp.Profundum.Domain.Models.Bewertung;
using Microsoft.EntityFrameworkCore;

namespace Altafraner.AfraApp.Profundum.Services;

internal class FeedbackAnkerService
{
    private readonly AfraAppContext _dbContext;

    public FeedbackAnkerService(AfraAppContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ProfundumFeedbackAnker> AddAnker(string label, Guid kategorieId)
    {
        var category = await _dbContext.ProfundumFeedbackKategories.FindAsync(kategorieId);
        if (category is null) throw new ArgumentException("Kategorie not found", nameof(kategorieId));

        var entry = await _dbContext.ProfundumFeedbackAnker.AddAsync(new ProfundumFeedbackAnker
        {
            Label = label,
            Kategorie = category
        });

        await _dbContext.SaveChangesAsync();
        return entry.Entity;
    }

    public async Task RemoveAnker(Guid id)
    {
        var entry = await _dbContext.ProfundumFeedbackAnker.FindAsync(id);
        if (entry is null) throw new ArgumentException("Anker not found", nameof(id));
        _dbContext.ProfundumFeedbackAnker.Remove(entry);
        await _dbContext.SaveChangesAsync();
    }

    public async Task UpdateAnker(Guid id, string label, Guid? kategorieId)
    {
        var entry = await _dbContext.ProfundumFeedbackAnker
            .Include(a => a.Kategorie)
            .FirstOrDefaultAsync(a => a.Id == id);
        if (entry is null) throw new ArgumentException("Anker not found", nameof(id));

        if (kategorieId is not null && kategorieId != entry.Kategorie.Id)
        {
            var kategorie = await _dbContext.ProfundumFeedbackKategories.FindAsync(kategorieId) ??
                            throw new ArgumentException("Kategorie not found", nameof(kategorieId));
            entry.Kategorie = kategorie;
        }

        entry.Label = label;

        _dbContext.ProfundumFeedbackAnker.Update(entry);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<Dictionary<ProfundumFeedbackKategorie, List<ProfundumFeedbackAnker>>> GetAnkerByCategories()
    {
        var result = await _dbContext.ProfundumFeedbackAnker
            .Include(e => e.Kategorie)
            .OrderBy(a => a.Label)
            .ThenBy(e => e.Kategorie.Kategorien.Count)
            .ThenBy(e => e.Kategorie.Label)
            .GroupBy(a => a.Kategorie)
            .ToDictionaryAsync(e => e.Key, e => e.ToList());

        return result;
    }

    public async Task<Dictionary<ProfundumFeedbackKategorie, List<ProfundumFeedbackAnker>>> GetAnkerByCategories(
        Guid profundumId)
    {
        var kategorieId = await _dbContext.ProfundaInstanzen.AsNoTracking()
            .Where(e => e.Id == profundumId)
            .Select(e => e.Profundum.Kategorie.Id)
            .FirstOrDefaultAsync();

        if (kategorieId == Guid.Empty) throw new ArgumentException("Profundum not found", nameof(profundumId));

        var result = await _dbContext.ProfundumFeedbackAnker
            .Include(e => e.Kategorie)
            .Where(e => e.Kategorie.Kategorien.Any(k => k.Id == kategorieId))
            .OrderBy(a => a.Label)
            .ThenBy(e => e.Kategorie.Kategorien.Count)
            .ThenBy(e => e.Kategorie.Label)
            .GroupBy(a => a.Kategorie)
            .ToDictionaryAsync(e => e.Key, e => e.ToList());

        return result;
    }
}
