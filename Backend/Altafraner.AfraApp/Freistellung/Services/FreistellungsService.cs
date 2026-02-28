using Altafraner.AfraApp.Freistellung.Domain.DTO;
using Altafraner.AfraApp.Freistellung.Domain.Models;
using Altafraner.Backbone.EmailSchedulingModule;
using Altafraner.AfraApp.User.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Altafraner.AfraApp.Freistellung.Services;

/// <summary>
///     Service for managing leave requests (Freistellungsanträge).
/// </summary>
public class FreistellungsService
{
    private readonly AfraAppContext _dbContext;
    private readonly INotificationService _notificationService;
    private readonly Altafraner.Typst.Typst _typstService;

    /// <summary>
    ///     Constructs a new instance of the <see cref="FreistellungsService" />.
    /// </summary>
    public FreistellungsService(AfraAppContext dbContext, INotificationService notificationService,
        Altafraner.Typst.Typst typstService)
    {
        _dbContext = dbContext;
        _notificationService = notificationService;
        _typstService = typstService;
    }

    /// <summary>
    ///     Creates a new leave request for a student.
    /// </summary>
    public async Task<FreistellungsantragDto> CreateAntragAsync(Person student, CreateFreistellungsantragDto dto)
    {
        if (student.Rolle == Rolle.Tutor)
            throw new InvalidOperationException("Teachers cannot create leave requests.");

        if (dto.Stunden.Count == 0)
            throw new ArgumentException("At least one lesson must be specified.");

        // Require at least 5 days lead time before the start of the leave
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        if (DateOnly.FromDateTime(dto.Von) < today.AddDays(5))
            throw new ArgumentException(
                "Der Beginn der Freistellung muss mindestens 5 Tage in der Zukunft liegen.");

        if (string.IsNullOrWhiteSpace(dto.Grund))
            throw new ArgumentException("A title must be provided.");

        foreach (var stunde in dto.Stunden)
            if (stunde.Datum < DateOnly.FromDateTime(dto.Von) || stunde.Datum > DateOnly.FromDateTime(dto.Bis))
                throw new ArgumentException(
                    $"Lesson date {stunde.Datum:dd.MM.yyyy} is outside the requested leave period.");

        var uniqueLehrerIds = dto.Stunden.Select(s => s.LehrerId).Distinct().ToList();

        var lehrer = await _dbContext.Personen
            .Where(p => uniqueLehrerIds.Contains(p.Id) && p.Rolle == Rolle.Tutor)
            .ToListAsync();

        if (lehrer.Count != uniqueLehrerIds.Count)
            throw new ArgumentException("One or more specified teacher IDs are invalid.");

        var lehrerById = lehrer.ToDictionary(l => l.Id);

        // Look up the student's GM and IM mentors
        var mentorRelations = await _dbContext.MentorMenteeRelations
            .Where(r => r.StudentId == student.Id)
            .ToListAsync();

        var mentorIds = mentorRelations.Select(r => r.MentorId).Distinct().ToList();
        var mentors = await _dbContext.Personen
            .Where(p => mentorIds.Contains(p.Id))
            .ToListAsync();
        var mentorById = mentors.ToDictionary(m => m.Id);

        // All decisions (teachers + mentors) use LehrerEntscheidung
        var teacherEntscheidungen = lehrer.Select(l => new LehrerEntscheidung
        {
            Lehrer = l,
            Freistellungsantrag = null!,
        }).ToList();

        var mentorEntscheidungen = mentorRelations
            .Where(r => mentorById.ContainsKey(r.MentorId))
            .DistinctBy(r => r.MentorId)
            .Select(r => new LehrerEntscheidung
            {
                Lehrer = mentorById[r.MentorId],
                Freistellungsantrag = null!,
            })
            .ToList();

        var antrag = new Domain.Models.Freistellungsantrag
        {
            Student = student,
            Grund = dto.Grund.Trim(),
            Von = DateTime.SpecifyKind(dto.Von, DateTimeKind.Utc),
            Bis = DateTime.SpecifyKind(dto.Bis, DateTimeKind.Utc),
            Beschreibung = dto.Beschreibung,
            BetroffeneStunden = dto.Stunden.Select(s => new Domain.Models.BetroffeneStunde
            {
                Datum = s.Datum,
                Block = s.Block,
                Fach = s.Fach,
                Lehrer = lehrerById[s.LehrerId],
                Freistellungsantrag = null!,
            }).ToList(),
            Entscheidungen = teacherEntscheidungen
                .Concat(mentorEntscheidungen)
                .DistinctBy(e => e.Lehrer.Id)
                .ToList(),
        };
        Transition(antrag, student, FreistellungsStatus.Eingereicht);

        _dbContext.Freistellungsantraege.Add(antrag);
        await _dbContext.SaveChangesAsync();

        foreach (var teacher in lehrer.Concat(mentors).DistinctBy(l => l.Id))
            await NotifyAsync(teacher, "Neuer Freistellungsantrag",
                $"""
                 {StudentName(antrag)} hat einen Freistellungsantrag {AntragBezeichnung(antrag)} gestellt.
                 Bitte melde dich in der Afra-App an, um deine Einschätzung dazu abzugeben.

                 Titel: {antrag.Grund}
                 Grund: {dto.Beschreibung}
                 """);

        return await BuildDtoAsync(antrag);
    }

    /// <summary>
    ///     Gets all leave requests submitted by the given student.
    /// </summary>
    public async Task<List<FreistellungsantragDto>> GetAntraegeForStudentAsync(Person student)
    {
        var antraege = await LoadAntraegeAsync(a => a.StudentId == student.Id);
        return await BuildDtosAsync(antraege.OrderByDescending(a => a.ErstelltAm).ToList());
    }

    /// <summary>
    ///     Gets all leave requests that the given person is involved in as an approver
    ///     (either as a teacher of an affected lesson or as a mentor of the student),
    ///     both pending and already decided, ordered newest-first.
    /// </summary>
    public async Task<List<FreistellungsantragDto>> GetAntraegeForLehrerAsync(Person lehrer)
    {
        var antraege = await LoadAntraegeAsync(a => a.Entscheidungen.Any(e => e.LehrerId == lehrer.Id));
        return await BuildDtosAsync(antraege.OrderByDescending(a => a.Von).ToList());
    }

    /// <summary>
    ///     Records a teacher's or mentor's individual assessment of a leave request. This is
    ///     advisory only — it does not by itself approve or reject the overall request, only the
    ///     Schulleiter can do that. Once every teacher and mentor has responded (regardless of the
    ///     individual outcome), the request moves on to the Sekretariat. The student is not
    ///     notified of individual assessments, only of the Schulleiter's final decision, so a
    ///     single objection is never mistaken for a rejection of the whole request.
    /// </summary>
    public async Task<FreistellungsantragDto> RecordEntscheidungAsync(Person lehrer, Guid antragId, EntscheidungDto dto)
    {
        if (dto.Status == EntscheidungsStatus.Ausstehend)
            throw new ArgumentException("Decision status must be Genehmigt or Abgelehnt.");

        var antrag = await LoadAntragOrThrowAsync(antragId);

        if (antrag.Status != FreistellungsStatus.Eingereicht)
            throw new InvalidOperationException("This leave request is no longer pending teacher decisions.");

        var entscheidung = antrag.Entscheidungen
            .FirstOrDefault(e => e.LehrerId == lehrer.Id);

        if (entscheidung is null)
            throw new InvalidOperationException("You are not assigned to this leave request.");

        if (entscheidung.Status != EntscheidungsStatus.Ausstehend)
            throw new InvalidOperationException("You have already made a decision on this request.");

        entscheidung.Status = dto.Status;
        entscheidung.Kommentar = dto.Kommentar;
        entscheidung.EntschiedenAm = DateTime.UtcNow;

        if (AllDecided(antrag))
            Transition(antrag, null, FreistellungsStatus.BeiSekretariat);

        await _dbContext.SaveChangesAsync();

        if (antrag.Status == FreistellungsStatus.BeiSekretariat)
            await NotifyPersonenMitBerechtigungAsync(GlobalPermission.Sekretariat,
                "Freistellungsantrag wartet auf Bearbeitung",
                $"""
                 Für den Freistellungsantrag {AntragBezeichnung(antrag)} von {StudentName(antrag)} liegen nun alle Rückmeldungen der Lehrkräfte und Mentor:innen vor.
                 Bitte melde dich in der Afra-App an, um zu entscheiden, ob eine Elternbestätigung erforderlich ist.
                 """);

        return await BuildDtoAsync(antrag);
    }

    /// <summary>
    ///     Gets all leave requests relevant to the Sekretariat — everything from the point a request
    ///     first reaches them onward — ordered newest-first.
    /// </summary>
    public async Task<List<FreistellungsantragDto>> GetAntraegeForSekretariatAsync()
    {
        var antraege = await LoadAntraegeAsync(a => a.Status != FreistellungsStatus.Eingereicht);
        return await BuildDtosAsync(antraege.OrderByDescending(a => a.Von).ToList());
    }

    /// <summary>
    ///     Records the Sekretariat's decision about whether a Elternbestätigung (parental confirmation)
    ///     is required and, if so, whether it is already present. If not required, or required and
    ///     present, the request is forwarded to the Schulleiter. Otherwise it is sent back to the
    ///     student with a hint about what is missing.
    /// </summary>
    public async Task<FreistellungsantragDto> EntscheidungElternbestaetigungAsync(Person sekretariat, Guid antragId,
        EntscheidungElternbestaetigungDto dto)
    {
        var antrag = await LoadAntragOrThrowAsync(antragId);

        if (antrag.Status != FreistellungsStatus.BeiSekretariat
            && antrag.Status != FreistellungsStatus.ElternbestaetigungEingereicht)
            throw new InvalidOperationException(
                "The Elternbestätigung cannot be decided on this leave request in its current state.");

        antrag.ElternbestaetigungErforderlich = dto.Erforderlich;

        if (!dto.Erforderlich || dto.Vorhanden)
        {
            antrag.ElternbestaetigungVorhanden = dto.Erforderlich && dto.Vorhanden;
            Transition(antrag, sekretariat, FreistellungsStatus.BeimSchulleiter);
            await _dbContext.SaveChangesAsync();

            await NotifyPersonenMitBerechtigungAsync(GlobalPermission.Schulleiter,
                "Freistellungsantrag wartet auf abschließende Genehmigung",
                $"""
                 Der Freistellungsantrag {AntragBezeichnung(antrag)} von {StudentName(antrag)} wurde vom Sekretariat weitergeleitet und wartet auf Ihre Genehmigung.
                 Bitte melde dich in der Afra-App an, um den Antrag abschließend zu entscheiden.
                 """);
        }
        else
        {
            if (string.IsNullOrWhiteSpace(dto.Hinweis))
                throw new ArgumentException("Bitte einen Hinweis angeben, was noch fehlt.");

            antrag.ElternbestaetigungVorhanden = false;
            Transition(antrag, sekretariat, FreistellungsStatus.WartetAufEltern, dto.Hinweis.Trim());
            await _dbContext.SaveChangesAsync();

            await NotifyAsync(antrag.Student, "Elternbestätigung erforderlich",
                $"""
                 Für deinen Freistellungsantrag {AntragBezeichnung(antrag)} wird noch eine Elternbestätigung benötigt.
                 Hinweis vom Sekretariat: {dto.Hinweis.Trim()}
                 Melde dich in der Afra-App an und reiche den Antrag erneut ein, sobald die Bestätigung vorliegt.
                 """);
        }

        return await BuildDtoAsync(antrag);
    }

    /// <summary>
    ///     Lets a student indicate that a previously missing Elternbestätigung has now been provided.
    ///     Sends the request back to the Sekretariat to confirm.
    /// </summary>
    public async Task<FreistellungsantragDto> ElternbestaetigungNachreichenAsync(Person student, Guid antragId)
    {
        var antrag = await LoadAntragOrThrowAsync(antragId);

        if (antrag.StudentId != student.Id)
            throw new InvalidOperationException("You can only re-submit your own leave requests.");

        if (antrag.Status != FreistellungsStatus.WartetAufEltern)
            throw new InvalidOperationException(
                "This leave request is not currently waiting for a Elternbestätigung.");

        Transition(antrag, student, FreistellungsStatus.ElternbestaetigungEingereicht);
        await _dbContext.SaveChangesAsync();

        await NotifyPersonenMitBerechtigungAsync(GlobalPermission.Sekretariat, "Elternbestätigung nachgereicht",
            $"""
             {StudentName(antrag)} hat für den Freistellungsantrag {AntragBezeichnung(antrag)} eine Elternbestätigung nachgereicht.
             Bitte melde dich in der Afra-App an, um dies zu prüfen.
             """);

        return await BuildDtoAsync(antrag);
    }

    /// <summary>
    ///     Marks a Schulleiter-approved leave request as entered into Cevex, completing the workflow.
    /// </summary>
    public async Task<FreistellungsantragDto> CevexErledigtAsync(Person sekretariat, Guid antragId)
    {
        var antrag = await LoadAntragOrThrowAsync(antragId);

        if (antrag.Status != FreistellungsStatus.Genehmigt)
            throw new InvalidOperationException("Only Schulleiter-approved leave requests can be entered into Cevex.");

        Transition(antrag, sekretariat, FreistellungsStatus.Abgeschlossen);
        await _dbContext.SaveChangesAsync();

        return await BuildDtoAsync(antrag);
    }

    /// <summary>
    ///     Gets all leave requests relevant to the Schulleiter — those awaiting final decision
    ///     as well as those already decided, ordered newest-first.
    /// </summary>
    public async Task<List<FreistellungsantragDto>> GetAntraegeForSchulleiterAsync()
    {
        var antraege = await LoadAntraegeAsync(a => a.Status == FreistellungsStatus.BeimSchulleiter
                                                      || a.Status == FreistellungsStatus.Genehmigt
                                                      || a.Status == FreistellungsStatus.Abgeschlossen
                                                      || a.Status == FreistellungsStatus.Abgelehnt);
        return await BuildDtosAsync(antraege.OrderByDescending(a => a.Von).ToList());
    }

    /// <summary>
    ///     Gives final Schulleiter approval for a leave request. Can also be used to reverse a
    ///     previous Schulleiter rejection made in error.
    /// </summary>
    public async Task<FreistellungsantragDto> SchulleiterBestaetigenAsync(Person schulleiter, Guid antragId)
    {
        var antrag = await LoadAntragOrThrowAsync(antragId);

        if (antrag.Status != FreistellungsStatus.BeimSchulleiter
            && antrag.Status != FreistellungsStatus.Abgelehnt)
            throw new InvalidOperationException("This leave request is not awaiting Schulleiter approval.");

        Transition(antrag, schulleiter, FreistellungsStatus.Genehmigt);
        await _dbContext.SaveChangesAsync();

        await NotifyAsync(antrag.Student, "Freistellungsantrag genehmigt",
            $"""
             Dein Freistellungsantrag {AntragBezeichnung(antrag)} wurde vom Schulleiter genehmigt.
             Die Freistellung ist damit gültig. Melde dich in der Afra-App an, um die Details einzusehen.
             """);

        await NotifyPersonenMitBerechtigungAsync(GlobalPermission.Sekretariat, "Freistellung in Cevex eintragen",
            $"""
             Der Freistellungsantrag {AntragBezeichnung(antrag)} von {StudentName(antrag)} wurde vom Schulleiter genehmigt.
             Bitte die Freistellung in Cevex eintragen.
             """);

        return await BuildDtoAsync(antrag);
    }

    /// <summary>
    ///     Rejects a leave request. Only the Schulleiter may reject a request, and only once —
    ///     a mistaken rejection can be reversed via <see cref="SchulleiterBestaetigenAsync" />.
    /// </summary>
    public async Task<FreistellungsantragDto> SchulleiterAblehnenAsync(Person schulleiter, Guid antragId,
        AblehnungDto dto)
    {
        var antrag = await LoadAntragOrThrowAsync(antragId);

        if (antrag.Status != FreistellungsStatus.BeimSchulleiter)
            throw new InvalidOperationException("This leave request is not awaiting Schulleiter approval.");

        Transition(antrag, schulleiter, FreistellungsStatus.Abgelehnt, dto.Kommentar.Trim());
        await _dbContext.SaveChangesAsync();

        await NotifyAsync(antrag.Student, "Freistellungsantrag abgelehnt",
            $"""
             Dein Freistellungsantrag {AntragBezeichnung(antrag)} wurde vom Schulleiter abgelehnt.
             Kommentar: {dto.Kommentar.Trim()}
             """);

        return await BuildDtoAsync(antrag);
    }

    /// <summary>
    ///     Counts, for the given person, how many leave requests currently require their attention
    ///     across all roles they hold (teacher/mentor decision, Sekretariat, Schulleiter, or student
    ///     follow-up on a missing Elternbestätigung).
    /// </summary>
    public async Task<int> GetOffeneAntraegeAnzahlAsync(Person person)
    {
        var anzahl = await _dbContext.Freistellungsantraege.CountAsync(a =>
            a.StudentId == person.Id && a.Status == FreistellungsStatus.WartetAufEltern);

        anzahl += await _dbContext.Freistellungsantraege.CountAsync(a =>
            a.Status == FreistellungsStatus.Eingereicht
            && a.Entscheidungen.Any(e => e.LehrerId == person.Id && e.Status == EntscheidungsStatus.Ausstehend));

        if (HatBerechtigung(person, GlobalPermission.Sekretariat))
            anzahl += await _dbContext.Freistellungsantraege.CountAsync(a =>
                a.Status == FreistellungsStatus.BeiSekretariat
                || a.Status == FreistellungsStatus.ElternbestaetigungEingereicht
                || a.Status == FreistellungsStatus.Genehmigt);

        if (HatBerechtigung(person, GlobalPermission.Schulleiter))
            anzahl += await _dbContext.Freistellungsantraege.CountAsync(a =>
                a.Status == FreistellungsStatus.BeimSchulleiter);

        return anzahl;
    }

    /// <summary>
    ///     Generates a PDF document for the given leave request.
    /// </summary>
    public async Task<byte[]> GeneratePdfAsync(Guid antragId)
    {
        var antrag = await LoadAntragOrThrowAsync(antragId);
        var dto = await BuildDtoAsync(antrag);
        return _typstService.GeneratePdf(Altafraner.Typst.Templates.Freistellung.Antrag, dto);
    }

    /// <summary>
    ///     Returns true iff every teacher/mentor decision on the given request has been made
    ///     (approved or rejected — an individual rejection does not stop the process).
    /// </summary>
    private static bool AllDecided(Domain.Models.Freistellungsantrag antrag)
        => antrag.Entscheidungen.All(e => e.Status != EntscheidungsStatus.Ausstehend);

    /// <summary>
    ///     Moves <paramref name="antrag" /> to <paramref name="neuerStatus" /> and appends the
    ///     corresponding entry to its Verlauf. This is the only place <see cref="FreistellungsStatus" />
    ///     is ever assigned — status and history can therefore never drift apart.
    /// </summary>
    private static void Transition(Domain.Models.Freistellungsantrag antrag, Person? person,
        FreistellungsStatus neuerStatus, string? kommentar = null)
    {
        antrag.Status = neuerStatus;
        antrag.Verlauf.Add(new FreistellungsVerlaufEintrag
        {
            Freistellungsantrag = antrag,
            Person = person,
            NeuerStatus = neuerStatus,
            Kommentar = kommentar,
        });
    }

    /// <summary>
    ///     Returns true iff the given person holds the given GlobalPermission, or is an Admin
    ///     (who implicitly holds every role-scoped permission).
    /// </summary>
    private static bool HatBerechtigung(Person person, GlobalPermission permission)
        => person.GlobalPermissions.Contains(permission) || person.GlobalPermissions.Contains(GlobalPermission.Admin);

    private static string StudentName(Domain.Models.Freistellungsantrag antrag)
        => $"{antrag.Student.FirstName} {antrag.Student.LastName}";

    private static string AntragBezeichnung(Domain.Models.Freistellungsantrag antrag)
        => $"„{antrag.Grund}\" für {FormatDateRange(antrag.Von, antrag.Bis)}";

    private static string FormatDateRange(DateTime von, DateTime bis)
        => von.Date == bis.Date
            ? $"den {von:dd.MM.yyyy}"
            : $"den Zeitraum {von:dd.MM.yyyy} – {bis:dd.MM.yyyy}";

    private IQueryable<Domain.Models.Freistellungsantrag> AntraegeQuery()
        => _dbContext.Freistellungsantraege
            .AsSplitQuery()
            .Include(a => a.Student)
            .Include(a => a.BetroffeneStunden)
            .ThenInclude(s => s.Lehrer)
            .Include(a => a.Entscheidungen)
            .ThenInclude(e => e.Lehrer)
            .Include(a => a.Verlauf)
            .ThenInclude(v => v.Person);

    private async Task<List<Domain.Models.Freistellungsantrag>> LoadAntraegeAsync(
        System.Linq.Expressions.Expression<Func<Domain.Models.Freistellungsantrag, bool>> predicate)
        => await AntraegeQuery().Where(predicate).ToListAsync();

    private async Task<Domain.Models.Freistellungsantrag> LoadAntragOrThrowAsync(Guid antragId)
        => await AntraegeQuery().FirstOrDefaultAsync(a => a.Id == antragId)
           ?? throw new KeyNotFoundException("Leave request not found.");

    private async Task<List<FreistellungsantragDto>> BuildDtosAsync(List<Domain.Models.Freistellungsantrag> antraege)
    {
        var statistiken = await GetStatistikenAsync(antraege.Select(a => a.StudentId).Distinct().ToList());
        return antraege
            .Select(a => new FreistellungsantragDto(a,
                statistiken.GetValueOrDefault(a.StudentId, new FreistellungsStatistikDto(0, 0))))
            .ToList();
    }

    private async Task<FreistellungsantragDto> BuildDtoAsync(Domain.Models.Freistellungsantrag antrag)
    {
        var statistiken = await GetStatistikenAsync([antrag.StudentId]);
        return new FreistellungsantragDto(antrag,
            statistiken.GetValueOrDefault(antrag.StudentId, new FreistellungsStatistikDto(0, 0)));
    }

    /// <summary>
    ///     Computes, for each given student, the number of approved leave requests and the number
    ///     of missed lesson-hours they represent within the current Schuljahr. Counts both
    ///     <see cref="FreistellungsStatus.Genehmigt" /> and <see cref="FreistellungsStatus.Abgeschlossen" />
    ///     — whether the Sekretariat has already filed the leave in Cevex does not change whether
    ///     it was granted.
    /// </summary>
    private async Task<Dictionary<Guid, FreistellungsStatistikDto>> GetStatistikenAsync(List<Guid> studentIds)
    {
        if (studentIds.Count == 0)
            return new Dictionary<Guid, FreistellungsStatistikDto>();

        var schuljahresStart = GetSchuljahresStart(DateTime.UtcNow);
        var schuljahresEnde = schuljahresStart.AddYears(1);

        var rows = await _dbContext.Freistellungsantraege
            .Where(a => studentIds.Contains(a.StudentId)
                        && (a.Status == FreistellungsStatus.Genehmigt || a.Status == FreistellungsStatus.Abgeschlossen)
                        && a.Von >= schuljahresStart && a.Von < schuljahresEnde)
            .Select(a => new { a.StudentId, StundenAnzahl = a.BetroffeneStunden.Count })
            .ToListAsync();

        return rows
            .GroupBy(r => r.StudentId)
            .ToDictionary(g => g.Key,
                g => new FreistellungsStatistikDto(g.Count(), g.Sum(r => r.StundenAnzahl)));
    }

    /// <summary>
    ///     Returns the start (August 1st) of the Schuljahr containing the given date.
    /// </summary>
    private static DateTime GetSchuljahresStart(DateTime reference)
    {
        var jahr = reference.Month >= 8 ? reference.Year : reference.Year - 1;
        return new DateTime(jahr, 8, 1, 0, 0, 0, DateTimeKind.Utc);
    }

    /// <summary>
    ///     Notifies every person holding the given GlobalPermission (Admins always included).
    /// </summary>
    private async Task NotifyPersonenMitBerechtigungAsync(GlobalPermission permission, string titel, string nachricht)
    {
        var personen = await _dbContext.Personen
            .Where(p => p.GlobalPermissions.Contains(permission) || p.GlobalPermissions.Contains(GlobalPermission.Admin))
            .ToListAsync();

        foreach (var person in personen)
            await NotifyAsync(person, titel, nachricht);
    }

    private Task NotifyAsync(Person empfaenger, string titel, string nachricht)
        => _notificationService.ScheduleNotificationAsync(empfaenger, titel, nachricht, TimeSpan.Zero);
}
