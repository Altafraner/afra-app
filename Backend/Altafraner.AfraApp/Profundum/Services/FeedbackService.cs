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

    public async Task UpdateFeedback(Guid studentId, Guid instanzId, Guid slotId, Dictionary<Guid, int> content)
    {
        var anker = await _ankerService.GetAnker(instanzId);
        var ankerCount = 0;
        var usedCategories = new HashSet<ProfundumFeedbackKategorie>();

        foreach (var currentAnker in anker.Where(currentAnker => content.ContainsKey(currentAnker.Id)))
        {
            ankerCount++;
            usedCategories.Add(currentAnker.Kategorie);
        }

        if (ankerCount != content.Count)
            throw new ArgumentException(
                "At least one of the anchors is duplicated, does not exist, or is not applicable for this profundum",
                nameof(content));

        if (usedCategories.Count < 3)
            throw new ArgumentException(
                "Feedback does not contain anchors from at least three categories",
                nameof(content));

        var enrollment = await _dbContext.ProfundaEinschreibungen.FirstOrDefaultAsync(e =>
            e.ProfundumInstanzId == instanzId && e.BetroffenePersonId == studentId && e.SlotId == slotId);
        if (enrollment is null)
            throw new ArgumentException("Student is not enrolled for this profundum at this time!");

        // Clear feedback
        await _dbContext.ProfundumFeedbackEntries
            .Where(e => e.Einschreibung == enrollment)
            .ExecuteDeleteAsync();

        await _dbContext.ProfundumFeedbackEntries.AddRangeAsync(content.Select(c => new ProfundumFeedbackEntry
        {
            AnkerId = c.Key,
            Einschreibung = enrollment,
            Grad = c.Value
        }));

        await _dbContext.SaveChangesAsync();
    }

    public async
        IAsyncEnumerable<(ProfundumSlot Slot, ProfundumInstanz Instanz, Dictionary<ProfundumFeedbackAnker, int?>
            Feedback)> GetFeedback(Guid studentId, IEnumerable<Guid> slotIds)
    {
        var requestedSlotIds = slotIds.Distinct().ToArray();
        if (requestedSlotIds.Length == 0)
            yield break;

        var enrollments = await _dbContext.ProfundaEinschreibungen
            .Include(e => e.Slot)
            .Include(e => e.ProfundumInstanz)
            .ThenInclude(e => e!.Profundum)
            .Where(e => e.BetroffenePersonId == studentId &&
                        ((IEnumerable<Guid>)requestedSlotIds).Contains(e.SlotId) &&
                        e.ProfundumInstanzId != null)
            .OrderBy(e => e.Slot.Jahr)
            .ThenBy(e => e.Slot.Quartal)
            .ThenBy(e => e.Slot.Wochentag)
            .ThenBy(e => e.SlotId)
            .ToListAsync();

        foreach (var enrollment in enrollments)
        {
            var feedbackByAnker = await GetFeedback(studentId, enrollment.ProfundumInstanzId!.Value, enrollment.SlotId);

            yield return (
                enrollment.Slot,
                enrollment.ProfundumInstanz!,
                feedbackByAnker
            );
        }
    }

    public async Task<Dictionary<ProfundumFeedbackAnker, int?>> GetFeedback(Guid studentId, Guid instanzId, Guid slotId)
    {
        var anker = await _ankerService.GetAnker(instanzId);
        var bewertungen = await _dbContext.ProfundumFeedbackEntries
            .Where(e => e.Einschreibung.ProfundumInstanzId == instanzId &&
                        e.Einschreibung.BetroffenePersonId == studentId && e.Einschreibung.SlotId == slotId)
            .ToArrayAsync();

        return anker.ToDictionary(a => a, a => bewertungen.FirstOrDefault(b => b.AnkerId == a.Id)?.Grad ?? null);
    }

    public async Task<bool> MayProvideFeedbackForProfundumAsync(Person user, Guid instanzId)
    {
        if (user.GlobalPermissions.Contains(GlobalPermission.Profundumsverantwortlich)) return true;

        return await _dbContext.ProfundaInstanzen.Include(e => e.Verantwortliche)
            .AnyAsync(e => e.Id == instanzId && e.Verantwortliche.Contains(user));
    }

    public async IAsyncEnumerable<(ProfundumInstanz instanz, ProfundumSlot slot, FeedbackStatus status)>
        GetFeedbackStatus()
    {
        var occurences = await _dbContext.ProfundaInstanzen
            .Include(e => e.Einschreibungen)
            .ThenInclude(e => e.BetroffenePerson)
            .Where(p => p.MaxEinschreibungen != null && p.MaxEinschreibungen != 0)
            .SelectMany(e => e.Slots.Select(s => new { Instanz = e, Slot = s }))
            .ToListAsync();

        var feedback = await _dbContext.ProfundumFeedbackEntries.Select(e => new
                { e.Einschreibung.BetroffenePersonId, e.Einschreibung.ProfundumInstanzId, e.Einschreibung.SlotId })
            .Distinct()
            .ToArrayAsync();

        foreach (var occurence in occurences)
        {
            var numFeedback = feedback.Count(f =>
                f.ProfundumInstanzId == occurence.Instanz.Id && f.SlotId == occurence.Slot.Id);
            var numStudents = occurence.Instanz.Einschreibungen.Count(e => e.SlotId == occurence.Slot.Id);
            yield return (occurence.Instanz,
                occurence.Slot,
                numFeedback == numStudents ? FeedbackStatus.Done :
                numFeedback != 0 ? FeedbackStatus.Partial : FeedbackStatus.Missing);
        }
    }
}
