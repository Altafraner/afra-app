using Afra_App.Data;
using Afra_App.Data.Configuration;
using Afra_App.Data.DTO.Otium;
using Afra_App.Data.Otium;
using Afra_App.Data.People;
using Afra_App.Data.Schuljahr;
using Afra_App.Data.TimeInterval;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Einschreibung = Afra_App.Data.Otium.Einschreibung;
using Termin = Afra_App.Data.Otium.Termin;

namespace Afra_App.Services.Otium;

/// <summary>
///     A service for handling enrollments.
/// </summary>
public class EnrollmentService
{
    private readonly OtiumConfiguration _configuration;
    private readonly AfraAppContext _context;
    private readonly KategorieService _kategorieService;
    private readonly ILogger _logger;

    /// <summary>
    ///     Constructs the EnrollmentService. Usually called by the DI container.
    /// </summary>
    public EnrollmentService(AfraAppContext context,
        KategorieService kategorieService, IOptions<OtiumConfiguration> configuration,
        ILogger<EnrollmentService> logger)
    {
        _context = context;
        _kategorieService = kategorieService;
        _configuration = configuration.Value;
        _logger = logger;
    }

    /// <summary>
    ///     Enrolls a user in a termin for the subblock starting at a given time.
    /// </summary>
    /// <param name="terminId">The is of the termin entity to enroll to</param>
    /// <param name="student">The student wanting to enroll</param>
    /// <returns>null, iff the user may not enroll into the termin; Otherwise the Termin entity.</returns>
    public async Task<Termin?> EnrollAsync(Guid terminId, Person student)
    {
        var termin = await _context.OtiaTermine
            .Include(termin => termin.Block)
            .ThenInclude(block => block.Schultag)
            .Include(termin => termin.Otium)
            .ThenInclude(otium => otium.Kategorie)
            .Include(termin => termin.Tutor)
            .FirstOrDefaultAsync(t => t.Id == terminId);

        if (termin == null) return null;

        var (mayEnroll, _) = await MayEnroll(student, termin);
        if (!mayEnroll) return null;

        var einschreibung = new Einschreibung
        {
            Termin = termin,
            BetroffenePerson = student,
        };

        _context.OtiaEinschreibungen.Add(einschreibung);
        await _context.SaveChangesAsync();
        return termin;
    }

    /// <summary>
    ///     Unenrolls a user from a termin for the subblock starting at a given time.
    /// </summary>
    /// <param name="terminId">the id of the termin entity</param>
    /// <param name="student">the student wanting to enroll</param>
    /// <returns>null, if the user may not enroll with the given parameters; Otherwise the termin the user has enrolled in.</returns>
    public async Task<Termin?> UnenrollAsync(Guid terminId, Person student)
    {
        var enrollment = await _context.OtiaEinschreibungen
            .Include(e => e.Termin)
            .ThenInclude(t => t.Block)
            .ThenInclude(b => b.Schultag)
            .FirstOrDefaultAsync(e => e.BetroffenePerson.Id == student.Id && e.Termin.Id == terminId);

        if (enrollment == null) return null;

        try
        {
            var (mayUnenroll, _) = await MayUnenroll(student, enrollment.Termin);
            if (!mayUnenroll) return null;
        }
        catch (InvalidOperationException)
        {
            return null;
        }

        _context.OtiaEinschreibungen.Remove(enrollment);

        await _context.SaveChangesAsync();
        return enrollment.Termin;
    }

    /// <summary>
    /// Gets the times of all non-optional blocks that the student is not enrolled in.
    /// </summary>
    /// <param name="blocks">The blocks to check for</param>
    /// <param name="user">The users to get the times the user is not enrolled in</param>
    /// <returns>A timeline containing all times the user must enroll in but has not done so.</returns>
    public Timeline<TimeOnly> GetNotEnrolledTimes(IEnumerable<Block> blocks, Person user)
    {
        var timeline = new Timeline<TimeOnly>();

        var blockList = blocks.ToList();

        var notEnrolledBlocks = from block in _context.Blocks
            where blockList.Contains(block)
            join einschreibung in _context.OtiaEinschreibungen
                on block.Id equals einschreibung.Termin.Block.Id
                into einschreibungen
            where einschreibungen.All(e => e.BetroffenePerson != user)
            select block.SchemaId;

        foreach (var schemaId in notEnrolledBlocks)
        {
            var schema = _configuration.Blocks.First(b => b.Id == schemaId);
            if (schema.Verpflichtend) timeline.Add(schema.Interval);
        }

        return timeline;
    }

    /// <summary>
    ///     Checks if a set of einschreibungen covers all non-optional blocks of a schultag.
    /// </summary>
    /// <param name="schultag">The day to check for</param>
    /// <param name="einschreibungen">The set of enrollments to aggregate the intervals from</param>
    /// <returns>True, iff all non optional blocks are enrolled</returns>
    public bool AreAllNonOptionalBlocksEnrolled(Schultag schultag, IEnumerable<Einschreibung> einschreibungen)
    {
        var blocksOnSchoolday = schultag.Blocks;
        var blocksEnrolled = einschreibungen
            .Select(e => e.Termin.Block)
            .Where(b => b.Schultag == schultag)
            .Distinct();

        return blocksOnSchoolday.Count == blocksEnrolled.Count();
    }

    /// <summary>
    /// Gets all kategories that are required for a set of enrollments and not included in the categories of enrollments.
    /// </summary>
    /// <param name="enrollments">The enrollments to exclude the (transitive) categories from</param>
    /// <returns>
    ///     An enumerable of all <see cref="Kategorie">Kategorien</see> that are required but covered by the <paramref name="enrollments"/>
    /// </returns>
    public async Task<HashSet<Kategorie>> GetMissingKategories(IEnumerable<Einschreibung> enrollments)
    {
        // Get required categories
        var requiredKategories = (await _kategorieService.GetRequiredKategorienAsync()).ToHashSet();
        foreach (var enrollment in enrollments)
        {
            var requiredParent = await _kategorieService.GetRequiredParentAsync(enrollment.Termin.Otium.Kategorie);
            if (requiredParent != null) requiredKategories.Remove(requiredParent);
        }

        return requiredKategories;
    }

    /// <summary>
    ///     Checks if a set of einschreibungen fulfills the required kategories for each week.
    /// </summary>
    /// <param name="allEinschreibungen">A list of einschreibungen to check if it fulfils the required Kategorien</param>
    /// <returns>
    ///     A <see cref="Dictionary{TKey,TValue}" /> with <see cref="DateTimeInterval" />s representing the weeks as keys
    ///     and a boolean value that is true iff the rule is fulfilled
    /// </returns>
    public async Task<Dictionary<DateOnly, bool>> CheckAllKategoriesInWeeks(
        List<Einschreibung> allEinschreibungen)
    {
        var requiredKategories = (await _kategorieService.GetRequiredKategorienAsync())
            .Select(k => k.Id)
            .ToList();

        var einschreibungenByWeek = new Dictionary<DateOnly, List<Einschreibung>>();
        foreach (var einschreibung in allEinschreibungen)
        {
            var day = einschreibung.Termin.Block.Schultag.Datum;
            var monday = day.AddDays(-(int)day.DayOfWeek + 1);
            einschreibungenByWeek.TryAdd(monday, []);
            einschreibungenByWeek[monday].Add(einschreibung);
        }

        var resultByWeek = new Dictionary<DateOnly, bool>();
        foreach (var (week, weekEinschreibungen) in einschreibungenByWeek)
        {
            // Check if the user is enrolled in all required kategories
            var localRequiredKategories = requiredKategories.ToHashSet();
            foreach (var einschreibung in weekEinschreibungen)
            {
                var requiredParent =
                    await _kategorieService.GetRequiredParentAsync(einschreibung.Termin.Otium.Kategorie);
                if (requiredParent != null) localRequiredKategories.Remove(requiredParent.Id);
            }

            var kategorieRuleFulfilled = localRequiredKategories.Count == 0;
            resultByWeek[week] = kategorieRuleFulfilled;
        }

        return resultByWeek;
    }

    /// <summary>
    ///     Checks if a user may enroll in a termin for all subblocks in a termin.
    /// </summary>
    /// <param name="user">The user to check for</param>
    /// <param name="termin">The termin to check in</param>
    /// <returns></returns>
    public async IAsyncEnumerable<EinschreibungsPreview> GetEnrolmentPreviews(Person user, Termin termin)
    {
        var terminEinschreibungen = await _context.OtiaEinschreibungen.AsNoTracking()
            .Where(e => e.Termin == termin)
            .ToListAsync();

        var schema = _configuration.Blocks.FirstOrDefault(b => b.Id == termin.Block.SchemaId);
        if (schema == null)
        {
            _logger.LogWarning(
                "Schema with id {Id} not found. This should not happen. Please check the configuration.",
                termin.Block.SchemaId);
            yield break;
        }

        var countEnrolled = terminEinschreibungen.Count;
        var usersEnrollment = await _context.OtiaEinschreibungen
            .FirstOrDefaultAsync(e => e.Termin == termin && e.BetroffenePerson == user);
        var (mayEdit, reason) = usersEnrollment != null
            ? await MayUnenroll(user, termin)
            : await MayEnroll(user, termin, countEnrolled);
        yield return new EinschreibungsPreview(countEnrolled, mayEdit, reason, usersEnrollment != null,
            schema.Interval);
    }

    /// <summary>
    ///     Calculates the load for a given termin.
    /// </summary>
    /// <param name="termin">The termin for which to calculate the load.</param>
    /// <returns>The calculated load as a double, or null if MaxEinschreibungen is null.</returns>
    public async Task<int?> GetLoadPercent(Termin termin)
    {
        if (termin.MaxEinschreibungen is null)
            return null;

        var numEnrollments = await _context.OtiaEinschreibungen.AsNoTracking()
            .CountAsync(e => e.Termin == termin);

        return termin.MaxEinschreibungen == null
            ? null
            : (int)Math.Round((double)numEnrollments / termin.MaxEinschreibungen.Value) * 100;
    }

    private (bool MayUnEnroll, string? Reason) CommonMayUnEnroll(Person user, Termin termin)
    {
        var schema = _configuration.Blocks.FirstOrDefault(b => b.Id == termin.Block.SchemaId);

        if (schema == null)
        {
            _logger.LogWarning("A specified block id {id} cannot be found. Denying enrollment.", termin.Block.SchemaId);
            return (false, "Ein interner Fehler ist aufgetreten");
        }

        var startDateTime = new DateTime(termin.Block.Schultag.Datum, schema.Interval.Start);
        if (startDateTime <= DateTime.Now) return (false, "Der Termin hat bereits begonnen.");

        if (user.Rolle != Rolle.Student) return (false, "Nur Schüler:innen können sich einschreiben.");

        return (true, null);
    }

    private Task<(bool MayUnenroll, string? Reason)> MayUnenroll(Person user, Termin termin)
    {
        var common = CommonMayUnEnroll(user, termin);
        if (!common.MayUnEnroll) return Task.FromResult(common);

        return Task.FromResult<(bool MayUnenroll, string? Reason)>((true, null));
    }

    private async Task<(bool, string?)> MayEnroll(Person user, Termin termin)
    {
        var countEnrolled = await _context.OtiaEinschreibungen.AsNoTracking()
            .CountAsync(e => e.Termin == termin);

        return await MayEnroll(user, termin, countEnrolled);
    }

    /// <remarks>
    ///     This is quite an annoying Problem. I try to maintain compatability with changing Block sizes and durations.
    ///     <para>
    ///         It will check for the following conditions:
    ///         <list type="bullet">
    ///             <item>The user is not enrolled in another termin at the same time</item>
    ///             <item>The termin hasn't already started</item>
    ///             <item>The user isn't enrolling in a timeframe smaller than 30 minutes</item>
    ///             <item>
    ///                 The user isn't enrolling in the last available subBlock >= 30 min for a given week, the Otium
    ///                 does not have the academic category and the user is not enrolled in an academic Otium in the given week
    ///             </item>
    ///         </list>
    ///     </para>
    ///     <para>
    ///         If called for different SubBlocks of the same Termin, this will make the same SQL-Query multiple times.
    ///     </para>
    /// </remarks>
    private async Task<(bool MayEnroll, string? Reason)> MayEnroll(Person user, Termin termin,
        int countEnrolled)
    {
        var common = CommonMayUnEnroll(user, termin);
        // Check common conditions with unenrollment
        if (!common.MayUnEnroll) return common;

        // Check if canceled
        if (termin.IstAbgesagt)
            return (false, "Der Termin wurde abgesagt.");

        // Check if the termin is already full
        if (termin.MaxEinschreibungen is not null && countEnrolled >= termin.MaxEinschreibungen)
            return (false, "Der Termin ist bereits vollständig belegt.");

        // Get enrollments for the same block
        var parallelEnrollment = await _context.OtiaEinschreibungen
            .AnyAsync(e => e.BetroffenePerson == user && e.Termin.Block.Id == termin.Block.Id);

        if (parallelEnrollment)
            return (false, "Du bist bereits zur selben Zeit eingeschrieben");

        // Check if the user is adhering to the required categories
        var lastAvailableBlockRuleFulfilled = await LastAvailableBlockRuleFulfilled(user, termin);
        return lastAvailableBlockRuleFulfilled
            ? (true, null)
            : (false,
                "Du bist nicht in allen erforderlichen Kategorien eingeschrieben. Durch diese Einschreibung wäre das nicht mehr möglich.");
    }

    // Come here for some hideous shit; Optimizing this is a problem for future me.
    // This currently needs three separate SQL-Queries + loading all categories.
    private async Task<bool> LastAvailableBlockRuleFulfilled(Person user, Termin termin)
    {
        // Find all required kategories the user is not enrolled to. -> notEnrolled[]
        var firstDayOfWeek = termin.Block.Schultag.Datum.AddDays(-(int)termin.Block.Schultag.Datum.DayOfWeek + 1);
        var lastDayOfWeek = firstDayOfWeek.AddDays(7);
        var weekInterval = new DateTimeInterval(new DateTime(firstDayOfWeek, TimeOnly.MinValue),
            TimeSpan.FromDays(7));

        var usersEnrollmentsInWeek = await _context.OtiaEinschreibungen
            .Where(e => e.BetroffenePerson == user)
            .Include(e => e.Termin)
            .ThenInclude(t => t.Block)
            .Include(e => e.Termin)
            .ThenInclude(e => e.Otium)
            .ThenInclude(e => e.Kategorie)
            .Where(e => DateOnly.FromDateTime(weekInterval.Start) <= e.Termin.Block.Schultag.Datum &&
                        e.Termin.Block.Schultag.Datum < DateOnly.FromDateTime(weekInterval.End))
            .ToListAsync();

        var userKategoriesInWeek = usersEnrollmentsInWeek
            .Select(e => e.Termin.Otium.Kategorie)
            .Distinct()
            .ToList();

        var requiredKategories = (await _kategorieService.GetRequiredKategorienAsync())
            .Select(k => k.Id)
            .ToHashSet();

        // Once we have async linq we can do this with a set minus
        foreach (var cat in userKategoriesInWeek)
        {
            var required = await _kategorieService.GetRequiredParentAsync(cat);
            if (required != null)
                requiredKategories.Remove(required.Id);
        }

        if (requiredKategories.Count == 0)
            return true;

        // I have to do this before the next query as ef core cannot translate it automatically
        var usersBlocks = usersEnrollmentsInWeek.Select(e => e.Termin.Block.Id);

        var blocksAvailable = await _context.Blocks
            .Where(b => b.Schultag.Datum >= firstDayOfWeek && b.Schultag.Datum < lastDayOfWeek)
            .Where(b => !usersBlocks.Contains(b.Id) &&
                        b.Id != termin.Block.Id)
            .Include(b => b.Schultag)
            .ToListAsync();

        var terminsRequiredCategory = await _kategorieService.GetRequiredParentAsync(termin.Otium.Kategorie);
        if (terminsRequiredCategory != null) requiredKategories.Remove(terminsRequiredCategory.Id);

        var blockCategories = new Dictionary<Guid, HashSet<Guid>>();

        var catsByBlock = await _context.OtiaTermine
            .Where(t => blocksAvailable.Contains(t.Block))
            .GroupBy(t => t.Block.Id)
            .Select(e => new { Block = e.Key, Category = e.Select(t => t.Otium.Kategorie).Distinct().ToHashSet() })
            .ToDictionaryAsync(t => t.Block, t => t.Category);

        foreach (var block in blocksAvailable)
        {
            blockCategories.TryAdd(block.Id, []);
            var cats = catsByBlock.TryGetValue(block.Id, out var set) ? set : [];

            foreach (var cat in cats)
            {
                var reqCat = await _kategorieService.GetRequiredParentAsync(cat);
                if (reqCat is not null && requiredKategories.Contains(reqCat.Id))
                    blockCategories[block.Id].Add(reqCat.Id);
            }
        }

        var blockIds = blocksAvailable.Select(b => b.Id).ToArray();

        return Backtrack([]);

        bool Backtrack(HashSet<Guid> catsSelected, int blockIndex = 0)
        {
            if (catsSelected.Count == requiredKategories.Count) return true;
            if (blockIndex >= blockIds.Length) return false;

            foreach (var cat in blockCategories[blockIds[blockIndex]])
            {
                if (!catsSelected.Add(cat)) continue;

                if (Backtrack(catsSelected, blockIndex + 1))
                    return true;
                catsSelected.Remove(cat);
            }

            // Also try to not select a required category from this block. I'm unsure if this is a valid option.
            return Backtrack(catsSelected, blockIndex + 1);
        }
    }
}
