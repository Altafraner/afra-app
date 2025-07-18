using System.Text;
using Afra_App.Backbone.Domain.TimeInterval;
using Afra_App.Backbone.Services.Email;
using Afra_App.Otium.Domain.DTO;
using Afra_App.Otium.Domain.DTO.Dashboard;
using Afra_App.Otium.Domain.DTO.Katalog;
using Afra_App.Otium.Domain.Models.Schuljahr;
using Afra_App.User.Domain.DTO;
using Afra_App.User.Domain.Models;
using Microsoft.EntityFrameworkCore;
using DB_Otium = Afra_App.Otium.Domain.Models.Otium;
using DB_Schultag = Afra_App.Otium.Domain.Models.Schuljahr.Schultag;
using DB_Termin = Afra_App.Otium.Domain.Models.Termin;
using DB_Wiederholung = Afra_App.Otium.Domain.Models.Wiederholung;
using DTO_Otium_Creation = Afra_App.Otium.Domain.DTO.ManagementOtiumCreation;
using DTO_Otium_View = Afra_App.Otium.Domain.DTO.ManagementOtiumView;
using DTO_Termin_Creation = Afra_App.Otium.Domain.DTO.ManagementTerminCreation;
using DTO_Wiederholung_Creation = Afra_App.Otium.Domain.DTO.ManagementWiederholungCreation;
using Einschreibung = Afra_App.Otium.Domain.DTO.Einschreibung;
using Person = Afra_App.User.Domain.Models.Person;
using Termin = Afra_App.Otium.Domain.DTO.Katalog.Termin;

namespace Afra_App.Otium.Services;

/// <summary>
///     A Service for handling requests to the Otium endpoint.
/// </summary>
public class OtiumEndpointService
{
    private readonly IAttendanceService _attendanceService;
    private readonly BlockHelper _blockHelper;
    private readonly AfraAppContext _dbContext;
    private readonly IEmailOutbox _emailOutbox;
    private readonly EnrollmentService _enrollmentService;
    private readonly KategorieService _kategorieService;
    private readonly SchuljahrService _schuljahrService;

    /// <summary>
    ///     Constructor for the OtiumEndpointService. Usually called by the DI container.
    /// </summary>
    public OtiumEndpointService(AfraAppContext dbContext, KategorieService kategorieService,
        EnrollmentService enrollmentService,
        IEmailOutbox emailOutbox, BlockHelper blockHelper, IAttendanceService attendanceService,
        SchuljahrService schuljahrService)
    {
        _dbContext = dbContext;
        _kategorieService = kategorieService;
        _enrollmentService = enrollmentService;
        _emailOutbox = emailOutbox;
        _blockHelper = blockHelper;
        _attendanceService = attendanceService;
        _schuljahrService = schuljahrService;
    }

    /// <summary>
    ///     Gets the Katalog for a given date.
    /// </summary>
    /// <param name="person">The person the generate the messages for</param>
    /// <param name="date">The date to get the <see cref="TerminPreview" />s for</param>
    public async Task<Tag> GetKatalogForDay(Person person, DateOnly date)
    {
        return new Tag(GetTerminPreviewsForDay(date),
            person.Rolle != Rolle.Oberstufe ? await GetStatusForDayAsync(person, date) : []);
    }

    /// <summary>
    ///     Retrieves the Otium data for a given date.
    /// </summary>
    /// <param name="date">The date for which to retrieve the Otium data.</param>
    /// <returns>A List of all Otia happening at that time.</returns>
    private async IAsyncEnumerable<TerminPreview> GetTerminPreviewsForDay(DateOnly date)
    {
        // Get the schultag for the given date and block
        var blocks = await _dbContext.Blocks
            .Where(b => b.Schultag.Datum == date)
            .ToListAsync();


        if (blocks.Count == 0) yield break;

        // Get all termine for the given schultag and block
        // Note: This needs to be materialized before the foreach loop as EF Core does not support multiple active queries.
        // Note: This needs to be a tracking query as we need to load the related entities at a later point.
        var termine = await _dbContext.OtiaTermine
            .Where(t => blocks.Contains(t.Block))
            .Include(t => t.Otium)
            .ThenInclude(o => o.Kategorie)
            .Include(t => t.Tutor)
            .OrderBy(t => t.IstAbgesagt)
            .ThenBy(t => t.Block.SchemaId)
            .ThenBy(t => t.Otium.Bezeichnung)
            .Select(t => new TerminWithLoad
            {
                Termin = t,
                Auslasung = t.MaxEinschreibungen == null
                    ? null
                    : (int)Math.Round((double)t.Enrollments.Count * 100 / t.MaxEinschreibungen.Value)
            })
            .ToListAsync();


        // Calculate the load for each termin and cast it to a json object
        foreach (var termin in termine)
            yield return new TerminPreview(termin.Termin,
                termin.Auslasung,
                _kategorieService.GetTransitiveKategoriesIdsAsyncEnumerable(termin.Termin.Otium.Kategorie));
    }

    /// <summary>
    ///     Get the details of a termin.
    /// </summary>
    /// <param name="terminId">The <see cref="Guid" /> of the termin to get details for</param>
    /// <param name="user">The user requesting the termin</param>
    public async Task<Termin?> GetTerminAsync(Guid terminId, Person user)
    {
        var termin = await _dbContext.OtiaTermine
            .Include(termin => termin.Tutor)
            .Include(termin => termin.Otium)
            .ThenInclude(otium => otium.Kategorie)
            .Include(termin => termin.Block)
            .ThenInclude(block => block.Schultag)
            .Include(termin => termin.Wiederholung)
            .ThenInclude(wdh => wdh!.Termine)
            .ThenInclude(wdhTermin => wdhTermin.Block)
            .AsSplitQuery()
            .FirstOrDefaultAsync(t => t.Id == terminId);
        if (termin == null) return null;

        return new Termin(termin,
            await _enrollmentService.GetEnrolmentPreview(user, termin),
            termin.Otium.Kategorie.Id,
            _blockHelper.Get(termin.Block.SchemaId)!.Interval.Start);
    }

    private async Task<IEnumerable<string>> GetStatusForDayAsync(Person user, DateOnly date)
    {
        List<string> messages = [];

        // Week Start and End
        var weekStart = date.AddDays(-(int)date.DayOfWeek + 1);
        var weekEnd = weekStart.AddDays(7);

        // Get all blocks for the given week
        var blocks = await _dbContext.Schultage
            .Where(s => s.Datum >= weekStart && s.Datum < weekEnd)
            .SelectMany(s => s.Blocks)
            .OrderBy(b => b.SchultagKey)
            .ThenBy(b => b.SchemaId)
            .ToListAsync();

        // Get all enrollments for the given week
        var weeksEnrollments = await _dbContext.OtiaEinschreibungen
            .Include(e => e.Termin)
            .ThenInclude(t => t.Otium)
            .ThenInclude(o => o.Kategorie)
            .Include(einschreibung => einschreibung.Termin)
            .ThenInclude(termin => termin.Block)
            .Where(e => blocks.Contains(e.Termin.Block) &&
                        e.BetroffenePerson.Id == user.Id)
            .ToListAsync();

        // Find all times on date the user is not enrolled in
        var timeline = _enrollmentService.GetNotEnrolledTimes(
            blocks.Where(b => b.SchultagKey == date),
            user);

        messages.AddRange(timeline.GetIntervals().Select(interval =>
            $"Es fehlen Einschreibungen von {interval.Start:t} bis {interval.End:t}."));

        var missingCategories = await _enrollmentService.GetMissingKategories(weeksEnrollments);

        messages.AddRange(missingCategories.Select(category =>
            $"Es muss mindestens ein Angebot der Kategorie \"{category}\" pro Woche für einen vollen Block belegt werden."));

        return messages;
    }

    /// <summary>
    ///     Generates the dashboard for a student.
    /// </summary>
    /// <param name="user">The student to generate the dashboard for</param>
    /// <param name="all">Iff true, all available school-days are included. Otherwise, just the current and next two weeks.</param>
    // I hate myself for writing this mess of a method. Have fun!
    public async IAsyncEnumerable<Week> GetStudentDashboardAsyncEnumerable(Person user,
        bool all)
    {
        var thisMonday = GetMonday(DateOnly.FromDateTime(DateTime.Today));

        var startDate = thisMonday;
        var endDate = startDate.AddDays(7 * 3);

        IQueryable<DB_Schultag> schultageQuery = _dbContext.Schultage
            .Include(s => s.Blocks)
            .OrderBy(s => s.Datum);
        if (!all)
        {
            schultageQuery = schultageQuery.Where(s => s.Datum >= startDate && s.Datum < endDate);
        }

        var schultage = await schultageQuery.ToListAsync();

        var einschreibungen = await _dbContext.OtiaEinschreibungen
            .Where(e => e.BetroffenePerson.Id == user.Id)
            .Include(e => e.Termin)
            .ThenInclude(e => e.Block)
            .ThenInclude(e => e.Schultag)
            .Include(e => e.Termin)
            .ThenInclude(e => e.Tutor)
            .Include(e => e.Termin)
            .ThenInclude(e => e.Otium)
            .ThenInclude(e => e.Kategorie)
            .OrderBy(s => s.Termin.Block.SchultagKey)
            .ThenBy(s => s.Termin.Block.SchemaId)
            .Where(e => schultage.Contains(e.Termin.Block.Schultag))
            .ToListAsync();

        var weeks = schultage.GroupBy(s => GetMonday(s.Datum));

        foreach (var week in weeks)
        {
            var weekEnd = week.Key.AddDays(7);
            var einschreibungenForWeek = einschreibungen
                .TakeWhile(e => e.Termin.Block.SchultagKey < weekEnd)
                .ToList();
            einschreibungen.RemoveRange(0, einschreibungenForWeek.Count);

            var messageBuilder = new StringBuilder();

            var kategorieRule = await _enrollmentService.GetMissingKategories(einschreibungenForWeek);

            if (kategorieRule.Count > 0)
            {
                messageBuilder.AppendLine("**Fehlende Kategorien**");
                foreach (var kategorie in kategorieRule)
                {
                    messageBuilder.AppendLine(
                        $"""- Es muss mindestens ein Angebot der Kategorie *"{kategorie}"* pro Woche für einen vollen Block belegt werden.""");
                }

                messageBuilder.AppendLine();
            }

            var enrollmentsByDay = einschreibungenForWeek.GroupBy(e => e.Termin.Block.SchultagKey)
                .ToDictionary(e => e.Key, e => e.ToList());

            foreach (var day in week)
            {
                var enrollmentsForDay = enrollmentsByDay.TryGetValue(day.Datum, out var list) ? list : [];
                var missingEnrollmentsForDay =
                    _enrollmentService.GetNotEnrolledTimes(day.Blocks, enrollmentsForDay).GetIntervals();

                if (missingEnrollmentsForDay.Count <= 0) continue;
                messageBuilder.AppendLine($"**Fehlende Einschreibungen am {day.Datum.ToString("dd.MM.yyyy")}**");
                foreach (var interval in missingEnrollmentsForDay)
                {
                    messageBuilder.AppendLine(
                        $"- Es fehlen Einschreibungen von {interval.Start:t} Uhr bis {interval.End:t} Uhr.");
                }

                messageBuilder.AppendLine();
            }

            yield return new Week(
                week.Key,
                messageBuilder.ToString(),
                einschreibungenForWeek.Select(e => new Einschreibung(e))
            );
        }
        /*
        // Get Monday of the current week
        var startDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + 1));
        var endDate = startDate.AddDays(7 * 3);

        // Okay, this looks heavy. Enumerate to List as we need to access the elements multiple times.
        var allEinschreibungen = await _dbContext.OtiaEinschreibungen
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

        var allSchoolDays = await _dbContext.Schultage
            .Include(s => s.Blocks)
            .Where(t => all || (t.Datum >= startDate && t.Datum < endDate))
            .OrderBy(s => s.Datum)
            .ToListAsync();

        var einschreibungenByDay = allEinschreibungen.GroupBy(e => e.Termin.Block.Schultag)
            .ToDictionary(g => g.Key, g => g.ToList());

        // Check if the user is enrolled in all non-optional subblocks
        var kategorieRuleByWeek = user.Rolle != Rolle.Oberstufe
            ? await _enrollmentService.CheckAllKategoriesInWeeks(allEinschreibungen)
            : [];
        var allEnrolledRuleByDay = new Dictionary<DateOnly, bool>();
        foreach (var (schultag, einschreibungen) in einschreibungenByDay)
            allEnrolledRuleByDay[schultag.Datum] =
                _enrollmentService.AreAllNonOptionalBlocksEnrolled(schultag, einschreibungen);

        foreach (var schultag in allSchoolDays)
        {
            var monday = GetMonday(schultag.Datum);

            var localKategorienErfuellt =
                user.Rolle == Rolle.Oberstufe || kategorieRuleByWeek.GetValueOrDefault(monday, false);
            var vollstaendig = allEnrolledRuleByDay.ContainsKey(schultag.Datum) && allEnrolledRuleByDay[schultag.Datum];
            var einschreibungen = einschreibungenByDay.Keys.Any(k => k.Datum == schultag.Datum)
                ? einschreibungenByDay
                    .FirstOrDefault(t => t.Key.Datum == schultag.Datum)
                    .Value
                    .OrderBy(e => e.Termin.Block.SchemaId)
                    .Select(e => new Einschreibung(e))
                : [];
            var tag = new Domain.DTO.Dashboard.Tag(schultag.Datum,
                vollstaendig,
                localKategorienErfuellt,
                einschreibungen
            );

            yield return tag;
        }*/
    }

    private DateOnly GetMonday(DateOnly date) => date.AddDays(-(int)date.DayOfWeek + 1);

    /// <summary>
    ///     Returns an overview of termine and mentees for a teacher.
    /// </summary>
    public async Task<LehrerUebersicht> GetTeacherDashboardAsync(Person user)
    {
        var startDate = DateOnly.FromDateTime(DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek - 6));
        var endDate = startDate.AddDays(21);

        var mentees = await _dbContext.Personen
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

        var schultage = _dbContext.Schultage
            .Include(s => s.Blocks)
            .Where(s => s.Datum >= startDate && s.Datum < endDate);

        List<MenteePreview> menteePreviews = [];

        var lastWeekInterval = new DateTimeInterval(startDate.ToDateTime(new TimeOnly(0, 0)), TimeSpan.FromDays(7));
        var thisWeekInterval = new DateTimeInterval(lastWeekInterval.End, TimeSpan.FromDays(7));
        var nextWeekInterval = new DateTimeInterval(thisWeekInterval.End, TimeSpan.FromDays(7));

        foreach (var mentee in mentees) menteePreviews.Add(await GenerateMenteePreview(mentee));

        List<LehrerTerminPreview> terminPreviews = [];

        var termine = await _dbContext.OtiaTermine
            .Include(t => t.Otium)
            .Include(t => t.Block)
            .ThenInclude(b => b.Schultag)
            .OrderBy(t => t.Block.Schultag.Datum)
            .ThenBy(t => t.Block.SchemaId)
            .Where(t => !t.IstAbgesagt && t.Tutor != null && t.Tutor.Id == user.Id &&
                        t.Block.Schultag.Datum >= DateOnly.FromDateTime(DateTime.Today) &&
                        t.Block.Schultag.Datum < endDate)
            .ToListAsync();

        foreach (var termin in termine)
            terminPreviews.Add(
                new LehrerTerminPreview(termin.Id, termin.Otium.Bezeichnung, termin.Ort,
                    await _enrollmentService.GetLoadPercent(termin), termin.Block.Schultag.Datum, termin.Block.SchemaId)
            );

        return new LehrerUebersicht(terminPreviews, menteePreviews);


        async Task<MenteePreview> GenerateMenteePreview(Person mentee)
        {
            if (mentee.Rolle == Rolle.Oberstufe)
                return new MenteePreview(new PersonInfoMinimal(mentee),
                    MenteePreviewStatus.Okay,
                    MenteePreviewStatus.Okay,
                    MenteePreviewStatus.Okay);

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
                       kategorieRuleByWeek.ContainsKey(DateOnly.FromDateTime(week.Start)) &&
                       kategorieRuleByWeek[DateOnly.FromDateTime(week.Start)];
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
    // The param teacher is still here as we need it later for checking if the teacher is allowed to see this termin.
    public async Task<LehrerTermin?> GetTerminForTeacher(Guid terminId, Person teacher)
    {
        var termin = await _dbContext.OtiaTermine
            .Include(t => t.Tutor)
            .Include(t => t.Block)
            .ThenInclude(b => b.Schultag)
            .Include(t => t.Otium)
            .Where(t => !t.IstAbgesagt)
            .FirstOrDefaultAsync(t => t.Id == terminId);

        if (termin is null)
            return null;

        var anwesenheiten = await _attendanceService.GetAttendanceForTerminAsync(terminId);

        Block? currentBlock;
        try
        {
            currentBlock = await _schuljahrService.GetCurrentBlockAsync();
        }
        catch (KeyNotFoundException)
        {
            currentBlock = null;
        }

        var isRunning = currentBlock is not null && currentBlock.Id == termin.Block.Id;

        return new LehrerTermin
        {
            Id = termin.Id,
            Ort = termin.Ort,
            Otium = termin.Otium.Bezeichnung,
            OtiumId = termin.Otium.Id,
            Block = termin.Block.SchemaId,
            BlockId = termin.Block.Id,
            Datum = termin.Block.Schultag.Datum,
            MaxEinschreibungen = termin.MaxEinschreibungen,
            IstAbgesagt = termin.IstAbgesagt,
            IsRunning = isRunning,
            Tutor = termin.Tutor is not null ? new PersonInfoMinimal(termin.Tutor) : null,
            Einschreibungen = anwesenheiten.Select(e =>
                new LehrerEinschreibung(new PersonInfoMinimal(e.Key), e.Value))
        };
    }

    /// <summary>
    ///     Gets all Otia
    /// </summary>
    public IEnumerable<DTO_Otium_View> GetOtia()
    {
        return _dbContext.Otia
            .AsSplitQuery()
            .Include(o => o.Verantwortliche)
            .Include(o => o.Termine).ThenInclude(t => t.Tutor)
            .Include(o => o.Termine).ThenInclude(t => t.Block).ThenInclude(b => b.Schultag)
            .Include(o => o.Wiederholungen).ThenInclude(t => t.Tutor)
            .Include(o => o.Wiederholungen).ThenInclude(t => t.Termine)
            .Include(o => o.Kategorie)
            .OrderBy(o => o.Bezeichnung)
            .ThenByDescending(o => o.Termine.Count)
            .Select(otium => new DTO_Otium_View(otium));
    }

    /// <summary>
    ///     Gets a single Otium
    /// </summary>
    /// <param name="otiumId">The ID of the Otium to get.</param>
    public DTO_Otium_View GetOtium(Guid otiumId)
    {
        var otium = _dbContext.Otia
            .AsSplitQuery()
            .Include(o => o.Verantwortliche)
            .Include(o => o.Termine)
            .ThenInclude(t => t.Tutor)
            .Include(o => o.Termine.OrderBy(t => t.Block.Schultag.Datum).ThenBy(t => t.Block.SchemaId))
            .ThenInclude(t => t.Block)
            .ThenInclude(b => b.Schultag)
            .Include(o => o.Wiederholungen.OrderBy(w => w.Wochentyp).ThenBy(w => w.Wochentyp).ThenBy(w => w.Block))
            .ThenInclude(t => t.Tutor)
            .Include(o => o.Kategorie)
            .FirstOrDefault(o => o.Id == otiumId);

        if (otium is null)
            throw new EntityNotFoundException("Kein Otium mit dieser Id gefunden.");

        return new DTO_Otium_View(otium);
    }

    /// <summary>
    ///     Creates a new Otium
    /// </summary>
    /// <param name="dtoOtium">The Otium data to add</param>
    public async Task<Guid> CreateOtiumAsync(DTO_Otium_Creation dtoOtium)
    {
        var kategorie = await _dbContext.OtiaKategorien.FindAsync(dtoOtium.Kategorie);
        if (kategorie is null)
            throw new ArgumentException("Kategorie must be valid Kategorie");

        var dbOtium = new DB_Otium
        {
            Kategorie = kategorie,
            Bezeichnung = dtoOtium.Bezeichnung,
            Beschreibung = dtoOtium.Beschreibung
        };
        _dbContext.Otia.Add(dbOtium);
        await _dbContext.SaveChangesAsync();

        return dbOtium.Id;
    }

    /// <summary>
    ///     Deletes an Otium
    /// </summary>
    /// <param name="otiumId">The ID of the Otium to delete.</param>
    public async Task DeleteOtiumAsync(Guid otiumId)
    {
        var otium = await _dbContext.Otia
            .Include(o => o.Termine)
            .FirstOrDefaultAsync(o => o.Id == otiumId);
        if (otium is null)
            throw new EntityNotFoundException("Kein Otium mit dieser Id gefunden.");

        var hatEinschreibungen = _dbContext.OtiaEinschreibungen.Any(e => e.Termin.Otium.Id == otiumId);
        if (hatEinschreibungen)
            throw new EntityDeletionException("Otia mit Terminen mit Einschreibungen können nicht gelöscht werden.");

        _dbContext.OtiaTermine.RemoveRange(otium.Termine);

        _dbContext.Otia.Remove(otium);
        await _dbContext.SaveChangesAsync();
    }

    /// <summary>
    ///     Creates an individual OtiumTermin
    /// </summary>
    /// <param name="otiumTermin">The OtiumTermin to create.</param>
    /// <exception cref="ArgumentException">A required argument was not set</exception>
    /// <exception cref="ArgumentNullException">A referenced object could not be found</exception>
    public async Task<Guid> CreateOtiumTerminAsync(DTO_Termin_Creation otiumTermin)
    {
        if (string.IsNullOrWhiteSpace(otiumTermin.Ort))
            throw new ArgumentNullException(nameof(otiumTermin), "Sie müssen einen Ort angeben.");

        var otium = await _dbContext.Otia.FindAsync(otiumTermin.OtiumId);
        if (otium is null)
            throw new ArgumentException("Kein Otium mit dieser Id existiert.");

        var block = await _dbContext.Blocks.FirstOrDefaultAsync(c =>
            c.SchemaId == otiumTermin.Block && c.Schultag.Datum == otiumTermin.Datum);
        if (block is null) throw new ArgumentException("Kein solcher Block existiert.");

        Person? tutor = null;
        if (otiumTermin.Tutor is not null)
        {
            tutor = await _dbContext.Personen.FindAsync(otiumTermin.Tutor);
            if (tutor is null)
                throw new ArgumentException("Kein Tutor mit dieser Id existiert.");
        }

        var conflict = await _dbContext.OtiaTermine.AnyAsync(t =>
            t.Otium.Id == otiumTermin.OtiumId &&
            t.Block.SchemaId == otiumTermin.Block &&
            t.Block.Schultag.Datum == otiumTermin.Datum &&
            t.Tutor == tutor);

        if (conflict)
            throw new ArgumentException("Ein Termin mit diesen Eigenschaften existiert bereits.");

        var dbOtiumTermin = new DB_Termin
        {
            Otium = otium,
            Block = block,
            Ort = otiumTermin.Ort,
            Tutor = tutor,
            MaxEinschreibungen = otiumTermin.MaxEinschreibungen,
            IstAbgesagt = false,
            Wiederholung = null
        };

        _dbContext.OtiaTermine.Add(dbOtiumTermin);
        await _dbContext.SaveChangesAsync();

        return dbOtiumTermin.Id;
    }

    /// <summary>
    ///     Deletes an individual OtiumTermin. Cannot be used on regular Termine.
    /// </summary>
    /// <param name="otiumTerminId">The ID of the OtiumTermin to delete.</param>
    public async Task DeleteOtiumTerminAsync(Guid otiumTerminId)
    {
        var otiumTermin = await _dbContext.OtiaTermine
            .Include(x => x.Enrollments)
            .Include(x => x.Wiederholung)
            .Include(x => x.Otium)
            .FirstOrDefaultAsync(o => o.Id == otiumTerminId);
        if (otiumTermin is null)
            throw new EntityNotFoundException("Kein Termin mit dieser Id");

        if (otiumTermin.Wiederholung is not null)
            throw new EntityDeletionException(
                "Termine aus Wiederholungsregeln können nicht gelöscht werden, sondern nur abgesagt.");

        var hatEinschreibungen = otiumTermin.Enrollments.Count != 0;

        if (hatEinschreibungen)
            throw new EntityDeletionException("Termine mit Einschreibungen können nicht gelöscht werden.");

        _dbContext.OtiaTermine.Remove(otiumTermin);
        await _dbContext.SaveChangesAsync();
    }

    /// <summary>
    ///     Creates an Otiumwiederholung and its OtiumTermine
    /// </summary>
    /// <param name="otiumWiederholung">The Wiederholung to create.</param>
    public async Task<Guid> CreateOtiumWiederholungAsync(DTO_Wiederholung_Creation otiumWiederholung)
    {
        var otium = await _dbContext.Otia.FindAsync(otiumWiederholung.OtiumId);
        if (otium is null)
            throw new ArgumentException("Kein Otium mit dieser Id existiert.");

        Person? tutor = null;
        if (otiumWiederholung.Tutor != null)
        {
            tutor = await _dbContext.Personen.FindAsync(otiumWiederholung.Tutor);
            if (tutor is null)
                throw new ArgumentException("Kein Tutor mit dieser Id existiert.");
        }

        var dbOtiumWiederholung = new DB_Wiederholung
        {
            Otium = otium,
            Block = otiumWiederholung.Block,
            Wochentyp = otiumWiederholung.Wochentyp,
            Wochentag = otiumWiederholung.Wochentag,
            Ort = otiumWiederholung.Ort,
            Tutor = tutor,
            MaxEinschreibungen = otiumWiederholung.MaxEinschreibungen
        };
        _dbContext.OtiaWiederholungen.Add(dbOtiumWiederholung);

        var blocks = _dbContext.Blocks
            .Where(b => b.SchemaId == otiumWiederholung.Block &&
                        b.Schultag.Datum.DayOfWeek == otiumWiederholung.Wochentag &&
                        b.Schultag.Wochentyp == otiumWiederholung.Wochentyp &&
                        b.Schultag.Datum <= otiumWiederholung.EndDate &&
                        b.Schultag.Datum >= otiumWiederholung.StartDate)
            .AsAsyncEnumerable();

        await foreach (var block in blocks)
        {
            var dbOtiumTermin = new DB_Termin
            {
                Otium = otium,
                Ort = dbOtiumWiederholung.Ort,
                Tutor = dbOtiumWiederholung.Tutor,
                Block = block,
                MaxEinschreibungen = otiumWiederholung.MaxEinschreibungen,
                IstAbgesagt = false,
                Wiederholung = dbOtiumWiederholung
            };

            _dbContext.OtiaTermine.Add(dbOtiumTermin);
        }

        await _dbContext.SaveChangesAsync();

        return dbOtiumWiederholung.Id;
    }

    /// <summary>
    ///     Deletes an Otiumwiederholung.
    /// </summary>
    /// <param name="otiumWiederholungId">The ID of the OtiumWiederholung to delete.</param>
    public async Task DeleteOtiumWiederholungAsync(Guid otiumWiederholungId)
    {
        var otiumWiederholung = await _dbContext.OtiaWiederholungen
            .AsSplitQuery()
            .Include(x => x.Otium)
            .Include(x => x.Termine)
            .ThenInclude(t => t.Enrollments)
            .FirstOrDefaultAsync(o => o.Id == otiumWiederholungId);
        if (otiumWiederholung is null)
            throw new EntityNotFoundException("Keine Wiederholung mit dieser Id");

        var hatEinschreibungen = otiumWiederholung.Termine.Any(t => t.Enrollments.Count != 0);
        if (hatEinschreibungen)
            throw new EntityDeletionException(
                "Wiederholungen mit Terminen mit Einschreibungen können nicht gelöscht werden.");

        _dbContext.OtiaTermine.RemoveRange(otiumWiederholung.Termine);

        _dbContext.OtiaWiederholungen.Remove(otiumWiederholung);
        await _dbContext.SaveChangesAsync();
    }

    /// <summary>
    ///     Discontinues an Otiumwiederholung by deleting all termine starting from <paramref name="firstDayAfter" />
    ///     Cancels future termine to ensure that there are not have any enrollments.
    /// </summary>
    /// <param name="otiumWiederholungId">The ID of the OtiumWiederholung to discontinue.</param>
    /// <param name="firstDayAfter">The first date from which on the recurrence will not be scheduled.</param>
    public async Task OtiumWiederholungDiscontinueAsync(Guid otiumWiederholungId, DateOnly firstDayAfter)
    {
        if (firstDayAfter < DateOnly.FromDateTime(DateTime.Today))
            throw new ArgumentException("Das Datum muss in der Zukunft liegen.");

        var otiumWiederholung = await _dbContext.OtiaWiederholungen
            .AsSplitQuery()
            .Include(x => x.Otium)
            .Include(x => x.Termine)
            .ThenInclude(t => t.Enrollments)
            .Include(x => x.Termine.Where(t => t.Block.Schultag.Datum > firstDayAfter))
            .FirstOrDefaultAsync(o => o.Id == otiumWiederholungId);
        if (otiumWiederholung is null)
            throw new EntityNotFoundException("Keine Wiederholung mit dieser Id");

        var termine = otiumWiederholung.Termine.ToList();

        foreach (var t in termine.Where(t => !t.IstAbgesagt))
            await OtiumTerminAbsagenAsync(t.Id);

        _dbContext.OtiaTermine.RemoveRange(termine);
        await _dbContext.SaveChangesAsync();
    }

    /// <summary>
    ///     Cancels an OtiumTermin.
    /// </summary>
    /// <param name="otiumTerminId">The ID of the OtiumTermin to cancel.</param>
    public async Task OtiumTerminAbsagenAsync(Guid otiumTerminId)
    {
        var otiumTermin = await _dbContext.OtiaTermine
            .AsSplitQuery()
            .Include(x => x.Enrollments).ThenInclude(e => e.BetroffenePerson)
            .Include(x => x.Wiederholung)
            .Include(x => x.Block).ThenInclude(b => b.Schultag)
            .Include(x => x.Otium)
            .FirstOrDefaultAsync(o => o.Id == otiumTerminId);
        if (otiumTermin is null)
            throw new EntityNotFoundException("Kein Termin mit dieser Id");

        if (otiumTermin.IstAbgesagt)
            return;

        // Delete existing enrollments
        var einschreibungen = otiumTermin.Enrollments;

        var teilnehmer = einschreibungen.Select(oe => oe.BetroffenePerson).ToList();

        _dbContext.OtiaEinschreibungen.RemoveRange(einschreibungen);
        otiumTermin.IstAbgesagt = true;
        await _dbContext.SaveChangesAsync();

        // Notify previously enrolled students
        foreach (var t in teilnehmer)
            await _emailOutbox.ScheduleNotificationAsync(
                t,
                "Otium Termin abgesagt",
                $"""
                 Das Otium {otiumTermin.Otium.Bezeichnung} wurde
                 am {otiumTermin.Block.Schultag.Datum} abgesagt.
                 Schreibe dich gegebenenfalls um.
                 """,
                TimeSpan.FromSeconds(30)
            );
    }

    /// <summary>
    ///     Sets the Bezeichnung of an OtiumTermin
    /// </summary>
    /// <param name="otiumId">The ID of the Otium to change the Bezeichnung of.</param>
    /// <param name="bezeichnung">The new Bezeichnung</param>
    public async Task OtiumSetBezeichnungAsync(Guid otiumId, string bezeichnung)
    {
        var otium = await _dbContext.Otia.FindAsync(otiumId);
        if (otium is null) throw new ArgumentException("Kein Otium mit dieser Id existiert.");

        otium.Bezeichnung = bezeichnung.Trim();
        await _dbContext.SaveChangesAsync();
    }

    /// <summary>
    ///     Sets the Beschreibung of an OtiumTermin
    /// </summary>
    /// <param name="otiumId">The ID of the Otium to change the Beschreibung> of.</param>
    /// <param name="beschreibung">The new Beschreibung</param>
    public async Task OtiumSetBeschreibungAsync(Guid otiumId, string beschreibung)
    {
        var otium = await _dbContext.Otia.FindAsync(otiumId);
        if (otium is null)
            throw new ArgumentException("Kein Otium mit dieser Id existiert.");

        var beschreibungBuilder = new StringBuilder();
        foreach (var line in beschreibung.Split('\n'))
            if (!string.IsNullOrWhiteSpace(line))
                beschreibungBuilder.AppendLine(line.Trim());

        otium.Beschreibung = beschreibungBuilder.ToString();
        await _dbContext.SaveChangesAsync();
    }

    /// <summary>
    ///     Adds a Person as Verantwortlich for the given Otium
    /// </summary>
    /// <param name="otiumId">The ID of the Otium to add the Person on.</param>
    /// <param name="persId">The ID new Verantwortliche Person</param>
    public async Task OtiumAddVerantwortlichAsync(Guid otiumId, Guid persId)
    {
        var otium = await _dbContext.Otia.FindAsync(otiumId);
        if (otium is null)
            throw new ArgumentException("Kein Otium mit dieser Id existiert.");

        var person = await _dbContext.Personen.FindAsync(persId);
        if (person is null)
            throw new ArgumentException("Keine Person mit dieser Id existiert.");

        otium.Verantwortliche.Add(person);
        await _dbContext.SaveChangesAsync();
    }

    /// <summary>
    ///     Removes a Person as Verantwortlich for the given Otium
    /// </summary>
    /// <param name="otiumId">The ID of the Otium to remove the Person on.</param>
    /// <param name="persId">The ID new Verantwortliche Person</param>
    public async Task OtiumRemoveVerantwortlichAsync(Guid otiumId, Guid persId)
    {
        var otium = await _dbContext.Otia.FindAsync(otiumId);
        if (otium is null)
            throw new ArgumentException("Kein Otium mit dieser Id existiert.");

        var person = await _dbContext.Personen.FindAsync(persId);
        if (person is null)
            throw new ArgumentException("Keine Person mit dieser Id existiert.");

        otium.Verantwortliche.Remove(person);
        await _dbContext.SaveChangesAsync();
    }

    /// <summary>
    ///     Sets the Beschreibung of an Kategorie
    /// </summary>
    /// <param name="otiumId">The ID of the Otium to change the Kategorie of.</param>
    /// <param name="kategorieId">The Id of the new Kategorie</param>
    /// TODO : Implement proper constraints
    public async Task OtiumSetKategorieAsync(Guid otiumId, Guid kategorieId)
    {
        var otium = await _dbContext.Otia
            .Include(o => o.Kategorie)
            .FirstOrDefaultAsync(o => o.Id == otiumId);
        if (otium is null) throw new ArgumentException("Kein Otium mit dieser Id existiert.");

        var kategorie = await _dbContext.OtiaKategorien.FindAsync(kategorieId);
        if (kategorie is null) throw new ArgumentException("Keine Kategorie mit dieser Id existiert.");

        if (otium.Kategorie.Required && !kategorie.Required) throw new InvalidOperationException();

        otium.Kategorie = kategorie;
        await _dbContext.SaveChangesAsync();
    }

    /// <summary>
    ///     Sets the maxEinschreibungen of an OtiumTermin.
    /// </summary>
    /// <param name="otiumTerminId">The ID of the OtiumTermin to set maxEinschreibungen on.</param>
    /// <param name="maxEinschreibungen">The new value of MaxEinschreibungen.</param>
    public async Task OtiumTerminSetMaxEinschreibungenAsync(Guid otiumTerminId, int? maxEinschreibungen)
    {
        var otiumTermin = await _dbContext.OtiaTermine
            .Include(x => x.Enrollments).ThenInclude(e => e.BetroffenePerson).Include(termin => termin.Otium)
            .Include(termin => termin.Block).ThenInclude(block => block.Schultag)
            .FirstOrDefaultAsync(o => o.Id == otiumTerminId);
        if (otiumTermin is null)
            throw new EntityNotFoundException("Kein Termin mit dieser Id");

        if (maxEinschreibungen <= 0)
            throw new InvalidOperationException("maxEinschreibungen needs to greater than zero");

        // The first part of the expression is not strictly necessary as int > null is always false. It is here just for clarity.
        if (maxEinschreibungen is not null && otiumTermin.Enrollments.Count > maxEinschreibungen)
        {
            // kick some attendees from the list
            var enrollments = otiumTermin.Enrollments.ToArray();
            Random.Shared.Shuffle(enrollments);

            var enrollmentsToCancel = enrollments[maxEinschreibungen.Value..];
            var attendeesToCancel = enrollmentsToCancel.Select(oe => oe.BetroffenePerson).ToList();

            _dbContext.OtiaEinschreibungen.RemoveRange(enrollmentsToCancel);
            await _dbContext.SaveChangesAsync();

            // Notify previously enrolled students
            foreach (var t in attendeesToCancel)
                await _emailOutbox.ScheduleNotificationAsync(
                    t,
                    "Otium Einschreibung Abgesagt",
                    $"""
                     Für das Otium {otiumTermin.Otium.Bezeichnung} wurde
                     am {otiumTermin.Block.Schultag.Datum} die Teilnehmerbegrenzung reduziert.
                     Deine Einschreibung wurde nach dem Losverfahren gelöscht. Schreibe dich bitte neu ein.
                     """,
                    TimeSpan.FromSeconds(30)
                );
        }

        otiumTermin.MaxEinschreibungen = maxEinschreibungen;
        await _dbContext.SaveChangesAsync();
    }

    /// <summary>
    ///     Sets the tutor of an OtiumTermin.
    /// </summary>
    /// <param name="otiumTerminId">The ID of the OtiumTermin to set the tutor on.</param>
    /// <param name="personId">The new tutor.</param>
    public async Task OtiumTerminSetTutorAsync(Guid otiumTerminId, Guid? personId)
    {
        var otiumTermin = await _dbContext.OtiaTermine
            .Include(t => t.Tutor)
            .FirstOrDefaultAsync(t => t.Id == otiumTerminId);

        if (otiumTermin is null)
            throw new EntityNotFoundException("Kein Termin mit dieser Id");

        Person? person = null;
        if (personId.HasValue)
        {
            person = await _dbContext.Personen.FindAsync(personId);
            if (person is null)
                throw new EntityNotFoundException("Keine Person mit dieser Id");
        }

        otiumTermin.Tutor = person;
        await _dbContext.SaveChangesAsync();
    }

    /// <summary>
    ///     Sets the ort of an OtiumTermin.
    /// </summary>
    /// <param name="otiumTerminId">The ID of the OtiumTermin to set the ort on.</param>
    /// <param name="ort">The new ort.</param>
    public async Task OtiumTerminSetOrtAsync(Guid otiumTerminId, string ort)
    {
        var otiumTermin = await _dbContext.OtiaTermine
            .FindAsync(otiumTerminId);
        if (otiumTermin is null)
            throw new EntityNotFoundException("Kein Termin mit dieser Id");

        otiumTermin.Ort = ort;
        await _dbContext.SaveChangesAsync();
    }

    private class TerminWithLoad
    {
        public required int? Auslasung { get; init; }
        public required DB_Termin Termin { get; init; }
    }

    /// <summary>
    ///     An Exception thrown when the Entity to operate on was not found
    /// </summary>
    public class EntityNotFoundException : InvalidOperationException
    {
        /// <summary>
        ///     Constructs a new EntityNotFoundException
        /// </summary>
        public EntityNotFoundException(string message) : base(message)
        {
        }
    }

    /// <summary>
    ///     An Exception thrown when the request failed because it breaks logical constraints
    /// </summary>
    public class EntityDeletionException : InvalidOperationException
    {
        /// <summary>
        ///     Constructs a new EntityDeletionException
        /// </summary>
        public EntityDeletionException(string message) : base(message)
        {
        }
    }
}
