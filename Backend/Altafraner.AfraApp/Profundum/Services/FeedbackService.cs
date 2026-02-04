using Altafraner.AfraApp.Profundum.Domain.DTO;
using Altafraner.AfraApp.Profundum.Domain.Models;
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
        var ankerCount = 0;
        var usedCategories = new HashSet<ProfundumFeedbackKategorie>();

        foreach (
            var currentAnker in anker.Where(currentAnker => content.ContainsKey(currentAnker.Id))
        )
        {
            ankerCount++;
            usedCategories.Add(currentAnker.Kategorie);
        }

        if (ankerCount != content.Count)
            throw new ArgumentException(
                "At least one of the anchors is duplicated, does not exist, or is not applicable for this profundum",
                nameof(content)
            );

        if (usedCategories.Count < 3)
            throw new ArgumentException(
                "Feedback does not contain anchors from at least three categories",
                nameof(content)
            );

        // Clear feedback
        await _dbContext
            .ProfundumFeedbackEntries.Where(e =>
                e.InstanzId == instanzId && e.BetroffenePersonId == studentId
            )
            .ExecuteDeleteAsync();

        await _dbContext.ProfundumFeedbackEntries.AddRangeAsync(
            content.Select(c => new ProfundumFeedbackEntry
            {
                AnkerId = c.Key,
                InstanzId = instanzId,
                BetroffenePersonId = studentId,
                Grad = c.Value,
            })
        );

        await _dbContext.SaveChangesAsync();
    }

    public async Task<Dictionary<ProfundumFeedbackAnker, int?>> GetFeedback(
        Guid studentId,
        Guid instanzId
    )
    {
        var anker = await _ankerService.GetAnker(instanzId);
        var bewertungen = await _dbContext
            .ProfundumFeedbackEntries.Where(e =>
                e.InstanzId == instanzId && e.BetroffenePersonId == studentId
            )
            .ToArrayAsync();

        return anker.ToDictionary(
            a => a,
            a => bewertungen.FirstOrDefault(b => b.AnkerId == a.Id)?.Grad ?? null
        );
    }

    public async Task<bool> MayProvideFeedbackForProfundumAsync(Person user, Guid profundumId)
    {
        if (user.GlobalPermissions.Contains(GlobalPermission.Profundumsverantwortlich))
            return true;

        return await _dbContext
            .ProfundaInstanzen.Include(e => e.Verantwortliche)
            .AnyAsync(e => e.Id == profundumId && e.Verantwortliche.Contains(user));
    }

    public async IAsyncEnumerable<(
        ProfundumInstanz instanz,
        FeedbackStatus status
    )> GetFeedbackStatus()
    {
        var instances = await _dbContext
            .ProfundaInstanzen.Include(e => e.Einschreibungen)
                .ThenInclude(e => e.BetroffenePerson)
            .Where(p => p.MaxEinschreibungen != null && p.MaxEinschreibungen != 0)
            .ToListAsync();

        var feedback = await _dbContext
            .ProfundumFeedbackEntries.Select(e => new { e.BetroffenePersonId, e.InstanzId })
            .Distinct()
            .ToArrayAsync();

        foreach (var instance in instances)
        {
            var numFeedback = feedback.Count(f => f.InstanzId == instance.Id);
            var numStudents = instance
                .Einschreibungen.DistinctBy(e => (e.BetroffenePersonId, e.ProfundumInstanzId))
                .Count();
            yield return (
                instance,
                numFeedback == numStudents ? FeedbackStatus.Done
                : numFeedback != 0 ? FeedbackStatus.Partial
                : FeedbackStatus.Missing
            );
        }
    }
}
