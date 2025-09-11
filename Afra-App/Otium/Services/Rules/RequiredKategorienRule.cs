using Afra_App.Backbone.Domain.TimeInterval;
using Afra_App.Otium.Domain.Contracts.Rules;
using Afra_App.Otium.Domain.Models;
using Afra_App.Schuljahr.Domain.Models;
using Afra_App.User.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Afra_App.Otium.Services.Rules;

/// <summary>
///     Checks that the person is enrolled in all required categories.
/// </summary>
public class RequiredKategorienRule : IWeekRule
{
    private readonly BlockHelper _blockHelper;
    private readonly AfraAppContext _dbContext;
    private readonly KategorieService _kategorieService;
    private List<Guid>? _requiredCategories;

    ///
    public RequiredKategorienRule(BlockHelper blockHelper, AfraAppContext dbContext, KategorieService kategorieService)
    {
        _blockHelper = blockHelper;
        _dbContext = dbContext;
        _kategorieService = kategorieService;
    }

    /// <inheritdoc />
    public async ValueTask<RuleStatus> IsValidAsync(Person person, IEnumerable<Schultag> schultage,
        IEnumerable<OtiumEinschreibung> einschreibungen)
    {
        var fulfilledCategories =
            await GetFulfilledCategoriesFromEnrollments(einschreibungen, schultage);
        var missingCategories = await GetUnfulfilledRequiredCategories(fulfilledCategories);
        var messages = new List<string>();
        foreach (var missingCategory in missingCategories)
            messages.Add(
                $"Fehlende Einschreibungen für die Kategorie „{(await _kategorieService.GetKategorieByIdAsync(missingCategory))!.Bezeichnung}“.");
        return missingCategories.Count == 0
            ? RuleStatus.Valid
            : new RuleStatus
            {
                IsValid = false,
                Messages = messages
            };
    }

    /// <inheritdoc />
    public async ValueTask<RuleStatus> MayEnrollAsync(Person person, IEnumerable<Schultag> schultage,
        IEnumerable<OtiumEinschreibung> einschreibungen, OtiumTermin termin)
    {
        if (termin.Otium.Kategorie.IgnoreEnrollmentRule) return RuleStatus.Valid;

        var tage = schultage as Schultag[] ?? schultage.ToArray();
        var einschreibungsArray = einschreibungen as OtiumEinschreibung[] ?? einschreibungen.ToArray();
        var fulfilledCategories =
            await GetFulfilledCategoriesFromEnrollments(einschreibungsArray, tage);
        _requiredCategories ??= await _kategorieService.GetRequiredKategorienIdsAsync();
        var missingCategories = await GetUnfulfilledRequiredCategories(fulfilledCategories);

        var currentCategoriesRequiredParentId =
            await _kategorieService.GetRequiredParentIdAsync(termin.Otium.Kategorie);

        // The easy part: Check if the rule is fulfilled after this enrollment
        if (missingCategories.Count == 0 ||
            (missingCategories.Count == 1 && missingCategories.First() == currentCategoriesRequiredParentId))
            return RuleStatus.Valid;

        // The hard part: Check if enrolling in this termin would still allow fulfilling all required categories
        var now = DateTime.Now;
        var today = DateOnly.FromDateTime(now);
        var time = TimeOnly.FromDateTime(now);

        var blocksInFutureWithoutEnrollments = tage
            .SelectMany(s => s.Blocks)
            .Where(b => b.SchultagKey > today ||
                        (b.SchultagKey == today && _blockHelper.Get(b.SchemaId)!.Interval.Start > time))
            .Where(b => einschreibungsArray.All(e => e.Termin.Block != b))
            .ToList();

        var angeboteByBlocks = await GetPossibleRequiredCategoriesForBlocks(blocksInFutureWithoutEnrollments);
        foreach (var (_, angebote) in angeboteByBlocks) angebote.ExceptWith(fulfilledCategories);

        var angeboteForCurrentBlock = angeboteByBlocks.GetValueOrDefault(termin.Block.Id, []);
        if (angeboteForCurrentBlock.Count == 0) return RuleStatus.Valid;

        // Remove this so we can backtrack without it.
        angeboteByBlocks.Remove(termin.Block.Id);
        var currentCategoryStillMissing = currentCategoriesRequiredParentId is not null &&
                                          missingCategories.Contains(currentCategoriesRequiredParentId.Value);
        if (currentCategoryStillMissing)
            missingCategories.Remove(currentCategoriesRequiredParentId!.Value);

        var isFulfillableWithEnrollment =
            IsFulfillable(missingCategories.ToHashSet(), angeboteByBlocks.Values.ToList());
        if (isFulfillableWithEnrollment) return RuleStatus.Valid;

        // If we reach this point, the enrollment would make it impossible to fulfill all required categories, but it could still be possible that there is no way whatsoever to fulfill all required categories. In that case, we should allow the enrollment.
        if (currentCategoryStillMissing) missingCategories.Add(currentCategoriesRequiredParentId!.Value);
        angeboteByBlocks.Add(termin.Block.Id, angeboteForCurrentBlock);

        var isFulfillableWithoutEnrollment =
            IsFulfillable(missingCategories.ToHashSet(), angeboteByBlocks.Values.ToList());

        if (isFulfillableWithoutEnrollment)
            return RuleStatus.Invalid(
                "Mit dieser Einschreibung können nicht mehr alle Pflichtkategorien erfüllt werden.");

        // Now we know that there is no way to fulfill all required categories. But there might be a way to fulfill more categories than without this enrollment. We will not check all combinations, but we will check if this enrollment allows to fulfill at least one more category than without this enrollment.
        if (currentCategoryStillMissing) return RuleStatus.Valid;

        return angeboteForCurrentBlock.Count == 0
            ? RuleStatus.Valid
            : RuleStatus.Invalid(
                "Es ist nicht mehr möglich, alle Pflichtkategorien zu erfüllen. Mit einer anderen Einschreibung in diesem Block kannst du aber noch mehr Pflichtkategorien wahrnehmen.");
    }

    private async Task<HashSet<Guid>> GetUnfulfilledRequiredCategories(IEnumerable<Guid> categories)
    {
        _requiredCategories ??= await _kategorieService.GetRequiredKategorienIdsAsync();

        // Clone the required categories to avoid modifying the original list
        var categoriesUnfulfilled = _requiredCategories.ToHashSet();
        foreach (var category in categories)
        {
            categoriesUnfulfilled.Remove(category);

            // Small optimization as this is a very common case
            if (categoriesUnfulfilled.Count == 0) break;
        }

        return categoriesUnfulfilled;
    }

    private async Task<HashSet<Guid>> GetFulfilledCategoriesFromEnrollments(
        IEnumerable<OtiumEinschreibung> einschreibungen, IEnumerable<Schultag> schultage)
    {
        _requiredCategories ??= await _kategorieService.GetRequiredKategorienIdsAsync();

        var timelines = new Dictionary<Guid, Timeline<DateTime>>();
        foreach (var einschreibung in einschreibungen)
        {
            var requiredCategory =
                await _kategorieService.GetRequiredParentIdAsync(einschreibung.Termin.Otium.Kategorie);

            if (requiredCategory is null) continue;
            timelines.TryAdd(requiredCategory.Value, new Timeline<DateTime>());
            timelines[requiredCategory.Value]
                .Add(einschreibung.Interval.ToDateTimeInterval(einschreibung.Termin.Block.SchultagKey));
        }

        var blockIntervals = schultage.SelectMany(s =>
                s.Blocks.Select(b => _blockHelper.Get(b.SchemaId)!.Interval.ToDateTimeInterval(s.Datum)))
            .ToList();

        var fulfilledCategories = new HashSet<Guid>();

        foreach (var (kategorieId, timeline) in timelines)
            if (blockIntervals.Any(b => timeline.Contains(b)))
                fulfilledCategories.Add(_requiredCategories.First(c => c == kategorieId));

        return fulfilledCategories;
    }

    private async Task<Dictionary<Guid, HashSet<Guid>>> GetPossibleRequiredCategoriesForBlocks(
        List<Block> blocks)
    {
        _requiredCategories ??= await _kategorieService.GetRequiredKategorienIdsAsync();

        var result = await _dbContext.OtiaTermine
            .Where(t => blocks.Contains(t.Block))
            .Where(t => t.Enrollments.Count < (t.MaxEinschreibungen ?? int.MaxValue))
            .Select(t => new { BlockId = t.Block.Id, t.Otium.Kategorie })
            .Distinct()
            .GroupBy(g => g.BlockId)
            .ToDictionaryAsync(g => g.Key, g => g.Select(e => e.Kategorie).Distinct().ToList());

        return blocks.ToDictionary(b => b.Id,
            b => result.GetValueOrDefault(b.Id, [])
                .Select(c => c.Id)
                .Where(_requiredCategories.Contains)
                .ToHashSet()
        );
    }

    /// <summary>
    ///     Recursively checks if the missing categories can be fulfilled with the available blocks.
    /// </summary>
    private static bool IsFulfillable(HashSet<Guid> missingCategories, List<HashSet<Guid>> blocks)
    {
        if (blocks.Count == 0) return missingCategories.Count == 0;
        if (missingCategories.Count == 0) return true;

        var currentBlock = blocks[0];
        var remainingBlocks = blocks.Skip(1).ToList();

        var availableCategories = currentBlock.Intersect(missingCategories).ToArray();
        if (availableCategories.Length == 0) return IsFulfillable(missingCategories, remainingBlocks);
        foreach (var availableCategory in availableCategories)
        {
            // Trying to remove side effects so the category is always readded after the recursive call
            var foundSomething = false;
            var wasRemoved = missingCategories.Remove(availableCategory);
            if (IsFulfillable(missingCategories, remainingBlocks)) foundSomething = true;
            if (wasRemoved) missingCategories.Add(availableCategory);
            if (foundSomething) return true;
        }

        return false;
    }
}
