using Afra_App.Data;
using Afra_App.Data.Configuration;
using Afra_App.Data.DTO;
using Afra_App.Data.DTO.Otium;
using Afra_App.Data.TimeInterval;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Person = Afra_App.Data.People.Person;

namespace Afra_App.Services.Otium;

/// <summary>
///     A Service for handling requests to the Otium endpoint.
/// </summary>
public class OtiumEndpointService
{
    private readonly AfraAppContext _context;
    private readonly EnrollmentService _enrollmentService;
    private readonly KategorieService _kategorieService;
    private readonly OtiumConfiguration _otiumConfiguration;

    /// <summary>
    ///     Constructor for the OtiumEndpointService. Usually called by the DI container.
    /// </summary>
    public OtiumEndpointService(AfraAppContext context, KategorieService kategorieService,
        IOptions<OtiumConfiguration> otiumConfiguration, EnrollmentService enrollmentService)
    {
        _context = context;
        _kategorieService = kategorieService;
        _enrollmentService = enrollmentService;
        _otiumConfiguration = otiumConfiguration.Value;
    }

    /// <summary>
    ///     Retrieves the Otium data for a given date and block.
    /// </summary>
    /// <param name="date">The date for which to retrieve the Otium data.</param>
    /// <returns>A List of all Otia happening at that time.</returns>
    public async IAsyncEnumerable<TerminPreview> GetKatalogForDay(DateOnly date)
    {
        // Get the schultag for the given date and block
        var blocks = await _context.Blocks
            .Where(b => b.Schultag.Datum == date)
            .ToListAsync();


        if (blocks.Count == 0) yield break;

        // Get all termine for the given schultag and block
        // Note: This needs to be materialized before the foreach loop as EF Core does not support multiple active queries.
        // Note: This needs to be a tracking query as we need to load the related entities at a later point.
        var termine = await _context.OtiaTermine
            .Where(t => blocks.Contains(t.Block))
            .Include(t => t.Otium)
            .ThenInclude(o => o.Kategorie)
            .Include(t => t.Tutor)
            .OrderBy(t => t.IstAbgesagt)
            .ThenBy(t => t.Block.Nummer)
            .ThenBy(t => t.Otium.Bezeichnung)
            .ToListAsync();


        // Calculate the load for each termin and cast it to a json object
        foreach (var termin in termine)
            yield return new TerminPreview(termin,
                await _enrollmentService.GetLoadPercent(termin),
                _kategorieService.GetTransitiveKategoriesIdsAsyncEnumerable(termin.Otium.Kategorie));
    }

    /// <summary>
    ///     Get the details of a termin.
    /// </summary>
    /// <param name="terminId">The <see cref="Guid" /> of the termin to get details for</param>
    /// <param name="user">The user requesting the termin</param>
    public async Task<Termin?> GetTerminAsync(Guid terminId, Person user)
    {
        var termin = await _context.OtiaTermine
            .Include(termin => termin.Tutor)
            .Include(termin => termin.Otium)
            .ThenInclude(otium => otium.Kategorie)
            .Include(termin => termin.Block)
            .ThenInclude(block => block.Schultag)
            .FirstOrDefaultAsync(t => t.Id == terminId);
        if (termin == null) return null;

        return new Termin(termin,
            _enrollmentService.GetEnrolmentPreviews(user, termin),
            _kategorieService.GetTransitiveKategoriesIdsAsyncEnumerable(termin.Otium.Kategorie),
            _otiumConfiguration.Blocks[termin.Block.Nummer].First().Interval.Start);
    }

    /// <summary>
    ///     Generates the dashboard for a student.
    /// </summary>
    /// <param name="user">The student to generate the dashboard for</param>
    /// <param name="all">Iff true, all available school-days are included. Otherwise just the current and next two weeks.</param>
    public async IAsyncEnumerable<Tag> GetStudentDashboardAsyncEnumerable(Person user, bool all)
    {
        // Get Monday of the current week
        var startDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + 1));
        var endDate = startDate.AddDays(7 * 3);


        // Okay, this looks heavy. Enumerate to List as we need to access the elements multiple times.
        var allEinschreibungen = await _context.OtiaEinschreibungen.AsNoTrackingWithIdentityResolution()
            .Include(e => e.Termin)
            .ThenInclude(t => t.Block)
            .ThenInclude(b => b.Schultag)
            .Include(e => e.Termin)
            .ThenInclude(t => t.Tutor)
            .Include(e => e.Termin)
            .ThenInclude(t => t.Otium)
            .ThenInclude(o => o.Kategorie)
            .ThenInclude(k => k.Parent)
            .Where(t => t.BetroffenePerson == user)
            .Where(t => all || (t.Termin.Block.Schultag.Datum >= startDate && t.Termin.Block.Schultag.Datum < endDate))
            .ToListAsync();

        var allSchoolDays = await _context.Schultage.AsNoTracking()
            .Where(t => all || (t.Datum >= startDate && t.Datum < endDate))
            .OrderBy(s => s.Datum)
            .ToListAsync();

        var kategorieRuleByWeek = await _enrollmentService.CheckAllKategoriesInWeeks(allEinschreibungen);
        var allEnrolledRuleByDay = new Dictionary<DateOnly, bool>();

        // Check if the user is enrolled in all non-optional subblocks
        var einschreibungenByDay = allEinschreibungen.GroupBy(e => e.Termin.Block.Schultag)
            .ToDictionary(g => g.Key, g => g.ToList());
        foreach (var (schultag, einschreibungen) in einschreibungenByDay)
            allEnrolledRuleByDay[schultag.Datum] =
                _enrollmentService.AreAllNonOptionalBlocksEnrolled(schultag, einschreibungen);

        foreach (var schultag in allSchoolDays)
        {
            var localKategorienErfuellt = kategorieRuleByWeek
                .FirstOrDefault(e => e.Key.Contains(schultag.Datum.ToDateTime(new TimeOnly()))).Value;
            var vollstaendig = allEnrolledRuleByDay.ContainsKey(schultag.Datum) && allEnrolledRuleByDay[schultag.Datum];
            var einschreibungen = einschreibungenByDay.Keys.Any(k => k.Datum == schultag.Datum)
                ? einschreibungenByDay
                    .FirstOrDefault(t => t.Key.Datum == schultag.Datum)
                    .Value
                    .OrderBy(e => e.Interval.Start)
                    .Select(e => new Einschreibung(e))
                : [];
            var tag = new Tag(schultag.Datum,
                vollstaendig,
                localKategorienErfuellt,
                einschreibungen
            );

            yield return tag;
        }
    }

    /// <summary>
    ///     Returns an overview of termine and mentees for a teacher.
    /// </summary>
    public async Task<LehrerUebersicht> GetTeacherDashboardAsync(Person user)
    {
        var startDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek - 6));
        var endDate = startDate.AddDays(21);

        var mentees = await _context.Personen
            .Where(p => p.Mentor != null && p.Mentor.Id == user.Id)
            .Include(p => p.OtiaEinschreibungen
                .Where(e => e.Termin.Block.Schultag.Datum >= startDate && e.Termin.Block.Schultag.Datum < endDate))
            .ThenInclude(p => p.Termin)
            .ThenInclude(p => p.Block)
            .ThenInclude(b => b.Schultag)
            .Include(p => p.OtiaEinschreibungen)
            .ThenInclude(e => e.Termin)
            .ThenInclude(t => t.Otium)
            .ThenInclude(o => o.Kategorie)
            .OrderBy(p => p.Vorname)
            .ThenBy(p => p.Nachname)
            .ToListAsync();

        var schultage = _context.Schultage.Where(s => s.Datum >= startDate && s.Datum < endDate);

        List<MenteePreview> menteePreviews = [];

        var lastWeekInterval = new DateTimeInterval(startDate.ToDateTime(new TimeOnly(0, 0)), TimeSpan.FromDays(7));
        var thisWeekInterval = new DateTimeInterval(lastWeekInterval.End, TimeSpan.FromDays(7));
        var nextWeekInterval = new DateTimeInterval(thisWeekInterval.End, TimeSpan.FromDays(7));

        foreach (var mentee in mentees) menteePreviews.Add(await GenerateMenteePreview(mentee));

        List<LehrerTerminPreview> terminPreviews = [];

        var termine = await _context.OtiaTermine
            .Include(t => t.Otium)
            .Include(t => t.Block)
            .ThenInclude(b => b.Schultag)
            .OrderBy(t => t.Block.Schultag.Datum)
            .ThenBy(t => t.Block.Nummer)
            .Where(t => !t.IstAbgesagt && t.Tutor != null && t.Tutor.Id == user.Id &&
                        t.Block.Schultag.Datum >= DateOnly.FromDateTime(DateTime.Today) &&
                        t.Block.Schultag.Datum < endDate)
            .ToListAsync();

        foreach (var termin in termine)
            terminPreviews.Add(
                new LehrerTerminPreview(termin.Id, termin.Otium.Bezeichnung, termin.Ort,
                    await _enrollmentService.GetLoadPercent(termin), termin.Block.Schultag.Datum, termin.Block.Nummer)
            );

        return new LehrerUebersicht(terminPreviews, menteePreviews);


        async Task<MenteePreview> GenerateMenteePreview(Person mentee)
        {
            var enrollmentsPerDay = mentee.OtiaEinschreibungen.GroupBy(e => e.Termin.Block.Schultag.Datum)
                .ToDictionary(g => g.Key, g => g.ToList());
            var isFullyEnrolledPerDay = new Dictionary<DateOnly, bool>();

            foreach (var schultag in schultage)
                isFullyEnrolledPerDay[schultag.Datum] = _enrollmentService.AreAllNonOptionalBlocksEnrolled(schultag,
                    enrollmentsPerDay.TryGetValue(schultag.Datum, out var value) ? value : []);

            var kategorieRuleByWeek =
                await _enrollmentService.CheckAllKategoriesInWeeks(mentee.OtiaEinschreibungen.ToList());

            return new MenteePreview(new PersonInfoMinimal(mentee),
                IsWeekOkay(lastWeekInterval) ? MenteePreviewStatus.Okay : MenteePreviewStatus.Auffaellig,
                IsWeekOkay(thisWeekInterval) ? MenteePreviewStatus.Okay : MenteePreviewStatus.Auffaellig,
                IsWeekOkay(nextWeekInterval) ? MenteePreviewStatus.Okay : MenteePreviewStatus.Auffaellig
            );

            bool IsWeekOkay(DateTimeInterval week)
            {
                return isFullyEnrolledPerDay
                           .Where(group => week.Contains(group.Key.ToDateTime(TimeOnly.MinValue)))
                           .All(group => group.Value) &&
                       kategorieRuleByWeek.ContainsKey(week) && kategorieRuleByWeek[week];
            }
        }
    }

    /// <summary>
    ///     Generates the dashboard view as a mentee would see it and adds information about the mentee
    /// </summary>
    public LehrerMenteeView GetStudentDashboardForTeacher(Person student, bool all = false)
    {
        return new LehrerMenteeView(
            GetStudentDashboardAsyncEnumerable(student, all),
            new PersonInfoMinimal(student));
    }

    /// <summary>
    ///     Gets the detailed information about a termin for a teacher.
    /// </summary>
    public async Task<LehrerTermin?> GetTerminForTeacher(Guid terminId, Person teacher)
    {
        var termin = await _context.OtiaTermine
            .Include(t => t.Tutor)
            .Include(t => t.Block)
            .ThenInclude(b => b.Schultag)
            .Include(t => t.Otium)
            .Include(t => t.Enrollments)
            .ThenInclude(e => e.BetroffenePerson)
            .Where(t => !t.IstAbgesagt)
            .FirstOrDefaultAsync(t => t.Id == terminId);

        if (termin?.Tutor is null || termin.Tutor.Id != teacher.Id) return null;

        return new LehrerTermin
        {
            Id = termin.Id,
            Ort = termin.Ort,
            Otium = termin.Otium.Bezeichnung,
            Block = termin.Block.Nummer,
            Datum = termin.Block.Schultag.Datum,
            Tutor = new PersonInfoMinimal(termin.Tutor),
            Einschreibungen = termin.Enrollments.Select(e =>
                new LehrerEinschreibung(new PersonInfoMinimal(e.BetroffenePerson), e.Interval,
                    AnwesenheitsStatus.Anwesend))
        };
    }
}