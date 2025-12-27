using Altafraner.AfraApp.Profundum.Domain.Models.Bewertung;
using Altafraner.AfraApp.User.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Altafraner.AfraApp.Profundum.Services;

internal sealed class FeedbackService
{
    private readonly AfraAppContext _dbContext;

    public FeedbackService(AfraAppContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task UpdateFeedback(Guid studentId, Guid instanzId, Dictionary<Guid, int> content)
    {
        // validate
        var profundumKategorieId = await _dbContext.ProfundaInstanzen
            .Where(e => e.Id == instanzId)
            .Select(e => e.Profundum.Kategorie.Id)
            .FirstOrDefaultAsync();
        if (profundumKategorieId == Guid.Empty)
            throw new ArgumentException("There is no profundums instance with the specified id.");

        var keys = content.Keys;
        var ankerCount = await _dbContext.ProfundumFeedbackAnker
            .Include(e => e.Kategorie)
            .ThenInclude(e => e.Kategorien)
            .Where(a => keys.Contains(a.Id) && a.Kategorie.Kategorien.Any(e => e.Id == profundumKategorieId))
            .CountAsync();

        if (ankerCount != content.Count)
            throw new ArgumentException(
                "At least one of the anchors is duplicated, does not exist, or is not applicable for this profundum",
                nameof(content));

        // Clear feedback
        await _dbContext.ProfundumFeedbackEntries
            .Where(e => e.InstanzId == instanzId && e.BetroffenePersonId == studentId)
            .ExecuteDeleteAsync();

        await _dbContext.ProfundumFeedbackEntries.AddRangeAsync(content.Select(c => new ProfundumFeedbackEntry
        {
            AnkerId = c.Key,
            InstanzId = instanzId,
            BetroffenePersonId = studentId,
            Grad = c.Value
        }));

        await _dbContext.SaveChangesAsync();
    }

    public async Task<Dictionary<ProfundumFeedbackAnker, int?>> GetFeedback(Guid studentId, Guid instanzId)
    {
        var profundumKategorieId = await _dbContext.ProfundaInstanzen
            .Where(e => e.Id == instanzId)
            .Select(e => e.Profundum.Kategorie.Id)
            .FirstOrDefaultAsync();

        var anker = await _dbContext.ProfundumFeedbackAnker
            .Where(a => a.Kategorie.Kategorien.Any(e => e.Id == profundumKategorieId))
            .ToArrayAsync();

        var bewertungen = await _dbContext.ProfundumFeedbackEntries
            .Where(e => e.InstanzId == instanzId && e.BetroffenePersonId == studentId)
            .ToArrayAsync();

        return anker.ToDictionary(a => a, a => bewertungen.FirstOrDefault(b => b.AnkerId == a.Id)?.Grad ?? null);
    }

    public async Task<bool> MayProvideFeedbackForProfundumAsync(Person user, Guid profundumId)
    {
        if (user.GlobalPermissions.Contains(GlobalPermission.Profundumsverantwortlich)) return true;

        var managingTutor = (await _dbContext.ProfundaInstanzen.Include(e => e.Tutor)
            .FirstOrDefaultAsync(e => e.Id == profundumId))?.Tutor;

        return managingTutor == user;
    }
}
