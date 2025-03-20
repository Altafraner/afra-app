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
    public EnrollmentService(ILogger<EnrollmentService> logger, AfraAppContext context,
        KategorieService kategorieService, IOptions<OtiumConfiguration> configuration)
    {
        _logger = logger;
        _context = context;
        _kategorieService = kategorieService;
        _configuration = configuration.Value;
    }

    /// <summary>
    ///     Enrolls a user in a termin for the subblock starting at a given time.
    /// </summary>
    /// <param name="terminId">The is of the termin entity to enroll to</param>
    /// <param name="student">The student wanting to enroll</param>
    /// <param name="start">The start of the subblock to enroll in</param>
    /// <returns>null, iff the user may not enroll into the termin; Otherwise the Termin entity.</returns>
    public async Task<Termin?> EnrollAsync(Guid terminId, Person student, TimeOnly start)
    {
        var termin = await _context.OtiaTermine
            .Include(termin => termin.Block)
            .ThenInclude(block => block.Schultag)
            .Include(termin => termin.Otium)
            .ThenInclude(otium => otium.Kategorie)
            .Include(termin => termin.Tutor)
            .FirstOrDefaultAsync(t => t.Id == terminId);

        if (termin == null) return null;

        var subBlock = _configuration.Blocks[termin.Block.Nummer].FirstOrDefault(b => b.Interval.Start == start);
        if (subBlock == null) return null;

        var (mayEnroll, _) = await MayEnroll(student, termin, subBlock);
        if (!mayEnroll) return null;

        var enrollments = await _context.OtiaEinschreibungen
            .Where(e => e.Termin == termin && e.BetroffenePerson == student)
            .ToListAsync();

        // this parsing is just dumb...
        var timeline = new Timeline<TimeOnly>(enrollments.Select(e => (ITimeInterval<TimeOnly>)e.Interval));
        timeline.Add(subBlock.Interval);

        var intervals = timeline.GetIntervals();

        for (var i = 0; i < int.Min(enrollments.Count, intervals.Count); i++)
        {
            var interval = intervals[i];
            enrollments[i].Interval = new TimeOnlyInterval(interval.Start, interval.Duration);
        }

        if (intervals.Count == enrollments.Count + 1)
        {
            var interval = intervals[enrollments.Count];

            var einschreibung = new Einschreibung
            {
                Termin = termin,
                BetroffenePerson = student,
                Interval = new TimeOnlyInterval(interval.Start, interval.Duration)
            };

            _context.OtiaEinschreibungen.Add(einschreibung);
            await _context.SaveChangesAsync();
            return termin;
        }

        if (intervals.Count < enrollments.Count)
            _context.OtiaEinschreibungen.RemoveRange(enrollments.Skip(intervals.Count));
        await _context.SaveChangesAsync();

        return termin;
    }

    /// <summary>
    ///     Unenrolls a user from a termin for the subblock starting at a given time.
    /// </summary>
    /// <param name="terminId">the id of the termin entity</param>
    /// <param name="student">the student wanting to enroll</param>
    /// <param name="start">the start time of the subblock</param>
    /// <returns>null, if the user may not enroll with the given parameters; Otherwise the termin the user has enrolled in.</returns>
    public async Task<Termin?> UnenrollAsync(Guid terminId, Person student, TimeOnly start)
    {
        var termin = await _context.OtiaTermine
            .Include(termin => termin.Block)
            .ThenInclude(block => block.Schultag)
            .Include(termin => termin.Otium)
            .ThenInclude(otium => otium.Kategorie)
            .Include(termin => termin.Tutor)
            .FirstOrDefaultAsync(t => t.Id == terminId);

        if (termin == null) return null;

        var subBlock = _configuration.Blocks[termin.Block.Nummer].FirstOrDefault(b => b.Interval.Start == start);
        if (subBlock == null) return null;

        try
        {
            var (mayUnenroll, _) = await MayUnenroll(student, termin, subBlock);
            if (!mayUnenroll) return null;
        }
        catch (InvalidOperationException)
        {
            return null;
        }

        var einschreibungen = (await _context.OtiaEinschreibungen
                .Where(e => e.Termin == termin && e.BetroffenePerson == student)
                .ToListAsync())
            .Where(e => e.Interval.Intersects(subBlock.Interval));


        // Normally, there should be only one einschreibung, but we will handle multiple for safety.
        foreach (var einschreibung in einschreibungen)
        {
            var (before, after) = einschreibung.Interval.Difference(subBlock.Interval);
            if (before is null && after is null)
            {
                _context.OtiaEinschreibungen.Remove(einschreibung);
                await _context.SaveChangesAsync();
                return termin;
            }

            if (before is null && after is not null) (before, after) = (after, null);
            einschreibung.Interval = new TimeOnlyInterval(before!.Start, before.Duration);

            if (after is null) continue;
            await _context.OtiaEinschreibungen.AddAsync(new Einschreibung
            {
                Termin = termin,
                BetroffenePerson = student,
                Interval = new TimeOnlyInterval(after.Start, after.Duration)
            });
        }

        await _context.SaveChangesAsync();
        return termin;
    }

    /// <summary>
    ///     Checks if a set of einschreibungen covers all non-optional blocks of a schultag.
    /// </summary>
    /// <param name="schultag">The day to check for</param>
    /// <param name="einschreibungen">The set of enrollments to aggregate the intervals from</param>
    /// <returns>True, iff all non optional blocks are enrolled</returns>
    public bool AreAllNonOptionalBlocksEnrolled(Schultag schultag, IEnumerable<Einschreibung> einschreibungen)
    {
        var timeline = new Timeline<TimeOnly>();
        foreach (var block in schultag.Blocks)
        foreach (var subBlock in _configuration.Blocks[block.Nummer].Where(b => b.Mandatory))
            timeline.Add(subBlock.Interval);

        foreach (var einschreibung in einschreibungen) timeline.Remove(einschreibung.Interval);

        var allNonOptionalBlocksEnrolled = timeline.GetIntervals().Count == 0;
        return allNonOptionalBlocksEnrolled;
    }

    /// <summary>
    ///     Checks if a set of einschreibungen fulfills the required kategories for each week.
    /// </summary>
    /// <param name="allEinschreibungen">A list of einschreibungen to check if it fulfils the required Kategorien</param>
    /// <returns>
    ///     A <see cref="Dictionary{TKey,TValue}" /> with <see cref="DateTimeInterval" />s representing the weeks as keys
    ///     and a boolean value that is true iff the rule is fulfilled
    /// </returns>
    public async Task<Dictionary<DateTimeInterval, bool>> CheckAllKategoriesInWeeks(
        List<Einschreibung> allEinschreibungen)
    {
        // This is also used to load all kategories so we do not have to lazy load them later. One query is better than many.
        var kategories = await _kategorieService.GetKategorienAsync();
        var requiredKategories = kategories.Where(k => k.Required).ToList();

        var einschreibungenByWeek = new Dictionary<DateTimeInterval, List<Einschreibung>>();
        foreach (var einschreibung in allEinschreibungen)
        {
            var entry = einschreibungenByWeek.FirstOrDefault(e =>
                e.Key.Contains(einschreibung.Termin.Block.Schultag.Datum.ToDateTime(einschreibung.Interval.Start)));
            var key = entry.Key;
            if (entry.Value is null)
            {
                var datum = einschreibung.Termin.Block.Schultag.Datum.ToDateTime(new TimeOnly(0, 0));
                datum = datum.AddDays(-(int)datum.DayOfWeek + 1);
                key = new DateTimeInterval(datum, TimeSpan.FromDays(7));
                einschreibungenByWeek.TryAdd(key, []);
            }

            einschreibungenByWeek[key].Add(einschreibung);
        }

        var resultByWeek = new Dictionary<DateTimeInterval, bool>();
        foreach (var (week, weekEinschreibungen) in einschreibungenByWeek)
        {
            // Check if the user is enrolled in all required kategories
            var localRequiredKategories = requiredKategories.ToList();
            foreach (var einschreibung in weekEinschreibungen)
            {
                var currentKategorie = kategories.First(k => k.Id == einschreibung.Termin.Otium.Kategorie.Id);
                while (currentKategorie != null && localRequiredKategories.Count != 0)
                {
                    if (localRequiredKategories.Any(k => k.Id == currentKategorie.Id))
                        localRequiredKategories.Remove(localRequiredKategories.First(k => k.Id == currentKategorie.Id));
                    currentKategorie = currentKategorie.Parent;
                }
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

        foreach (var subBlock in _configuration.Blocks[termin.Block.Nummer])
        {
            var countEnrolled = terminEinschreibungen.Count(e => e.Interval.Intersects(subBlock.Interval));
            var userEnrolled = (await _context.OtiaEinschreibungen
                    .Where(e => e.Termin == termin && e.BetroffenePerson == user)
                    .ToListAsync())
                .Any(e => e.Interval.Intersects(subBlock.Interval));
            bool mayEdit;
            string? reason;
            if (userEnrolled)
                (mayEdit, reason) = await MayUnenroll(user, termin, subBlock);
            else
                (mayEdit, reason) = await MayEnroll(user, termin, subBlock, countEnrolled);
            yield return new EinschreibungsPreview(countEnrolled, mayEdit, reason, userEnrolled, subBlock.Interval);
        }
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

        var minutesSum = await _context.OtiaEinschreibungen.AsNoTracking()
            .Where(e => e.Termin == termin)
            .SumAsync(e => e.Interval.Duration.TotalMinutes);

        return (int)Math.Round(minutesSum / (75 * (termin.MaxEinschreibungen ?? 1)) * 100);
    }

    private static (bool, string?) CommonMayUnEnroll(Person user, Termin termin, SubBlock subBlock)
    {
        var startDateTime = new DateTime(termin.Block.Schultag.Datum, subBlock.Interval.Start);
        if (startDateTime <= DateTime.Now) return (false, "Der Termin hat bereits begonnen.");

        if (user.Rolle != Rolle.Student) return (false, "Nur Schüler:innen können sich einschreiben.");

        return (true, null);
    }

    private async Task<(bool, string?)> MayUnenroll(Person user, Termin termin, SubBlock subBlock)
    {
        var common = CommonMayUnEnroll(user, termin, subBlock);
        if (!common.Item1) return common;

        var parallelEnrollment = (await _context.OtiaEinschreibungen
                .Include(e => e.Termin)
                .Where(e => e.BetroffenePerson == user && e.Termin.Id == termin.Id)
                .ToListAsync())
            .First(e => e.Interval.Intersects(subBlock.Interval));

        var (before, after) = parallelEnrollment.Interval.Difference(subBlock.Interval);
        var mayUnenroll = (after is null || after.Duration >= TimeSpan.FromMinutes(30)) &&
                          (before is null || before.Duration >= TimeSpan.FromMinutes(30));
        return mayUnenroll
            ? (true, null)
            : (false, "Durch das Austragen entsteht eine Einschreibung kürzer als die Mindestzeit.");
    }

    private async Task<(bool, string?)> MayEnroll(Person user, Termin termin, SubBlock subBlock)
    {
        var countEnrolled = (await _context.OtiaEinschreibungen.AsNoTracking()
                .Where(e => e.Termin == termin)
                .ToListAsync())
            .Count(e => e.Interval.Intersects(subBlock.Interval));

        return await MayEnroll(user, termin, subBlock, countEnrolled);
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
    private async Task<(bool, string?)> MayEnroll(Person user, Termin termin, SubBlock subBlock, int countEnrolled)
    {
        var common = CommonMayUnEnroll(user, termin, subBlock);
        // Check common conditions with unenrollment
        if (!common.Item1) return common;

        // Check if canceled
        if (termin.IstAbgesagt)
            return (false, "Der Termin wurde abgesagt.");

        // Check if the termin is already full
        if (termin.MaxEinschreibungen is not null && countEnrolled >= termin.MaxEinschreibungen)
            return (false, "Der Termin ist bereits vollständig belegt.");

        // Get enrollments for the same block
        var blockEnrollments = await _context.OtiaEinschreibungen
            .Include(e => e.Termin)
            .Where(e => e.BetroffenePerson == user && e.Termin.Block.Id == termin.Block.Id)
            .ToListAsync();

        // Check if the user is enrolled in another termin at the same time
        var parallelEnrollment = blockEnrollments
            .Any(e => e.Termin.Id != termin.Id && e.Interval.Intersects(subBlock.Interval));
        if (parallelEnrollment)
            return (false, "Sie sind bereits zur selben Zeit eingeschrieben");

        // Check if the user is enrolling for a timeframe smaller than 30 minutes
        if (subBlock.Interval.Duration < TimeSpan.FromMinutes(30) && !blockEnrollments
                .Any(e => e.Termin == termin &&
                          e.Interval.IsAdjacent(subBlock.Interval)))
            return (false, "Die Einschreibung überdauert nicht die erforderliche Mindestzeit.");

        // Check if the user is adhering to the required categories
        var lastAvailableBlockRuleFulfilled = await LastAvailableBlockRuleFulfilled(user, termin, subBlock);
        return lastAvailableBlockRuleFulfilled
            ? (true, null)
            : (false,
                "Sie sind nicht in allen erforderlichen Kategorien eingeschrieben. Durch diese Einschreibung wäre das nicht mehr möglich.");
    }

    // Come here for some hideous shit; Optimizing this is a problem for future me.
    // This currently needs three separate SQL-Queries + loading all categories.
    private async Task<bool> LastAvailableBlockRuleFulfilled(Person user, Termin termin, SubBlock subBlock)
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
            .ThenInclude(b => b.Schultag)
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

        var requiredKategories = await _kategorieService.GetKategorienTrackingAsync(true);
        List<Kategorie> notEnrolled = [];
        foreach (var kategorie in requiredKategories)
        {
            if (await _kategorieService.IsKategorieTransitiveInListAsync(kategorie, userKategoriesInWeek)) continue;
            notEnrolled.Add(kategorie);
            break;
        }

        if (notEnrolled.Count == 0)
            return true;

        // Check if the user currently tries to enroll in a still required kategorie
        var transitiveKategories = _kategorieService.GetTransitiveKategoriesAsyncEnumerable(termin.Otium.Kategorie);

        await foreach (var kategorie in transitiveKategories)
            if (notEnrolled.Contains(kategorie))
                return true;

        // Find num all free blocks >= 30 min in the week -> num30MinBlockAvailable
        var blocks = await _context.Blocks
            .Where(b => b.Schultag.Datum >= firstDayOfWeek && b.Schultag.Datum < lastDayOfWeek)
            .Include(b => b.Schultag)
            .ToListAsync();
        var timeline = new Timeline<DateTime>();

        foreach (var block in blocks)
            timeline.Add(_configuration.Blocks[block.Nummer].Skip(1)
                .Aggregate(_configuration.Blocks[block.Nummer].First().Interval,
                    (interval, current) => (TimeOnlyInterval)interval.Union(current.Interval))
                .ToDateTimeInterval(block.Schultag.Datum));
        timeline.Remove(subBlock.Interval.ToDateTimeInterval(termin.Block.Schultag.Datum));
        foreach (var enrollment in usersEnrollmentsInWeek)
            timeline.Remove(enrollment.Interval.ToDateTimeInterval(enrollment.Termin.Block.Schultag.Datum));
        var num30MinBlockAvailable = timeline.GetIntervals().Sum(i => (int)(i.Duration / TimeSpan.FromMinutes(30)));

        return num30MinBlockAvailable >= notEnrolled.Count;
    }
}