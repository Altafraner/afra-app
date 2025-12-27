using Altafraner.AfraApp.Profundum.Domain.Models.Bewertung;
using Microsoft.EntityFrameworkCore;

namespace Altafraner.AfraApp.Profundum.Services;

internal class FeedbackKategorienService
{
    private readonly AfraAppContext _dbContext;

    public FeedbackKategorienService(AfraAppContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ProfundumFeedbackKategorie> AddKategorie(string label, IEnumerable<Guid> kategorieIds)
    {
        var categories = await _dbContext.ProfundaKategorien
            .Where(k => kategorieIds.Contains(k.Id))
            .ToListAsync();

        if (kategorieIds.Count() != categories.Count)
            throw new ArgumentException("At least one of the specified kategorieIds does not exist",
                nameof(kategorieIds));

        var entry = await _dbContext.ProfundumFeedbackKategories.AddAsync(new ProfundumFeedbackKategorie
        {
            Label = label,
            Kategorien = categories
        });

        await _dbContext.SaveChangesAsync();

        return entry.Entity;
    }

    public async Task RemoveKategorie(Guid id)
    {
        var entry = await _dbContext.ProfundumFeedbackKategories.FindAsync(id);
        if (entry is null) throw new ArgumentException("Kategorie not found", nameof(id));
        _dbContext.ProfundumFeedbackKategories.Remove(entry);
        await _dbContext.SaveChangesAsync();
    }

    public async Task UpdateKategorie(Guid id, string label, IEnumerable<Guid> kategorieIds)
    {
        var entry = await _dbContext.ProfundumFeedbackKategories
            .Include(e => e.Kategorien)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (entry is null) throw new ArgumentException("Kategorie not found", nameof(id));

        var categories = await _dbContext.ProfundaKategorien
            .Where(k => kategorieIds.Contains(k.Id))
            .ToListAsync();

        if (kategorieIds.Count() != categories.Count)
            throw new ArgumentException("At least one of the specified kategorieIds does not exist",
                nameof(kategorieIds));

        entry.Label = label;
        entry.Kategorien = categories;

        _dbContext.ProfundumFeedbackKategories.Update(entry);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<IEnumerable<ProfundumFeedbackKategorie>> GetAllCategories()
    {
        return await _dbContext.ProfundumFeedbackKategories
            .Include(e => e.Kategorien)
            .OrderBy(e => e.Kategorien.Count)
            .ThenBy(e => e.Label)
            .ToListAsync();
    }
}
