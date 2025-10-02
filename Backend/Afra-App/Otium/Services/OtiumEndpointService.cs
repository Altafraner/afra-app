using System.Security.Claims;
using System.Text;
using Altafraner.AfraApp.Backbone.Domain.TimeInterval;
using Altafraner.AfraApp.Backbone.Email.Services.Contracts;
using Altafraner.AfraApp.Backbone.Utilities;
using Altafraner.AfraApp.Otium.Domain.Contracts.Services;
using Altafraner.AfraApp.Otium.Domain.DTO;
using Altafraner.AfraApp.Otium.Domain.DTO.Dashboard;
using Altafraner.AfraApp.Otium.Domain.DTO.Katalog;
using Altafraner.AfraApp.Otium.Domain.Models;
using Altafraner.AfraApp.Schuljahr.Domain.Models;
using Altafraner.AfraApp.User.Domain.DTO;
using Altafraner.AfraApp.User.Domain.Models;
using Altafraner.AfraApp.User.Services;
using Microsoft.EntityFrameworkCore;
using Katalog_Termin = Altafraner.AfraApp.Otium.Domain.DTO.Katalog.Termin;
using Models_Person = Altafraner.AfraApp.User.Domain.Models.Person;

namespace Altafraner.AfraApp.Otium.Services;

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
    private readonly RulesValidationService _rulesValidationService;
    private readonly UserService _userService;

    /// <summary>
    ///     Constructor for the OtiumEndpointService. Usually called by the DI container.
    /// </summary>
    public OtiumEndpointService(AfraAppContext dbContext, KategorieService kategorieService,
        EnrollmentService enrollmentService, IEmailOutbox emailOutbox, BlockHelper blockHelper,
        IAttendanceService attendanceService, UserService userService, RulesValidationService rulesValidationService)
    {
        _dbContext = dbContext;
        _kategorieService = kategorieService;
        _enrollmentService = enrollmentService;
        _emailOutbox = emailOutbox;
        _blockHelper = blockHelper;
        _attendanceService = attendanceService;
        _userService = userService;
        _rulesValidationService = rulesValidationService;
    }

    /// <summary>
    ///     Gets the Katalog for a given date.
    /// </summary>
    /// <param name="person">The person the generate the messages for</param>
    /// <param name="date">The date to get the <see cref="TerminPreview" />s for</param>
    public async Task<Tag> GetKatalogForDay(Models_Person person, DateOnly date)
    {
        return new Tag(GetTerminPreviewsForDay(date, person),
            person.Rolle != Rolle.Oberstufe ? await GetStatusForDayAsync(person, date) : []);
    }

    /// <summary>
    ///     Retrieves the Otium data for a given date.
    /// </summary>
    /// <param name="date">The date for which to retrieve the Otium data.</param>
    /// <param name="user">The user the preview ist for</param>
    /// <returns>A List of all Otia happening at that time.</returns>
    private async IAsyncEnumerable<TerminPreview> GetTerminPreviewsForDay(DateOnly date, Models_Person user)
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
            .ThenBy(t => t.OverrideBezeichnung != null ? t.OverrideBezeichnung : t.Otium.Bezeichnung)
            .Select(t => new TerminWithLoad
            {
                Termin = t,
                Auslasung = t.MaxEinschreibungen == null
                    ? null
                    : (int)Math.Round((double)t.Enrollments.Count * 100 / t.MaxEinschreibungen.Value),
                IstEingeschrieben = t.Enrollments.Any(e => e.BetroffenePerson.Id == user.Id)
            })
            .ToListAsync();


        // Calculate the load for each termin and cast it to a json object
        foreach (var termin in termine)
            yield return new TerminPreview(termin.Termin,
                termin.Auslasung,
                termin.IstEingeschrieben,
                _kategorieService.GetTransitiveKategoriesIdsAsyncEnumerable(termin.Termin.Otium.Kategorie),
                _blockHelper.Get(termin.Termin.Block.SchemaId)!.Bezeichnung);
    }

    /// <summary>
    ///     Get the details of a termin.
    /// </summary>
    /// <param name="terminId">The <see cref="Guid" /> of the termin to get details for</param>
    /// <param name="user">The user requesting the termin</param>
    public async Task<Katalog_Termin?> GetTerminAsync(Guid terminId, Models_Person user)
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

        var schema = _blockHelper.Get(termin.Block.SchemaId)!;

        return new Katalog_Termin(termin,
            await _enrollmentService.GetEnrolmentPreview(user, termin),
            termin.Otium.Kategorie.Id,
            schema.Interval.Start,
            schema.Bezeichnung);
    }

    private async Task<IEnumerable<string>> GetStatusForDayAsync(Models_Person user, DateOnly date)
    {
        List<string> messages = [];

        var weekStart = date.GetStartOfWeek();
        var weekEnd = weekStart.AddDays(7);

        // Get all blocks for the given week
        var schultage = await _dbContext.Schultage
            .Where(s => s.Datum >= weekStart && s.Datum < weekEnd)
            .Include(s => s.Blocks
                .OrderBy(b => b.SchultagKey)
                .ThenBy(b => b.SchemaId)
            )
            .ToListAsync();
        var blocks = schultage.SelectMany(s => s.Blocks);

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
        var datesEnrollments = weeksEnrollments.Where(e => e.Termin.Block.SchultagKey == date).ToList();

        messages.AddRange(await _rulesValidationService.GetMessagesForEnrollmentsAsync(user, datesEnrollments));
        messages.AddRange(await _rulesValidationService.GetMessagesForDayAsync(user,
            schultage.First(s => s.Datum == date),
            datesEnrollments));
        messages.AddRange(await _rulesValidationService.GetMessagesForWeekAsync(user, schultage, weeksEnrollments));

        return messages;
    }

    /// <summary>
    ///     Generates the dashboard for a student.
    /// </summary>
    /// <param name="user">The student to generate the dashboard for</param>
    /// <param name="all">Iff true, all available school-days are included. Otherwise, just the current and next two weeks.</param>
    // I hate myself for writing this mess of a method. Have fun!
    public async IAsyncEnumerable<Week> GetStudentDashboardAsyncEnumerable(Models_Person user,
        bool all)
    {
        var thisMonday = DateOnly.FromDateTime(DateTime.Today).GetStartOfWeek();

        var startDate = thisMonday;
        var endDate = startDate.AddDays(7 * 3);

        IQueryable<Schultag> schultageQuery = _dbContext.Schultage
            .Include(s => s.Blocks)
            .OrderBy(s => s.Datum);
        if (!all) schultageQuery = schultageQuery.Where(s => s.Datum >= startDate && s.Datum < endDate);

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

        var weeks = schultage.GroupBy(s => s.Datum.GetStartOfWeek());

        foreach (var week in weeks)
        {
            // Increase performance by taking from the already sorted list of enrollments, then removing them from the list before the next iteration.
            var weekEnd = week.Key.AddDays(7);
            var einschreibungenForWeek = einschreibungen
                .TakeWhile(e => e.Termin.Block.SchultagKey < weekEnd)
                .ToList();
            einschreibungen.RemoveRange(0, einschreibungenForWeek.Count);

            var messageBuilder = new StringBuilder();

            foreach (var schultag in week)
            {
                var messagesForBlocksOnDay =
                    await _rulesValidationService.GetMessagesForDayAsync(user, schultag, einschreibungenForWeek);
                if (messagesForBlocksOnDay.Count == 0) continue;
                messageBuilder.AppendLine();
                messageBuilder.AppendLine($"**{schultag.Datum:dddd, dd.MM.yyyy}**");
                foreach (var message in messagesForBlocksOnDay)
                    messageBuilder.AppendLine($"- {message}");
            }

            var messagesForWeek =
                await _rulesValidationService.GetMessagesForWeekAsync(user, week.ToList(), einschreibungenForWeek);
            if (messagesForWeek.Count > 0)
            {
                if (messageBuilder.Length > 0) messageBuilder.AppendLine();
                messageBuilder.AppendLine("**Wöchentliche Bedingungen**");
                foreach (var message in messagesForWeek)
                    messageBuilder.AppendLine($"- {message}");
            }

            yield return new Week(
                week.Key,
                messageBuilder.ToString(),
                await GenerateDtosWithPlaceholders(einschreibungenForWeek, week.ToList(), user)
            );
        }
    }

    /// <summary>
    ///     Generates DTOs for the enrollments, including placeholders for unenrolled blocks.
    /// </summary>
    /// <param name="enrollments">The enrollments in the week</param>
    /// <param name="schooldays">All schooldays with block in the week</param>
    /// <param name="user">The user whose enrollments are given</param>
    /// <returns>
    ///     An enumerable containing DTOs for all enrollments and all unenrolled blocks. DTOs for unenrolled blocks only
    ///     include date and block information. The sorting is stable.
    /// </returns>
    private async Task<IEnumerable<Einschreibung>> GenerateDtosWithPlaceholders(
        List<OtiumEinschreibung> enrollments, List<Schultag> schooldays, Models_Person user)
    {
        var allBlocks = schooldays.SelectMany(s => s.Blocks).ToList();

        var blocksUnenrolled =
            _enrollmentService.GetNotEnrolledBlocks(enrollments, allBlocks);

        var blocksDoneOrRunning = user.Rolle == Rolle.Mittelstufe
            ? allBlocks.Where(_blockHelper.IsBlockDoneOrRunning).Select(b => b.Id).ToHashSet()
            : [];
        var attendances = user.Rolle == Rolle.Mittelstufe
            ? await _attendanceService.GetAttendanceForBlocksAsync(blocksDoneOrRunning, user.Id)
            : [];

        var additionalEnrollments = blocksUnenrolled.Select(b => (b.SchemaId, new Einschreibung
        {
            Datum = b.SchultagKey,
            Block = _blockHelper.Get(b.SchemaId)!.Bezeichnung,
            Anwesenheit = blocksDoneOrRunning.Contains(b.Id) ? attendances[b.Id] : null
        }));

        return enrollments.Select(e => (e.Termin.Block.SchemaId, new Einschreibung
        {
            Block = _blockHelper.Get(e.Termin.Block.SchemaId)!.Bezeichnung,
            Datum = e.Termin.Block.SchultagKey,
            KategorieId = e.Termin.Otium.Kategorie.Id,
            Ort = e.Termin.Ort,
            Otium = e.Termin.Bezeichnung,
            TerminId = e.Termin.Id,
            Anwesenheit = blocksDoneOrRunning.Contains(e.Termin.Block.Id) ? attendances[e.Termin.Block.Id] : null
        }))
            .Concat(additionalEnrollments)
            .OrderBy(e => e.Item2.Datum)
            .ThenBy(e => e.SchemaId)
            .Select(e => e.Item2);
    }

    /// <summary>
    ///     Returns an overview of termine and mentees for a teacher.
    /// </summary>
    public async Task<LehrerUebersicht> GetTeacherDashboardAsync(Models_Person user)
    {
        var startDate = DateOnly.FromDateTime(DateTime.Today).GetStartOfWeek().AddDays(-7);
        var endDate = startDate.AddDays(21);

        var mentees = (await _userService.GetMenteesAsync(user))
            .OrderBy(s => s.Rolle switch
            {
                Rolle.Mittelstufe => 0,
                Rolle.Oberstufe => 1,
                _ => -1,
            })
            .ThenBy(s => s.Vorname)
            .ThenBy(s => s.Nachname);

        var menteesEnrollments = await _dbContext.OtiaEinschreibungen
            .Where(e => mentees.Contains(e.BetroffenePerson))
            .Where(e => e.Termin.Block.Schultag.Datum >= startDate && e.Termin.Block.Schultag.Datum < endDate)
            .Include(p => p.Termin)
            .ThenInclude(p => p.Block)
            .ThenInclude(b => b.Schultag)
            .Include(e => e.Termin)
            .ThenInclude(t => t.Otium)
            .ThenInclude(o => o.Kategorie)
            .GroupBy(e => e.BetroffenePerson.Id)
            .ToDictionaryAsync(e => e.Key, e => e.AsEnumerable());

        var schultage = await _dbContext.Schultage
            .Include(s => s.Blocks)
            .Where(s => s.Datum >= startDate && s.Datum < endDate)
            .ToListAsync();

        List<MenteePreview> menteePreviews = [];

        var lastWeekInterval = new DateTimeInterval(startDate.ToDateTime(new TimeOnly(0, 0)), TimeSpan.FromDays(7));
        var thisWeekInterval = new DateTimeInterval(lastWeekInterval.End, TimeSpan.FromDays(7));
        var nextWeekInterval = new DateTimeInterval(thisWeekInterval.End, TimeSpan.FromDays(7));

        foreach (var mentee in mentees)
            menteePreviews.Add(await GenerateMenteePreview(mentee,
                menteesEnrollments.GetValueOrDefault(mentee.Id, [])));

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
                new LehrerTerminPreview(termin.Id, termin.OverrideBezeichnung != null ? termin.OverrideBezeichnung : termin.Bezeichnung, termin.Ort,
                    await _enrollmentService.GetLoadPercent(termin), termin.Block.Schultag.Datum,
                    _blockHelper.Get(termin.Block.SchemaId)!.Bezeichnung)
            );

        return new LehrerUebersicht(terminPreviews, menteePreviews);


        async Task<MenteePreview> GenerateMenteePreview(Models_Person mentee, IEnumerable<OtiumEinschreibung> enrollments)
        {
            if (mentee.Rolle == Rolle.Oberstufe)
                return new MenteePreview(new PersonInfoMinimal(mentee),
                    MenteePreviewStatus.NichtVerfuegbar,
                    MenteePreviewStatus.NichtVerfuegbar,
                    MenteePreviewStatus.NichtVerfuegbar);

            var enrollmentsList = enrollments as OtiumEinschreibung[] ?? enrollments.ToArray();

            return new MenteePreview(new PersonInfoMinimal(mentee),
                await IsWeekOkay(lastWeekInterval),
                await IsWeekOkay(thisWeekInterval),
                await IsWeekOkay(nextWeekInterval)
            );

            async Task<MenteePreviewStatus> IsWeekOkay(DateTimeInterval week)
            {
                var schultageInWeek = schultage.Where(s =>
                    week.Contains(s.Datum.ToDateTime(new TimeOnly(0, 0)))).ToList();
                if (schultageInWeek.Count == 0) return MenteePreviewStatus.NichtVerfuegbar;

                var weeksMessages = await _rulesValidationService.GetMessagesForWeekAsync(mentee,
                    schultage.Where(s => schultageInWeek.Contains(s)).ToList(),
                    enrollmentsList.Where(e => schultageInWeek.Contains(e.Termin.Block.Schultag)).ToList());
                if (weeksMessages.Count > 0) return DecideBetweenOpenAndConspicuous(schultageInWeek);

                foreach (var schultag in schultageInWeek)
                {
                    var daysMessages = await _rulesValidationService.GetMessagesForDayAsync(mentee, schultag,
                        enrollmentsList.Where(e => e.Termin.Block.Schultag == schultag).ToList());
                    if (daysMessages.Count > 0) return DecideBetweenOpenAndConspicuous(schultageInWeek);
                }

                var enrollmentsMessages =
                    await _rulesValidationService.GetMessagesForEnrollmentsAsync(mentee, enrollmentsList.ToList());
                return enrollmentsMessages.Count > 0
                    ? DecideBetweenOpenAndConspicuous(schultageInWeek)
                    : MenteePreviewStatus.Okay;
            }

            MenteePreviewStatus DecideBetweenOpenAndConspicuous(List<Schultag> daysInWeek)
            {
                var today = DateOnly.FromDateTime(DateTime.Now);
                var lastDayWithBlocks = daysInWeek.Where(s => s.Blocks.Count > 0).MaxBy(s => s.Datum)?.Datum;
                if (lastDayWithBlocks is null) return MenteePreviewStatus.NichtVerfuegbar;
                if (lastDayWithBlocks >= today) return MenteePreviewStatus.Offen;
                return MenteePreviewStatus.Auffaellig;
            }
        }
    }

    /// <summary>
    ///     Generates the dashboard view as a mentee would see it and adds information about the mentee
    /// </summary>
    public LehrerMenteeView GetStudentDashboardForTeacher(Models_Person student, bool all = false)
    {
        return new LehrerMenteeView(
            GetStudentDashboardAsyncEnumerable(student, all),
            new PersonInfoMinimal(student));
    }

    /// <summary>
    ///     Gets the detailed information about a termin for a teacher.
    /// </summary>
    public async Task<LehrerTermin?> GetTerminForTeacher(Guid terminId, ClaimsPrincipal user)
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

        var isDoneOrRunning = _blockHelper.IsBlockDoneOrRunning(termin.Block);

        var schema = _blockHelper.Get(termin.Block.SchemaId)!;

        return new LehrerTermin
        {
            Id = termin.Id,
            Ort = termin.Ort,
            Otium = termin.Otium.Bezeichnung,
            OtiumId = termin.Otium.Id,
            BlockSchemaId = termin.Block.SchemaId,
            BlockId = termin.Block.Id,
            Block = schema.Bezeichnung,
            Datum = termin.Block.Schultag.Datum,
            Uhrzeit = schema.Interval,
            MaxEinschreibungen = termin.MaxEinschreibungen,
            IstAbgesagt = termin.IstAbgesagt,
            IsSupervisionEnabled = _attendanceService.MaySupervise(user, termin.Block),
            IsDoneOrRunning = isDoneOrRunning,
            Tutor = termin.Tutor is not null
                ? new PersonInfoMinimal(termin.Tutor)
                : null,
            Einschreibungen = anwesenheiten.Select(e =>
                new LehrerEinschreibung(new PersonInfoMinimal(e.Key),
                    e.Value)),
            Bezeichnung = termin.OverrideBezeichnung,
            Beschreibung = termin.OverrideBeschreibung,
        };
    }

    /// <summary>
    ///     Gets all Otia
    /// </summary>
    public IEnumerable<ManagementOtiumPreview> GetOtia()
    {
        return _dbContext.Otia
            .AsSplitQuery()
            .Include(o => o.Termine)
            .Include(o => o.Kategorie)
            .OrderBy(o => o.Bezeichnung)
            .ThenByDescending(o => o.Termine.Count)
            .Select(otium => new ManagementOtiumPreview
            {
                Id = otium.Id,
                Bezeichnung = otium.Bezeichnung,
                Kategorie = otium.Kategorie.Id,
                Termine = otium.Termine.Count
            });
    }

    /// <summary>
    ///     Gets a single Otium
    /// </summary>
    /// <param name="otiumId">The ID of the Otium to get.</param>
    public ManagementOtiumView GetOtium(Guid otiumId)
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

        return new ManagementOtiumView
        {
            Id = otium.Id,
            Bezeichnung = otium.Bezeichnung,
            Beschreibung = otium.Beschreibung,
            Kategorie = otium.Kategorie.Id,
            Verantwortliche = otium.Verantwortliche.Select(v => new PersonInfoMinimal(v)),
            Termine = otium.Termine.Select(t =>
                new ManagementTerminView(t, _blockHelper.Get(t.Block.SchemaId)!.Bezeichnung)),
            Wiederholungen = otium.Wiederholungen.Select(r =>
                new ManagementWiederholungView(r, _blockHelper.Get(r.Block)!.Bezeichnung)),
            MinKlasse = otium.MinKlasse,
            MaxKlasse = otium.MaxKlasse,
        };
    }

    /// <summary>
    ///     Creates a new Otium
    /// </summary>
    /// <param name="dtoOtium">The Otium data to add</param>
    public async Task<Guid> CreateOtiumAsync(ManagementOtiumCreation dtoOtium)
    {
        var kategorie = await _dbContext.OtiaKategorien.FindAsync(dtoOtium.Kategorie);
        if (kategorie is null)
            throw new ArgumentException("Kategorie must be valid Kategorie");

        var dbOtium = new OtiumDefinition
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
    public async Task<Guid> CreateOtiumTerminAsync(ManagementTerminCreation otiumTermin)
    {
        if (string.IsNullOrWhiteSpace(otiumTermin.Ort))
            throw new ArgumentNullException(nameof(otiumTermin), "Sie müssen einen Ort angeben.");

        var otium = await _dbContext.Otia.FindAsync(otiumTermin.OtiumId);
        if (otium is null)
            throw new ArgumentException("Kein Otium mit dieser Id existiert.");

        var block = await _dbContext.Blocks.FirstOrDefaultAsync(c =>
            c.SchemaId == otiumTermin.Block && c.Schultag.Datum == otiumTermin.Datum);
        if (block is null) throw new ArgumentException("Kein solcher Block existiert.");

        Models_Person? tutor = null;
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

        var dbOtiumTermin = new OtiumTermin
        {
            Otium = otium,
            Block = block,
            Ort = otiumTermin.Ort,
            Tutor = tutor,
            MaxEinschreibungen = otiumTermin.MaxEinschreibungen,
            IstAbgesagt = false,
            Wiederholung = null,
            OverrideBezeichnung = otiumTermin.OverrideBezeichnung,
            OverrideBeschreibung = otiumTermin.OverrideBeschreibung,
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
    public async Task<Guid> CreateOtiumWiederholungAsync(ManagementWiederholungCreation otiumWiederholung)
    {
        var otium = await _dbContext.Otia.FindAsync(otiumWiederholung.OtiumId);
        if (otium is null)
            throw new ArgumentException("Kein Otium mit dieser Id existiert.");

        Models_Person? tutor = null;
        if (otiumWiederholung.Tutor != null)
        {
            tutor = await _dbContext.Personen.FindAsync(otiumWiederholung.Tutor);
            if (tutor is null)
                throw new ArgumentException("Kein Tutor mit dieser Id existiert.");
        }

        var dbOtiumWiederholung = new OtiumWiederholung
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
            var dbOtiumTermin = new OtiumTermin
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
    public async Task DiscontinueOtiumWiederholungAsync(Guid otiumWiederholungId, DateOnly firstDayAfter)
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

    ///
    public async Task UpdateOtiumWiederholungAsync(Guid otiumWiederholungId, ManagementWiederholungEdit wiederholungEdit,
        DateOnly firstDay)
    {
        if (firstDay < DateOnly.FromDateTime(DateTime.Today))
            throw new ArgumentException("Das Datum muss in der Zukunft liegen.");

        var otiumWiederholung = await _dbContext.OtiaWiederholungen
            .AsSplitQuery()
            .Include(x => x.Otium)
            .Include(x => x.Termine.Where(t => t.Block.Schultag.Datum > firstDay))
            .FirstOrDefaultAsync(o => o.Id == otiumWiederholungId);
        if (otiumWiederholung is null)
            throw new EntityNotFoundException("Keine Wiederholung mit dieser Id");

        var termine = otiumWiederholung.Termine.ToList();

        foreach (var t in termine)
            await OtiumTerminSetOrtAsync(t.Id, wiederholungEdit.Ort, commit: false);
        otiumWiederholung.Ort = wiederholungEdit.Ort;

        foreach (var t in termine)
            await OtiumTerminSetMaxEinschreibungenAsync(t.Id, wiederholungEdit.MaxEinschreibungen, commit: false);
        otiumWiederholung.MaxEinschreibungen = wiederholungEdit.MaxEinschreibungen;
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
                 Das Otium {otiumTermin.Bezeichnung} wurde
                 am {otiumTermin.Block.Schultag.Datum} abgesagt.
                 Schreibe dich gegebenenfalls um.
                 """,
                TimeSpan.FromSeconds(30)
            );
    }

    /// <summary>
    ///     Sets the Bezeichnung of an Otium
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
    ///     Sets the Beschreibung of an Otium
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
    ///     Sets the grade limits of an Otium
    /// </summary>
    /// <param name="otiumId">The ID of the Otium to change the Bezeichnung of.</param>
    /// <param name="limits">The new grade limits</param>
    public async Task OtiumSetKlassenLimitsAsync(Guid otiumId, KlassenLimits limits)
    {
        var otium = await _dbContext.Otia.FindAsync(otiumId);
        if (otium is null) throw new ArgumentException("Kein Otium mit dieser Id existiert.");

        otium.MinKlasse = limits.MinKlasse;
        otium.MaxKlasse = limits.MaxKlasse;
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
    /// <param name="commit">Whether to commit the db changes</param>
    public async Task OtiumTerminSetMaxEinschreibungenAsync(Guid otiumTerminId, int? maxEinschreibungen,
        bool commit = true)
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
                     Für das Otium {otiumTermin.Bezeichnung} wurde
                     am {otiumTermin.Block.Schultag.Datum} die Teilnehmerbegrenzung reduziert.
                     Deine Einschreibung wurde nach dem Losverfahren gelöscht. Schreibe dich bitte neu ein.
                     """,
                    TimeSpan.FromSeconds(30)
                );
        }

        otiumTermin.MaxEinschreibungen = maxEinschreibungen;

        if (commit) await _dbContext.SaveChangesAsync();
    }

    /// <summary>
    ///     Overrides the Bezeichnung of an OtiumTermin.
    /// </summary>
    /// <param name="otiumTerminId">The ID of the OtiumTermin to set maxEinschreibungen on.</param>
    /// <param name="bezeichnung">The new Bezeichnung.</param>
    public async Task OtiumTerminSetOverrideBezeichnungAsync(Guid otiumTerminId, string? bezeichnung)
    {
        var otiumTermin = await _dbContext.OtiaTermine
            .Include(t => t.Otium)
            .FirstOrDefaultAsync(o => o.Id == otiumTerminId);
        if (otiumTermin is null)
            throw new EntityNotFoundException("Kein Termin mit dieser Id");

        otiumTermin.OverrideBezeichnung = bezeichnung?.Trim();

        await _dbContext.SaveChangesAsync();
    }

    /// <summary>
    ///     Overrides the Bezeichnung of an OtiumTermin.
    /// </summary>
    /// <param name="otiumTerminId">The ID of the OtiumTermin to set maxEinschreibungen on.</param>
    /// <param name="beschreibung">The new Beschreibung.</param>
    public async Task OtiumTerminSetOverrideBeschreibungAsync(Guid otiumTerminId, string? beschreibung)
    {
        var otiumTermin = await _dbContext.OtiaTermine
            .Include(t => t.Otium)
            .FirstOrDefaultAsync(o => o.Id == otiumTerminId);
        if (otiumTermin is null)
            throw new EntityNotFoundException("Kein Termin mit dieser Id");

        otiumTermin.OverrideBeschreibung = beschreibung?.Trim();

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

        Models_Person? person = null;
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
    /// <param name="commit">Whether to commit the db changes</param>
    public async Task OtiumTerminSetOrtAsync(Guid otiumTerminId, string ort, bool commit = true)
    {
        var otiumTermin = await _dbContext.OtiaTermine
            .FindAsync(otiumTerminId);
        if (otiumTermin is null)
            throw new EntityNotFoundException("Kein Termin mit dieser Id");

        otiumTermin.Ort = ort;

        if (commit)
        {
            await _dbContext.SaveChangesAsync();
        }
    }

    private class TerminWithLoad
    {
        public required int? Auslasung { get; init; }
        public required OtiumTermin Termin { get; init; }
        public bool IstEingeschrieben { get; set; }
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
