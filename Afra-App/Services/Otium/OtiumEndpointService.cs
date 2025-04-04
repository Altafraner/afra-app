using System.Text;
using Afra_App.Data;
using Afra_App.Data.Configuration;
using Afra_App.Data.DTO;
using Afra_App.Data.DTO.Otium;
using Afra_App.Data.People;
using Afra_App.Data.TimeInterval;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using DB_Otium = Afra_App.Data.Otium.Otium;
using DB_Termin = Afra_App.Data.Otium.Termin;
using DB_Wiederholung = Afra_App.Data.Otium.Wiederholung;
using DTO_Otium_Creation = Afra_App.Data.DTO.Otium.ManagementOtiumCreation;
using DTO_Otium_View = Afra_App.Data.DTO.Otium.ManagementOtiumView;
using DTO_Termin_Creation = Afra_App.Data.DTO.Otium.ManagementTerminCreation;
using DTO_Wiederholung_Creation = Afra_App.Data.DTO.Otium.ManagementWiederholungCreation;
using Person = Afra_App.Data.People.Person;

namespace Afra_App.Services.Otium;

/// <summary>
///     A Service for handling requests to the Otium endpoint.
/// </summary>
public class OtiumEndpointService
{
    private readonly IBatchingEmailService _batchingEmailService;
    private readonly AfraAppContext _context;
    private readonly EnrollmentService _enrollmentService;
    private readonly KategorieService _kategorieService;
    private readonly OtiumConfiguration _otiumConfiguration;

    /// <summary>
    ///     Constructor for the OtiumEndpointService. Usually called by the DI container.
    /// </summary>
    public OtiumEndpointService(AfraAppContext context, KategorieService kategorieService,
        IOptions<OtiumConfiguration> otiumConfiguration, EnrollmentService enrollmentService,
        IBatchingEmailService batchingEmailService)
    {
        _context = context;
        _kategorieService = kategorieService;
        _enrollmentService = enrollmentService;
        _otiumConfiguration = otiumConfiguration.Value;
        _batchingEmailService = batchingEmailService;
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

        //if (termin?.Tutor is null || termin.Tutor.Id != teacher.Id) return null;
        if (teacher.Rolle != Rolle.Tutor)
            return null;

        if (termin is null)
            return null;

        return new LehrerTermin
        {
            Id = termin.Id,
            Ort = termin.Ort,
            Otium = termin.Otium.Bezeichnung,
            Block = termin.Block.Nummer,
            Datum = termin.Block.Schultag.Datum,
            MaxEinschreibungen = termin.MaxEinschreibungen,
            IstAbgesagt = termin.IstAbgesagt,
            Tutor = termin.Tutor is not null ? new PersonInfoMinimal(termin.Tutor) : null,
            Einschreibungen = termin.Enrollments.Select(e =>
                new LehrerEinschreibung(new PersonInfoMinimal(e.BetroffenePerson), e.Interval,
                    AnwesenheitsStatus.Anwesend))
        };
    }

    /// <summary>
    ///     Gets all Otia
    /// </summary>
    public IEnumerable<DTO_Otium_View> GetOtia()
    {
        return _context.Otia
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
    /// <param name="otiumId">The Id of the Otium to get.</param>
    public DTO_Otium_View GetOtium(Guid otiumId)
    {
        var otium = _context.Otia
            .AsSplitQuery()
            .Include(o => o.Verantwortliche)
            .Include(o => o.Termine)
            .ThenInclude(t => t.Tutor)
            .Include(o => o.Termine.OrderBy(t => t.Block.Schultag.Datum).ThenBy(t => t.Block.Nummer))
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
        var kategorie = await _context.OtiaKategorien.FindAsync(dtoOtium.Kategorie);
        if (kategorie is null)
            throw new ArgumentException("Kategorie must be valid Kategorie");

        var dbOtium = new DB_Otium
        {
            Kategorie = kategorie,
            Bezeichnung = dtoOtium.Bezeichnung,
            Beschreibung = dtoOtium.Beschreibung
        };
        _context.Otia.Add(dbOtium);
        await _context.SaveChangesAsync();

        return dbOtium.Id;
    }

    /// <summary>
    ///     Deletes an Otium
    /// </summary>
    /// <param name="otiumId">The Id of the Otium to delete.</param>
    public async Task DeleteOtiumAsync(Guid otiumId)
    {
        var otium = await _context.Otia
            .Include(o => o.Termine)
            .FirstOrDefaultAsync(o => o.Id == otiumId);
        if (otium is null)
            throw new EntityNotFoundException("Kein Otium mit dieser Id gefunden.");

        var hatEinschreibungen = _context.OtiaEinschreibungen.Any(e => e.Termin.Otium.Id == otiumId);
        if (hatEinschreibungen)
            throw new EntityDeletionException("Otia mit Terminen mit Einschreibungen können nicht gelöscht werden.");

        _context.OtiaTermine.RemoveRange(otium.Termine);

        _context.Otia.Remove(otium);
        await _context.SaveChangesAsync();
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

        var otium = await _context.Otia.FindAsync(otiumTermin.otiumId);
        if (otium is null)
            throw new ArgumentException("Kein Otium mit dieser Id existiert.");

        var block = await _context.Blocks.FirstOrDefaultAsync(c =>
            c.Nummer == otiumTermin.Block && c.Schultag.Datum == otiumTermin.Datum);
        if (block is null)
        {
            throw new ArgumentException("Kein solcher Block existiert.");
        }

        Person? tutor = null;
        if (otiumTermin.Tutor is not null)
        {
            tutor = await _context.Personen.FindAsync(otiumTermin.Tutor);
            if (tutor is null)
                throw new ArgumentException("Kein Tutor mit dieser Id existiert.");
        }

        var conflict = await _context.OtiaTermine.AnyAsync(t =>
            t.Otium.Id == otiumTermin.otiumId &&
            t.Block.Nummer == otiumTermin.Block &&
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

        _context.OtiaTermine.Add(dbOtiumTermin);
        await _context.SaveChangesAsync();

        return dbOtiumTermin.Id;
    }

    /// <summary>
    ///     Deletes an individual OtiumTermin. Cannot be used on regular Termine.
    /// </summary>
    /// <param name="otiumTerminId">The Id of the OtiumTermin to delete.</param>
    public async Task DeleteOtiumTerminAsync(Guid otiumTerminId)
    {
        var otiumTermin = await _context.OtiaTermine
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

        _context.OtiaTermine.Remove(otiumTermin);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    ///     Creates an Otiumwiederholung and its OtiumTermine
    /// </summary>
    /// <param name="otiumWiederholung">The Wiederholung to create.</param>
    public async Task<Guid> CreateOtiumWiederholungAsync(DTO_Wiederholung_Creation otiumWiederholung)
    {
        var otium = await _context.Otia.FindAsync(otiumWiederholung.otiumId);
        if (otium is null)
            throw new ArgumentException("Kein Otium mit dieser Id existiert.");

        Person? tutor = null;
        if (otiumWiederholung.Tutor != null)
        {
            tutor = await _context.Personen.FindAsync(otiumWiederholung.Tutor);
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
        _context.OtiaWiederholungen.Add(dbOtiumWiederholung);

        var blocks = _context.Blocks
            .Where(b => b.Nummer == otiumWiederholung.Block &&
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

            _context.OtiaTermine.Add(dbOtiumTermin);
        }

        await _context.SaveChangesAsync();

        return dbOtiumWiederholung.Id;
    }

    /// <summary>
    ///     Deletes an Otiumwiederholung.
    /// </summary>
    /// <param name="otiumWiederholungId">The Id of the OtiumWiederholung to delete.</param>
    public async Task DeleteOtiumWiederholungAsync(Guid otiumWiederholungId)
    {
        var otiumWiederholung = await _context.OtiaWiederholungen
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

        _context.OtiaWiederholungen.Remove(otiumWiederholung);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    ///     Cancels an OtiumTermin.
    /// </summary>
    /// <param name="otiumTerminId">The Id of the OtiumTermin to cancel.</param>
    public async Task OtiumTerminAbsagenAsync(Guid otiumTerminId)
    {
        var otiumTermin = await _context.OtiaTermine
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

        _context.OtiaEinschreibungen.RemoveRange(einschreibungen);
        otiumTermin.IstAbgesagt = true;
        await _context.SaveChangesAsync();

        // Notify previously enrolled students
        foreach (var t in teilnehmer)
            await _batchingEmailService.ScheduleEmailAsync(
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
    /// <param name="otiumId">The Id of the Otium to change the Bezeichnung of.</param>
    /// <param name="bezeichnung">The new Bezeichnung</param>
    public async Task OtiumSetBezeichnungAsync(Guid otiumId, string bezeichnung)
    {
        var otium = await _context.Otia.FindAsync(otiumId);
        if (otium is null) throw new ArgumentException("Kein Otium mit dieser Id existiert.");

        otium.Bezeichnung = bezeichnung.Trim();
        await _context.SaveChangesAsync();
    }

    /// <summary>
    ///     Sets the Beschreibung of an OtiumTermin
    /// </summary>
    /// <param name="otiumId">The Id of the Otium to change the Beschreibung> of.</param>
    /// <param name="beschreibung">The new Beschreibung</param>
    public async Task OtiumSetBeschreibungAsync(Guid otiumId, string beschreibung)
    {
        var otium = await _context.Otia.FindAsync(otiumId);
        if (otium is null)
            throw new ArgumentException("Kein Otium mit dieser Id existiert.");

        var beschreibungBuilder = new StringBuilder();
        foreach (var line in beschreibung.Split('\n'))
            if (!string.IsNullOrWhiteSpace(line))
                beschreibungBuilder.AppendLine(line.Trim());

        otium.Beschreibung = beschreibungBuilder.ToString();
        await _context.SaveChangesAsync();
    }

    /// <summary>
    ///     Adds a Person as Verantwortlich for the given Otium
    /// </summary>
    /// <param name="otiumId">The Id of the Otium to add the Person on.</param>
    /// <param name="persId">The Id new Verantwortliche Person</param>
    public async Task OtiumAddVerantwortlichAsync(Guid otiumId, Guid persId)
    {
        var otium = await _context.Otia.FindAsync(otiumId);
        if (otium is null)
            throw new ArgumentException("Kein Otium mit dieser Id existiert.");

        var person = await _context.Personen.FindAsync(persId);
        if (person is null)
            throw new ArgumentException("Keine Person mit dieser Id existiert.");

        otium.Verantwortliche.Add(person);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    ///     Removes a Person as Verantwortlich for the given Otium
    /// </summary>
    /// <param name="otiumId">The Id of the Otium to remove the Person on.</param>
    /// <param name="persId">The Id new Verantwortliche Person</param>
    public async Task OtiumRemoveVerantwortlichAsync(Guid otiumId, Guid persId)
    {
        var otium = await _context.Otia.FindAsync(otiumId);
        if (otium is null)
            throw new ArgumentException("Kein Otium mit dieser Id existiert.");

        var person = await _context.Personen.FindAsync(persId);
        if (person is null)
            throw new ArgumentException("Keine Person mit dieser Id existiert.");

        otium.Verantwortliche.Remove(person);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    ///     Sets the Beschreibung of an Kategorie
    /// </summary>
    /// <param name="otiumId">The Id of the Otium to change the Kategorie of.</param>
    /// <param name="kategorieId">The Id of the new Kategorie</param>
    /// TODO : Implement proper constraints
    public async Task OtiumSetKategorieAsync(Guid otiumId, Guid kategorieId)
    {
        var otium = await _context.Otia
            .Include(o => o.Kategorie)
            .FirstOrDefaultAsync(o => o.Id == otiumId);
        if (otium is null)
        {
            throw new ArgumentException("Kein Otium mit dieser Id existiert.");
        }

        var kategorie = await _context.OtiaKategorien.FindAsync(kategorieId);
        if (kategorie is null)
        {
            throw new ArgumentException("Keine Kategorie mit dieser Id existiert.");
        }

        if (otium.Kategorie.Required && !kategorie.Required)
        {
            throw new InvalidOperationException();
        }

        otium.Kategorie = kategorie;
        await _context.SaveChangesAsync();
    }

    /// <summary>
    ///     Sets the maxEinschreibungen of an OtiumTermin.
    /// </summary>
    /// <param name="otiumTerminId">The Id of the OtiumTermin to set maxEinschreibungen on.</param>
    /// <param name="maxEinschreibungen">The new value of MaxEinschreibungen.</param>
    public async Task OtiumTerminSetMaxEinschreibungenAsync(Guid otiumTerminId, int? maxEinschreibungen)
    {
        var otiumTermin = await _context.OtiaTermine
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

            _context.OtiaEinschreibungen.RemoveRange(enrollmentsToCancel);
            await _context.SaveChangesAsync();

            // Notify previously enrolled students
            foreach (var t in attendeesToCancel)
            {
                await _batchingEmailService.ScheduleEmailAsync(
                    t,
                    $"Otium Einschreibung Abgesagt",
                    $"""
                     Für das Otium {otiumTermin.Otium.Bezeichnung} wurde
                     am {otiumTermin.Block.Schultag.Datum} die Teilnehmerbegrenzung reduziert.
                     Deine Einschreibung wurde nach dem Losverfahren gelöscht. Schreibe dich bitte neu ein.
                     """,
                    TimeSpan.FromSeconds(30)
                );
            }
        }

        otiumTermin.MaxEinschreibungen = maxEinschreibungen;
        await _context.SaveChangesAsync();
    }

    /// <summary>
    ///     Sets the tutor of an OtiumTermin.
    /// </summary>
    /// <param name="otiumTerminId">The Id of the OtiumTermin to set the tutor on.</param>
    /// <param name="personId">The new tutor.</param>
    public async Task OtiumTerminSetTutorAsync(Guid otiumTerminId, Guid personId)
    {
        var otiumTermin = await _context.OtiaTermine
            .FindAsync(otiumTerminId);
        if (otiumTermin is null)
            throw new EntityNotFoundException("Kein Termin mit dieser Id");

        var person = await _context.Personen.FindAsync(personId);
        if (otiumTermin is null)
            throw new EntityNotFoundException("Keine Person mit dieser Id");

        otiumTermin.Tutor = person;
        await _context.SaveChangesAsync();
    }

    /// <summary>
    ///     Sets the ort of an OtiumTermin.
    /// </summary>
    /// <param name="otiumTerminId">The Id of the OtiumTermin to set the ort on.</param>
    /// <param name="ort">The new ort.</param>
    public async Task OtiumTerminSetOrtAsync(Guid otiumTerminId, String ort)
    {
        var otiumTermin = await _context.OtiaTermine
            .FindAsync(otiumTerminId);
        if (otiumTermin is null)
            throw new EntityNotFoundException("Kein Termin mit dieser Id");

        otiumTermin.Ort = ort;
        await _context.SaveChangesAsync();
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