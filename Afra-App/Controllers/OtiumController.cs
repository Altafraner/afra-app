using Afra_App.Authentication;
using Afra_App.Data;
using Afra_App.Data.Json.Otium;
using Afra_App.Data.Otium;
using Afra_App.Data.People;
using Afra_App.Data.TimeInterval;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Termin = Afra_App.Data.Otium.Termin;

namespace Afra_App.Controllers;

/// <summary>
/// A controller for the Otium API.
/// </summary>
[Route("api/[controller]")]
[Authorize]
[ApiController]
public class OtiumController(AfraAppContext context, ILogger<OtiumController> logger) : ControllerBase
{
    private readonly List<List<SubBlock>> _blocks =
    [
        [
            new SubBlock(new TimeOnly(13, 30), 15, true),
            new SubBlock(new TimeOnly(13, 45), 30, false),
            new SubBlock(new TimeOnly(14, 15), 30, false)
        ],
        [
            new SubBlock(new TimeOnly(15, 00), 30, false),
            new SubBlock(new TimeOnly(15, 30), 15, false),
            new SubBlock(new TimeOnly(15, 45), 30, false)
        ]
    ];

    /// <summary>
    ///     Retrieves the list of categories.
    /// </summary>
    /// <returns>A tree of categories.</returns>
    [Route("kategorie")]
    public IAsyncEnumerable<Kategorie> GetKategorien()
    {
        return context.OtiaKategorien.AsNoTracking()
            .Include(k => k.Children)
            .Where(k => k.Parent == null)
            .AsAsyncEnumerable();
    }

    /// <summary>
    ///     Retrieves the Otium data for a given date and block.
    /// </summary>
    /// <param name="date">The date for which to retrieve the Otium data.</param>
    /// <param name="block">The block number for which to retrieve the Otium data.</param>
    /// <returns>A List of all Otia happening at that time.</returns>
    [HttpGet("{date}/{block:int}")]
    public async IAsyncEnumerable<TerminPreview> GetOtium(DateOnly date, byte block)
    {
        // Get the schultag for the given date and block
        var schultag = await context.Schultage
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Datum == date && s.OtiumsBlock[block]);


        if (schultag == null) yield break;
        logger.LogInformation("Schultag: {datum}", schultag.Datum);

        // Get all termine for the given schultag and block
        // Note: This needs to be materialized before the foreach loop as EF Core does not support multiple active queries.
        // Note: This needs to be a tracking query as we need to load the related entities at a later point.
        var termine = await context.OtiaTermine
            .Where(t => t.Schultag == schultag && t.Block == block)
            .Include(t => t.Otium)
            .ThenInclude(o => o.Kategorie)
            .Include(t => t.Tutor)
            .ToListAsync();

        logger.LogInformation("Termine: {termine}", termine.Count);

        // Calculate the load for each termin and cast it to a json object
        foreach (var termin in termine)
            yield return new TerminPreview(termin, await CalculateLoad(termin), GetOtiaKategories(termin.Otium));
    }


    /// <summary>
    /// Get the details of a termin.
    /// </summary>
    /// <param name="terminId">The <see cref="Guid"/> of the termin to get details for</param>
    /// <returns></returns>
    [HttpGet("{terminId:guid}")]
    public async Task<ActionResult<TerminPreview>> GetTermin(Guid terminId)
    {
        var termin = await context.OtiaTermine
            .Include(termin => termin.Otium)
            .Include(termin => termin.Tutor)
            .Include(termin => termin.Schultag)
            .FirstOrDefaultAsync(t => t.Id == terminId);
        if (termin == null) return NotFound();
        var user = await HttpContext.GetPersonAsync(context);

        var terminJson = new Data.Json.Otium.Termin(termin, GetEinschreibungsPreviews(user, termin), GetOtiaKategories(termin.Otium));

        return Ok(terminJson);
    }

    private async IAsyncEnumerable<EinschreibungsPreview> GetEinschreibungsPreviews(Person user, Termin termin)
    {
        var usersEinschreibungen = await context.OtiaEinschreibungen.AsNoTracking()
            .Include(e => e.Termin)
            .Where(e => e.BetroffenePerson == user && e.Termin.Schultag == termin.Schultag)
            .ToListAsync();
        var terminEinschreibungen = await context.OtiaEinschreibungen.AsNoTracking()
            .Where(e => e.Termin == termin)
            .ToListAsync();

        foreach (var subBlock in _blocks[termin.Block])
        {
            var countEnrolled = terminEinschreibungen.Count(e => e.Interval.Intersects(subBlock.Interval));
            var userEnrolled = (await context.OtiaEinschreibungen
                .Where(e => e.Termin == termin && e.BetroffenePerson == user)
                .ToListAsync())
                .Any(e => e.Interval.Intersects(subBlock.Interval));
            var mayEdit = await MayEditEinschreibungsStatus(user, termin, subBlock, usersEinschreibungen, countEnrolled);
            yield return new EinschreibungsPreview(countEnrolled, mayEdit, userEnrolled, subBlock.Interval);
        }
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
    ///                 The user isn't enrolling in the last available subBlock >= 30 min for a given week, the the Otium
    ///                 does not have the academic category and the user is not enrolled in a academic Otium in the given week
    ///             </item>
    ///         </list>
    ///     </para>
    /// </remarks>
    private async Task<bool> MayEditEinschreibungsStatus(Person user, Termin termin, SubBlock subBlock, IList<Einschreibung> userEinschreibungen, int countEnrolled)
    {
        if (termin.IstAbgesagt)
            return false;
        
        // Check the termin is still in the future
        var startDateTime = new DateTime(termin.Schultag.Datum, subBlock.Interval.Start);
        if (startDateTime <= DateTime.Now)
            return false;

        // Check if the user is already enrolled in another termin at the same time
        var userEnrolled = userEinschreibungen.FirstOrDefault(e => e.Interval.Intersects(subBlock.Interval));
        if (userEnrolled is not null && userEnrolled.Termin.Id == termin.Id)
        {
            // Check unenrollment is possible
            var (before, after) = userEnrolled.Interval.Difference(subBlock.Interval);
            return (after is null || after.Duration >= TimeSpan.FromMinutes(30)) &&
                   (before is null || before.Duration >= TimeSpan.FromMinutes(30));
        }

        if (userEnrolled is not null && userEnrolled.Termin != termin)
            return false;

        // Check whether the termin is full
        if (termin.MaxEinschreibungen is not null && countEnrolled >= termin.MaxEinschreibungen)
            return false;
        
        // Check if the user is enrolling in a timeframe smaller than 30 minutes and no adjacent block is enrolled
        if (subBlock.Interval.Duration < TimeSpan.FromMinutes(30) && !userEinschreibungen
                .Any(e => e.Termin == termin &&
                          e.Interval.IsAdjacent(subBlock.Interval)))
            return false;

        // Calculate first day of week for the given termin.Schultag.Datum
        var lastAvailableBlockRuleFulfilled = await LastAvailableBlockRuleFulfilled(user, termin);
        return lastAvailableBlockRuleFulfilled;
    }

    // Come here for some hideous shit
    private async Task<bool> LastAvailableBlockRuleFulfilled(Person user, Termin termin)
    {
        // Find all required kategories the user is not enrolled to. -> notEnrolled[]
        var firstDayOfWeek = termin.Schultag.Datum.AddDays(-(int)termin.Schultag.Datum.DayOfWeek);
        var lastDayOfWeek = firstDayOfWeek.AddDays(7);
        var weekInterval = new DateTimeInterval(new DateTime(firstDayOfWeek, TimeOnly.MinValue),
            new DateTime(lastDayOfWeek, TimeOnly.MaxValue));

        var usersEnrollmentsInWeek = (await context.OtiaEinschreibungen
                .Where(e => e.BetroffenePerson == user)
                .Include(e => e.Termin)
                .ThenInclude(e => e.Otium)
                .ThenInclude(e => e.Kategorie).Include(einschreibung => einschreibung.Termin)
                .ThenInclude(e => e.Schultag)
                .ToListAsync())
            .Where(e => weekInterval.Contains(new DateTime(e.Termin.Schultag.Datum, TimeOnly.MinValue)))
            .ToList();

        var userKategoriesInWeek = usersEnrollmentsInWeek
            .AsEnumerable()
            .Select(e => e.Termin.Otium.Kategorie)
            .Distinct()
            .ToList();

        var requiredKategories = await context.OtiaKategorien.Where(k => k.Required).ToListAsync();
        List<Kategorie> notEnrolled = [];
        foreach (var kategorie in requiredKategories)
        {
            if (!await IsEnrolledInKategorie(kategorie, userKategoriesInWeek)) continue;
            notEnrolled.Add(kategorie);
            break;
        }

        // Check if the user currently tries to enroll in a still required kategorie
        var currentKategorie = termin.Otium.Kategorie;
        while (currentKategorie != null)
        {
            if (notEnrolled.Contains(currentKategorie))
                return true;
            await context.Entry(currentKategorie).Reference(k => k.Parent).LoadAsync();
            currentKategorie = currentKategorie.Parent;
        }

        // Find num all free blocks >= 30 min in the week -> num30MinBlockAvailable
        var schultage = await context.Schultage
            .Where(s => s.Datum >= firstDayOfWeek && s.Datum < lastDayOfWeek)
            .ToListAsync();
        var timeline = new ReverseTimeline();

        foreach (var schultag in schultage) /* I know this is ugly, but copilot has no better idea too... */
            for (var i = 0; i < _blocks.Count; i++)
                if (schultag.OtiumsBlock[i])
                    timeline.Add(_blocks[i].Skip(1)
                        .Aggregate(_blocks[i].First().Interval,
                            (interval, current) => (TimeOnlyInterval)interval.Union(current.Interval))
                        .ToDateTimeInterval(schultag.Datum));
        foreach (var enrollment in usersEnrollmentsInWeek)
            timeline.Remove(enrollment.Interval.ToDateTimeInterval(enrollment.Termin.Schultag.Datum));
        var num30MinBlockAvailable = timeline.GetIntervals().Sum(i => (int)(i.Duration / TimeSpan.FromMinutes(30)));

        return num30MinBlockAvailable > notEnrolled.Count;
    }

    private async Task<bool> IsEnrolledInKategorie(Kategorie kategorie,
        List<Kategorie> availableKategorien)
    {
        ArgumentNullException.ThrowIfNull(availableKategorien);
        
        var currentCategory = kategorie;
        while (currentCategory is not null)
        {
            if (availableKategorien.Contains(currentCategory))
                return true;
            await context.Entry(currentCategory).Reference(c => c.Parent).LoadAsync();
            currentCategory = currentCategory.Parent;
        }

        return false;
    }

    /// <summary>
    ///     Calculates the load for a given termin.
    /// </summary>
    /// <param name="termin">The termin for which to calculate the load.</param>
    /// <returns>The calculated load as a double, or null if MaxEinschreibungen is null.</returns>
    private async Task<double?> CalculateLoad(Termin termin)
    {
        if (termin.MaxEinschreibungen is null)
            return null;

        var minutesSum = await context.OtiaEinschreibungen.AsNoTracking()
            .Where(e => e.Termin == termin)
            .SumAsync(e => e.Interval.Duration.TotalMinutes);

        return minutesSum / (75 * termin.MaxEinschreibungen);
    }

    private IAsyncEnumerable<Guid> GetOtiaKategories(Otium otium)
    {
        return GetOtiaKategories(otium.Kategorie);
    }

    private async IAsyncEnumerable<Guid> GetOtiaKategories(Kategorie kategorie)
    {
        // Extra variable needed to avoid null reference exception
        var currentCategory = kategorie;

        while (currentCategory is not null)
        {
            yield return currentCategory.Id;
            await context.Entry(currentCategory).Reference(c => c.Parent).LoadAsync();
            currentCategory = currentCategory.Parent;
        }
    }


    private record SubBlock(TimeOnlyInterval Interval, bool Optional)
    {
        public SubBlock(TimeOnly start, TimeOnly end, bool Optional) : this(new TimeOnlyInterval(start, end), Optional)
        {
        }

        public SubBlock(TimeOnly start, int dauerMinuten, bool Optional) : this(start, start.AddMinutes(dauerMinuten),
            Optional)
        {
        }
    }
}