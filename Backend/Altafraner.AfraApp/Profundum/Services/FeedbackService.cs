using Altafraner.AfraApp.Profundum.Domain.Models.Bewertung;
using Altafraner.AfraApp.User.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Altafraner.AfraApp.Profundum.Services;

internal sealed class FeedbackService
{
    private readonly FeedbackAnkerService _ankerService;
    private readonly AfraAppContext _dbContext;

    public FeedbackService(AfraAppContext dbContext, FeedbackAnkerService ankerService)
    {
        _dbContext = dbContext;
        _ankerService = ankerService;
    }

    public async Task UpdateFeedback(Guid studentId, Guid instanzId, Dictionary<Guid, int> content)
    {
        var anker = await _ankerService.GetAnker(instanzId);
        var ankerCount = anker.Count(a => content.ContainsKey(a.Id));

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
        var anker = await _ankerService.GetAnker(instanzId);
        var bewertungen = await _dbContext.ProfundumFeedbackEntries
            .Where(e => e.InstanzId == instanzId && e.BetroffenePersonId == studentId)
            .ToArrayAsync();

        return anker.ToDictionary(a => a, a => bewertungen.FirstOrDefault(b => b.AnkerId == a.Id)?.Grad ?? null);
    }

    public async Task<bool> MayProvideFeedbackForProfundumAsync(Person user, Guid profundumId)
    {
        if (user.GlobalPermissions.Contains(GlobalPermission.Profundumsverantwortlich)) return true;

        return await _dbContext.ProfundaInstanzen.Include(e => e.Verantwortliche)
            .AnyAsync(e => e.Id == profundumId && e.Verantwortliche.Contains(user));
    }
}
