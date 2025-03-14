using System.Collections.Concurrent;
using Afra_App.Authentication;
using Afra_App.Data;
using Afra_App.Data.DTO;
using Afra_App.Data.DTO.Otium;
using Afra_App.Data.Otium;
using Afra_App.Data.People;
using Afra_App.Data.Schuljahr;
using Afra_App.Data.TimeInterval;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Einschreibung = Afra_App.Data.Otium.Einschreibung;
using Person = Afra_App.Data.People.Person;
using Termin = Afra_App.Data.Otium.Termin;

namespace Afra_App.Controllers;

/// <summary>
///     A controller for the Otium API.
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
            .OrderBy(t => t.IstAbgesagt)
            .ThenBy(t => t.Otium.Bezeichnung)
            .ToListAsync();

        logger.LogInformation("Termine: {termine}", termine.Count);

        // Calculate the load for each termin and cast it to a json object
        foreach (var termin in termine)
            yield return new TerminPreview(termin, await CalculateLoad(termin), GetOtiaKategories(termin.Otium));
    }


    /// <summary>
    ///     Get the details of a termin.
    /// </summary>
    /// <param name="terminId">The <see cref="Guid" /> of the termin to get details for</param>
    /// <returns></returns>
    [HttpGet("{terminId:guid}")]
    public async Task<ActionResult<Data.DTO.Otium.Termin>> GetTermin(Guid terminId)
    {
        var termin = await context.OtiaTermine
            .Include(termin => termin.Tutor)
            .Include(termin => termin.Otium)
            .ThenInclude(otium => otium.Kategorie)
            .Include(termin => termin.Schultag)
            .FirstOrDefaultAsync(t => t.Id == terminId);
        if (termin == null) return NotFound();
        var user = await HttpContext.GetPersonAsync(context);

        var terminJson = GetTerminPreview(termin, user);

        return Ok(terminJson);
    }

    private Data.DTO.Otium.Termin GetTerminPreview(Termin termin, Person user)
    {
        return new Data.DTO.Otium.Termin(termin, GetEinschreibungsPreviews(user, termin),
            GetOtiaKategories(termin.Otium), _blocks[termin.Block].First().Interval.Start);
    }

    /// <summary>
    /// An API-Endpoint to enroll in a termin.
    /// </summary>
    /// <param name="terminId">The id of the termin</param>
    /// <param name="start">the start time of the subblock to enroll in</param>
    /// <returns><see cref="NotFoundResult"/> if the Termin does not exist, <see cref="BadRequestResult"/> if the subBlock does not exist or the user may not enroll to the termin; Otherwise <see cref="OkObjectResult"/></returns>
    /// <seealso cref="MayEnroll(Afra_App.Data.People.Person,Termin,SubBlock)"/>
    [HttpPut("{terminId:guid}/{start}")]
    public async Task<ActionResult> Enroll(Guid terminId, TimeOnly start)
    {
        var termin = await context.OtiaTermine
            .Include(termin => termin.Schultag)
            .Include(termin => termin.Otium)
            .ThenInclude(otium => otium.Kategorie)
            .Include(termin => termin.Tutor)
            .FirstOrDefaultAsync(t => t.Id == terminId);
        
        if (termin == null) return NotFound();
        var user = await HttpContext.GetPersonAsync(context);

        var subBlock = _blocks[termin.Block].FirstOrDefault(b => b.Interval.Start == start);
        if (subBlock == null) return BadRequest();
        
        var (mayEnroll, reason) = await MayEnroll(user, termin, subBlock);
        if (!mayEnroll) return BadRequest(reason);
        
        var enrollments = await context.OtiaEinschreibungen
            .Where(e => e.Termin == termin && e.BetroffenePerson == user)
            .ToListAsync();

        // this parsing is just dumb...
        var timeline = new Timeline<TimeOnly>(enrollments.Select(e => (ITimeInterval<TimeOnly>) e.Interval));
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
                BetroffenePerson = user,
                Interval = new TimeOnlyInterval(interval.Start, interval.Duration)
            };

            context.OtiaEinschreibungen.Add(einschreibung);
            await context.SaveChangesAsync();
            return Ok(GetTerminPreview(termin, user));
        }

        if (intervals.Count < enrollments.Count) context.OtiaEinschreibungen.RemoveRange(enrollments.Skip(intervals.Count));
        await context.SaveChangesAsync();

        return Ok(GetTerminPreview(termin, user));
    }

    /// <summary>
    /// An API-Endpoint to unenroll in a termin.
    /// </summary>
    /// <param name="terminId">The id of the termin</param>
    /// <param name="start">the start time of the subblock to unenroll of</param>
    /// <returns><see cref="NotFoundResult"/> if the Termin does not exist, <see cref="BadRequestResult"/> if the subBlock does not exist or the user may not unenroll from the termin; Otherwise <see cref="OkObjectResult"/></returns>
    /// <seealso cref="MayEnroll(Afra_App.Data.People.Person,Termin,SubBlock)"/>
    [HttpDelete("{terminId:guid}/{start}")]
    public async Task<ActionResult> Unenroll(Guid terminId, TimeOnly start)
    {
        var termin = await context.OtiaTermine
            .Include(termin => termin.Schultag)
            .Include(termin => termin.Otium)
            .ThenInclude(otium => otium.Kategorie)
            .Include(termin => termin.Tutor)
            .FirstOrDefaultAsync(t => t.Id == terminId);
        
        if (termin == null) return NotFound();
        var user = await HttpContext.GetPersonAsync(context);

        var subBlock = _blocks[termin.Block].FirstOrDefault(b => b.Interval.Start == start);
        if (subBlock == null) return BadRequest();

        try
        {
            var (mayUnenroll, reason) = await MayUnenroll(user, termin, subBlock);
            if (!mayUnenroll) return BadRequest(reason);
        }
        catch (InvalidOperationException)
        {
            return BadRequest("You cannot unenroll as you are not enrolled in this termin.");
        }
        
        var einschreibungen = (await context.OtiaEinschreibungen
            .Where(e => e.Termin == termin && e.BetroffenePerson == user)
            .ToListAsync())
            .Where(e => e.Interval.Intersects(subBlock.Interval));

        
        // Normally, there should be only one einschreibung, but we will handle multiple for safety.
        foreach (var einschreibung in einschreibungen)
        {
            var (before, after) = einschreibung.Interval.Difference(subBlock.Interval);
            if (before is null && after is null)
            {
                context.OtiaEinschreibungen.Remove(einschreibung);
                await context.SaveChangesAsync();
                return Ok(GetTerminPreview(termin, user));
            }

            if (before is null && after is not null) (before, after) = (after, null);
            einschreibung.Interval = new TimeOnlyInterval(before!.Start, before.Duration);

            if (after is null) continue;
            await context.OtiaEinschreibungen.AddAsync(new Einschreibung
            {
                Termin = termin,
                BetroffenePerson = user,
                Interval = new TimeOnlyInterval(after.Start, after.Duration),
            });
        }
        
        await context.SaveChangesAsync();
        return Ok(GetTerminPreview(termin, user));
    }
    
    private async IAsyncEnumerable<EinschreibungsPreview> GetEinschreibungsPreviews(Person user, Termin termin)
    {
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
    /// Gets the dashboard-information for a user containig all available days.
    /// </summary>
    [HttpGet("dashboard/all")]
    public Task<ActionResult<IAsyncEnumerable<Tag>>> DashboardAll()
    {
        return Dashboard(true);
    }

    /// <summary>
    /// Gets the dashboard-information for a user
    /// </summary>
    [HttpGet("dashboard")]
    public async Task<ActionResult<IAsyncEnumerable<Tag>>> Dashboard(bool all = false)
    {
        var user = await HttpContext.GetPersonAsync(context);

        return Ok(GenerateDashboardAsync(user, all));
    }

    /// <summary>
    /// Get the information about a mentee for a teacher loading all known Termine.
    /// </summary>
    /// <param name="studentId">The ID of the mentee</param>
    [HttpGet("student/{studentId}/all")]
    public Task<ActionResult<LehrerMenteeView>> GetMenteeAll(Guid studentId)
    {
        return GetMentee(studentId, true);
    }
    
    /// <summary>
    /// Gets the information about a mentee for a teacher.
    /// </summary>
    /// <param name="studentId">The ID of the Mentee</param>
    /// <param name="all">If true, loads all known Termine</param>
    [HttpGet("student/{studentId}")]
    public async Task<ActionResult<LehrerMenteeView>> GetMentee(Guid studentId, bool all = false)
    {
        var user = await HttpContext.GetPersonAsync(context);
        if (user.Rolle != Rolle.Tutor) return Unauthorized();
        var student = context.Personen
            .Include(p => p.Mentor)
            .FirstOrDefault(p => p.Id == studentId);
        
        if (student?.Mentor is null) return NotFound();

        if (student.Mentor.Id != user.Id)
            return Unauthorized();
        
        return new LehrerMenteeView(GenerateDashboardAsync(student, all), new PersonInfoMinimal(student));
    }

    private async IAsyncEnumerable<Tag> GenerateDashboardAsync(Person user, bool all)
    {
        // Get Monday of the current week
        var startDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + 1));
        var endDate = startDate.AddDays(7*3);
        
        
        // Okay, this looks heavy. Enumerate to List as we need to access the elements multiple times.
        var allEinschreibungen = await context.OtiaEinschreibungen.AsNoTrackingWithIdentityResolution()
            .Include(e => e.Termin)
            .ThenInclude(t => t.Schultag)
            .Include(e => e.Termin)
            .ThenInclude(t => t.Tutor)
            .Include(e => e.Termin)
            .ThenInclude(t => t.Otium)
            .ThenInclude(o => o.Kategorie)
            .ThenInclude(k => k.Parent)
            .Where(t => t.BetroffenePerson == user)
            .Where(t => all || t.Termin.Schultag.Datum >= startDate && t.Termin.Schultag.Datum < endDate)
            .ToListAsync();
        
        var allSchoolDays = await context.Schultage.AsNoTracking()
            .Where(t => all || t.Datum >= startDate && t.Datum < endDate)
            .OrderBy(s => s.Datum)
            .ToListAsync();
        
        var kategorieRuleByWeek = await CheckAllKategoriesInWeeks(allEinschreibungen);
        var allEnrolledRuleByDay = new Dictionary<DateOnly, bool>();
        // Check if the user is enrolled in all non-optional subblocks
        var einschreibungenByDay = allEinschreibungen.GroupBy(e => e.Termin.Schultag)
            .ToDictionary(g => g.Key, g => g.ToList());
        foreach (var (schultag, einschreibungen) in einschreibungenByDay)
        {
            allEnrolledRuleByDay[schultag.Datum] = AreAllNonOptionalBlocksEnrolled(schultag, einschreibungen);
        }
        
        foreach (var schultag in allSchoolDays)
        {
            var localKategorienErfuellt = kategorieRuleByWeek.FirstOrDefault(e => e.Key.Contains(schultag.Datum.ToDateTime(new TimeOnly()))).Value;
            var vollstaendig = allEnrolledRuleByDay.ContainsKey(schultag.Datum) && allEnrolledRuleByDay[schultag.Datum];
            var einschreibungen = einschreibungenByDay.Keys.Any(k => k.Datum == schultag.Datum) ?
                einschreibungenByDay
                    .FirstOrDefault(t => t.Key.Datum == schultag.Datum)
                    .Value
                    .OrderBy(e => e.Interval.Start)
                    .Select(e => new Data.DTO.Otium.Einschreibung(e))
                : [];
            var tag = new Tag(schultag.Datum, 
                vollstaendig, 
                localKategorienErfuellt, 
                einschreibungen
            );
            
            yield return tag;
        }
    }

    private bool AreAllNonOptionalBlocksEnrolled(Schultag schultag, List<Einschreibung> einschreibungen)
    {
        var timeline = new Timeline<TimeOnly>();
        for (var i = 0; i < _blocks.Count; i++)
        {
            if (!schultag.OtiumsBlock[i]) continue;
            foreach (var subBlock in _blocks[i].Where(b => !b.Optional)) timeline.Add(subBlock.Interval);
        }

        foreach (var einschreibung in einschreibungen) timeline.Remove(einschreibung.Interval);
            
        var allNonOptionalBlocksEnrolled = timeline.GetIntervals().Count == 0;
        return allNonOptionalBlocksEnrolled;
    }

    /// <summary>
    ///     Returns an overview of termine and mentees for a teacher.
    /// </summary>
    /// <exception cref="UnauthorizedAccessException">The user is not a teacher.</exception>
    [HttpGet("dashboard/teacher")]
    public async Task<LehrerUebersicht> GetTeacherDashboard()
    {
        var user = await HttpContext.GetPersonAsync(context);
        if (user.Rolle != Rolle.Tutor) throw new UnauthorizedAccessException("Only teachers may access this endpoint.");
        
        var startDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + 1));
        var endDate = startDate.AddDays(21);

        var mentees = await context.Personen
            .Where(p => p.Mentor != null && p.Mentor.Id == user.Id)
            .Include(p => p.OtiaEinschreibungen
                .Where(e => e.Termin.Schultag.Datum >= startDate && e.Termin.Schultag.Datum < endDate))
            .ThenInclude(p => p.Termin)
            .ThenInclude(p => p.Schultag)
            .OrderBy(p => p.Vorname)
            .ThenBy(p => p.Nachname)
            .ToListAsync();
        
        var schultage = context.Schultage.Where(s => s.Datum >= startDate && s.Datum < endDate);

        List<MenteePreview> menteePreviews = [];
        
        foreach (var mentee in mentees)
        {
            menteePreviews.Add(await GenerateMenteePreview(mentee, schultage, startDate));
        }
        
        List<LehrerTerminPreview> terminPreviews = [];

        var termine = await context.OtiaTermine
            .Include(t => t.Otium)
            .Include(t => t.Schultag)
            .OrderBy(t => t.Schultag.Datum)
            .ThenBy(t => t.Block)
            .Where(t => !t.IstAbgesagt && (t.Tutor != null && t.Tutor.Id == user.Id) &&
                        t.Schultag.Datum >= DateOnly.FromDateTime(DateTime.Today) && t.Schultag.Datum < endDate)
            .ToListAsync();

        foreach (var termin in termine)
        {
            terminPreviews.Add(
                new LehrerTerminPreview(termin.Id, termin.Otium.Bezeichnung, termin.Ort, await CalculateLoad(termin), termin.Schultag.Datum, termin.Block)
                );
        }

        return new LehrerUebersicht(terminPreviews, menteePreviews);
    }

    private async Task<MenteePreview> GenerateMenteePreview(Person mentee, IEnumerable<Schultag> schulage, DateOnly startDate)
    {
        var lastWeekInterval = new DateTimeInterval(startDate.ToDateTime(new TimeOnly(0, 0)), TimeSpan.FromDays(7));
        var thisWeekInterval = new DateTimeInterval(lastWeekInterval.End, TimeSpan.FromDays(7));
        var nextWeekInterval = new DateTimeInterval(thisWeekInterval.End, TimeSpan.FromDays(7));
        
        var enrollmentsPerDay = mentee.OtiaEinschreibungen.GroupBy(e => e.Termin.Schultag).ToDictionary(g => g.Key, g => g.ToList());
        var isFullyEnrolledPerDay = new Dictionary<DateOnly, bool>();
        
        foreach (var schultag in schulage)
        {
            isFullyEnrolledPerDay[schultag.Datum] = AreAllNonOptionalBlocksEnrolled(schultag, enrollmentsPerDay.TryGetValue(schultag, out var value) ? value : []);
        }
        
        var kategorieRuleByWeek = await CheckAllKategoriesInWeeks(mentee.OtiaEinschreibungen.ToList());

        return new MenteePreview(new PersonInfoMinimal(mentee),
            IsWeekOkay(lastWeekInterval) ? MenteePreviewStatus.Okay : MenteePreviewStatus.Auffaellig,
            IsWeekOkay(thisWeekInterval) ? MenteePreviewStatus.Okay : MenteePreviewStatus.Auffaellig,
            IsWeekOkay(nextWeekInterval) ? MenteePreviewStatus.Okay : MenteePreviewStatus.Auffaellig
            );

        bool IsWeekOkay(DateTimeInterval week)
        {
            return isFullyEnrolledPerDay.All(group =>
                       !week.Contains(group.Key.ToDateTime(TimeOnly.MinValue)) || group.Value) &&
                   kategorieRuleByWeek[week];
        }
    }

    private async Task<Dictionary<DateTimeInterval, bool>> CheckAllKategoriesInWeeks(List<Einschreibung> allEinschreibungen)
    {
        // This is also used to load all kategories so we do not have to lazy load them later. One query is better than many.
        var kategories = await context.OtiaKategorien.AsNoTrackingWithIdentityResolution()
            .Include(k => k.Children)
            .ToListAsync();
        var requiredKategories = kategories.Where(k => k.Required).ToList();
        
        
        // Looping asynchronously can do strange things, so lets use ConcurrentDictionary in hopes we don't need it.
        var einschreibungenByWeek = new ConcurrentDictionary<DateTimeInterval, ConcurrentBag<Einschreibung>>();

        foreach (var einschreibung in allEinschreibungen)
        {
            var entry = einschreibungenByWeek.FirstOrDefault(e =>
                e.Key.Contains(einschreibung.Termin.Schultag.Datum.ToDateTime(einschreibung.Interval.Start)));
            var key = entry.Key;
            if (entry.Value is null)
            {
                var datum = einschreibung.Termin.Schultag.Datum.ToDateTime(new TimeOnly(0, 0));
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
                var currentKategorie = einschreibung.Termin.Otium.Kategorie;
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
    ///     Check if the user may enroll in a termin.
    /// </summary>
    /// <param name="user">The user wanting to unenroll</param>
    /// <param name="termin">The termin the user wants to unenroll to</param>
    /// <param name="subBlock">The subblock during which the user wants to unenroll</param>
    /// <exception cref="InvalidOperationException">The user is not enrolled for the Termin during the SubBlock</exception>
    /// <returns>True, if the user may unenroll; Otherwise, false.</returns>
    private async Task<(bool, string?)> MayUnenroll(Person user, Termin termin, SubBlock subBlock)
    {
        var common = CommonMayUnEnroll(user, termin, subBlock);
        if (!common.Item1) return common;

        var parallelEnrollment = (await context.OtiaEinschreibungen
                .Include(e => e.Termin)
                .Where(e => e.BetroffenePerson == user && e.Termin.Id == termin.Id)
                .ToListAsync())
            .First(e => e.Interval.Intersects(subBlock.Interval));

        var (before, after) = parallelEnrollment.Interval.Difference(subBlock.Interval);
        var mayUnenroll = (after is null || after.Duration >= TimeSpan.FromMinutes(30)) &&
               (before is null || before.Duration >= TimeSpan.FromMinutes(30));
        return mayUnenroll ? (true, null) : (false, "Durch das Austragen entsteht eine Einschreibung kürzer als die Mindestzeit.");
    }

    /// <inheritdoc cref="MayEnroll(Afra_App.Data.People.Person,Termin,SubBlock,int)"/>
    private async Task<(bool, string?)> MayEnroll(Person user, Termin termin, SubBlock subBlock)
    {
        var countEnrolled = (await context.OtiaEinschreibungen.AsNoTracking()
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
        var blockEnrollments = await context.OtiaEinschreibungen
            .Include(e => e.Termin)
            .Where(e => e.BetroffenePerson == user && e.Termin.Schultag == termin.Schultag &&
                        e.Termin.Block == termin.Block)
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
        return lastAvailableBlockRuleFulfilled ? (true, null) : (false, "Sie sind nicht in allen erforderlichen Kategorien eingeschrieben. Durch diese Einschreibung wäre das nicht mehr möglich.");
    }

    private static (bool, string?) CommonMayUnEnroll(Person user, Termin termin, SubBlock subBlock)
    {
        var startDateTime = new DateTime(termin.Schultag.Datum, subBlock.Interval.Start);
        if (startDateTime <= DateTime.Now) return (false, "Der Termin hat bereits begonnen.");
        
        if (user.Rolle != Rolle.Student) return (false, "Nur Schüler:innen können sich einschreiben.");
        
        return (true, null);
    }

    // Come here for some hideous shit; Optimizing this is a problem for future me.
    // This currently needs three separate SQL-Queries + loading all categories.
    private async Task<bool> LastAvailableBlockRuleFulfilled(Person user, Termin termin, SubBlock subBlock)
    {
        // Find all required kategories the user is not enrolled to. -> notEnrolled[]
        var firstDayOfWeek = termin.Schultag.Datum.AddDays(-(int)termin.Schultag.Datum.DayOfWeek + 1);
        var lastDayOfWeek = firstDayOfWeek.AddDays(7);
        var weekInterval = new DateTimeInterval(new DateTime(firstDayOfWeek, TimeOnly.MinValue),
            TimeSpan.FromDays(7));

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
            if (await IsEnrolledInKategorie(kategorie, userKategoriesInWeek)) continue;
            notEnrolled.Add(kategorie);
            break;
        }

        if (notEnrolled.Count == 0)
            return true;

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
        var timeline = new Timeline<DateTime>();

        foreach (var schultag in schultage) /* I know this is ugly, but copilot has no better idea too... */
            for (var i = 0; i < _blocks.Count; i++)
                if (schultag.OtiumsBlock[i])
                    timeline.Add(_blocks[i].Skip(1)
                        .Aggregate(_blocks[i].First().Interval,
                            (interval, current) => (TimeOnlyInterval)interval.Union(current.Interval))
                        .ToDateTimeInterval(schultag.Datum));
        timeline.Remove(subBlock.Interval.ToDateTimeInterval(termin.Schultag.Datum));
        foreach (var enrollment in usersEnrollmentsInWeek)
            timeline.Remove(enrollment.Interval.ToDateTimeInterval(enrollment.Termin.Schultag.Datum));
        var num30MinBlockAvailable = timeline.GetIntervals().Sum(i => (int)(i.Duration / TimeSpan.FromMinutes(30)));

        return num30MinBlockAvailable >= notEnrolled.Count;
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
            logger.LogInformation("Fetching Kategorie: {kategorie}", currentCategory.Id);
            yield return currentCategory.Id;
            await context.Entry(currentCategory).Reference(c => c.Parent).LoadAsync();
            currentCategory = currentCategory.Parent;
        }
    }
}