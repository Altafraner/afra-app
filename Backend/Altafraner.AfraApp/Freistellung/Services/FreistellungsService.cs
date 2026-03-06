using Altafraner.AfraApp.Freistellung.Domain.DTO;
using Altafraner.AfraApp.Freistellung.Domain.Models;
using Altafraner.AfraApp.User.Domain.Models;
using Altafraner.Backbone.EmailSchedulingModule;
using Microsoft.EntityFrameworkCore;

namespace Altafraner.AfraApp.Freistellung.Services;

/// <summary>
///     Service for managing leave requests (Freistellungsanträge).
/// </summary>
public class FreistellungsService
{
    private readonly AfraAppContext _dbContext;
    private readonly INotificationService _notificationService;

    /// <summary>
    ///     Constructs a new instance of the <see cref="FreistellungsService" />.
    /// </summary>
    public FreistellungsService(AfraAppContext dbContext, INotificationService notificationService)
    {
        _dbContext = dbContext;
        _notificationService = notificationService;
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

        if (string.IsNullOrWhiteSpace(dto.Titel))
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
            Titel = dto.Titel.Trim(),
            Von = DateTime.SpecifyKind(dto.Von, DateTimeKind.Utc),
            Bis = DateTime.SpecifyKind(dto.Bis, DateTimeKind.Utc),
            Grund = dto.Grund,
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

        _dbContext.Freistellungsantraege.Add(antrag);
        await _dbContext.SaveChangesAsync();

        // Notify each assigned teacher
        foreach (var teacher in lehrer)
            await _notificationService.ScheduleNotificationAsync(
                teacher,
                "Neuer Freistellungsantrag",
                $"""
                 {student.FirstName} {student.LastName} hat einen Freistellungsantrag für {FormatDateRange(dto.Von, dto.Bis)} gestellt.
                 Bitte melde dich in der Afra-App an, um den Antrag zu bearbeiten.

                 Titel: {antrag.Titel}
                 Grund: {dto.Grund}
                 """,
                TimeSpan.FromMinutes(5)
            );

        // Notify each mentor
        foreach (var mentor in mentors.DistinctBy(m => m.Id))
            await _notificationService.ScheduleNotificationAsync(
                mentor,
                "Neuer Freistellungsantrag",
                $"""
                 {student.FirstName} {student.LastName} hat einen Freistellungsantrag für {FormatDateRange(dto.Von, dto.Bis)} gestellt.
                 Bitte melde dich in der Afra-App an, um den Antrag als Mentor:in zu bearbeiten.

                 Titel: {antrag.Titel}
                 Grund: {dto.Grund}
                 """,
                TimeSpan.FromMinutes(5)
            );

        return new FreistellungsantragDto(antrag);
    }

    /// <summary>
    ///     Gets all leave requests submitted by the given student.
    /// </summary>
    public async Task<List<FreistellungsantragDto>> GetAntraegeForStudentAsync(Person student)
    {
        var antraege = await _dbContext.Freistellungsantraege
            .AsSplitQuery()
            .Include(a => a.Student)
            .Include(a => a.BetroffeneStunden)
            .ThenInclude(s => s.Lehrer)
            .Include(a => a.Entscheidungen)
            .ThenInclude(e => e.Lehrer)
            .Where(a => a.StudentId == student.Id)
            .OrderByDescending(a => a.ErstelltAm)
            .ToListAsync();

        return antraege.Select(a => new FreistellungsantragDto(a)).ToList();
    }

    /// <summary>
    ///     Gets all leave requests that the given person is involved in as an approver
    ///     (either as a teacher of an affected lesson or as a mentor of the student),
    ///     both pending and already decided, ordered newest-first.
    /// </summary>
    public async Task<List<FreistellungsantragDto>> GetAntraegeForLehrerAsync(Person lehrer)
    {
        var antraege = await _dbContext.Freistellungsantraege
            .AsSplitQuery()
            .Include(a => a.Student)
            .Include(a => a.BetroffeneStunden)
            .ThenInclude(s => s.Lehrer)
            .Include(a => a.Entscheidungen)
            .ThenInclude(e => e.Lehrer)
            .Where(a => a.Entscheidungen.Any(e => e.LehrerId == lehrer.Id))
            .OrderByDescending(a => a.Von)
            .ToListAsync();

        return antraege.Select(a => new FreistellungsantragDto(a)).ToList();
    }

    /// <summary>
    ///     Records a teacher's decision on a leave request.
    /// </summary>
    public async Task<FreistellungsantragDto> RecordEntscheidungAsync(Person lehrer, Guid antragId, EntscheidungDto dto)
    {
        if (dto.Status == EntscheidungsStatus.Ausstehend)
            throw new ArgumentException("Decision status must be Genehmigt or Abgelehnt.");

        var antrag = await LoadAntragAsync(antragId);

        if (antrag is null)
            throw new KeyNotFoundException("Leave request not found.");

        if (antrag.Status != FreistellungsStatus.Gestellt)
            throw new InvalidOperationException("This leave request is no longer pending.");

        var entscheidung = antrag.Entscheidungen
            .FirstOrDefault(e => e.LehrerId == lehrer.Id);

        if (entscheidung is null)
            throw new InvalidOperationException("You are not assigned to this leave request.");

        if (entscheidung.Status != EntscheidungsStatus.Ausstehend)
            throw new InvalidOperationException("You have already made a decision on this request.");

        entscheidung.Status = dto.Status;
        entscheidung.Kommentar = dto.Kommentar;
        entscheidung.EntschiedenAm = DateTime.UtcNow;

        // Update overall status
        if (dto.Status == EntscheidungsStatus.Abgelehnt)
        {
            antrag.Status = FreistellungsStatus.Abgelehnt;
        }
        else if (AllApproved(antrag))
        {
            antrag.Status = FreistellungsStatus.AlleLehrerGenehmigt;
        }

        await _dbContext.SaveChangesAsync();

        await NotifyStudentOnEntscheidungAsync(antrag, lehrer, dto);
        await NotifySekretariatIfAllApprovedAsync(antrag);

        return new FreistellungsantragDto(antrag);
    }

    /// <summary>
    ///     Gets all leave requests relevant to the Sekretariat — those awaiting confirmation
    ///     as well as those already confirmed or rejected — ordered newest-first.
    /// </summary>
    public async Task<List<FreistellungsantragDto>> GetAntraegeForSekretariatAsync()
    {
        var antraege = await _dbContext.Freistellungsantraege
            .AsSplitQuery()
            .Include(a => a.Student)
            .Include(a => a.BetroffeneStunden)
            .ThenInclude(s => s.Lehrer)
            .Include(a => a.Entscheidungen)
            .ThenInclude(e => e.Lehrer)
            .Where(a => a.Status == FreistellungsStatus.AlleLehrerGenehmigt
                        || a.Status == FreistellungsStatus.Bestaetigt
                        || a.Status == FreistellungsStatus.SchulleiterBestaetigt
                        || a.Status == FreistellungsStatus.Abgelehnt)
            .OrderByDescending(a => a.Von)
            .ToListAsync();

        return antraege.Select(a => new FreistellungsantragDto(a)).ToList();
    }

    /// <summary>
    ///     Confirms a leave request that has been fully approved by all teachers and mentors.
    ///     Moves the status to <see cref="FreistellungsStatus.Bestaetigt" /> and notifies the Schulleiter.
    /// </summary>
    public async Task<FreistellungsantragDto> BestaetigeAntragAsync(Guid antragId)
    {
        var antrag = await LoadAntragAsync(antragId);

        if (antrag is null)
            throw new KeyNotFoundException("Leave request not found.");

        if (antrag.Status != FreistellungsStatus.AlleLehrerGenehmigt)
            throw new InvalidOperationException("This leave request has not been fully approved by all teachers and mentors.");

        antrag.Status = FreistellungsStatus.Bestaetigt;
        await _dbContext.SaveChangesAsync();

        // Notify the student that Sekretariat has confirmed
        await _notificationService.ScheduleNotificationAsync(
            antrag.Student,
            "Freistellungsantrag vom Sekretariat bestätigt",
            $"""
             Dein Freistellungsantrag „{antrag.Titel}" für {FormatDateRange(antrag.Von, antrag.Bis)} wurde vom Sekretariat bestätigt.
             Die Freistellung wartet noch auf die abschließende Genehmigung des Schulleiters.
             """,
            TimeSpan.FromMinutes(5)
        );

        // Notify the Schulleiter
        var schulleiter = await _dbContext.Personen
            .Where(p => p.GlobalPermissions.Contains(GlobalPermission.Schulleiter)
                        || p.GlobalPermissions.Contains(GlobalPermission.Admin))
            .ToListAsync();

        foreach (var sl in schulleiter)
            await _notificationService.ScheduleNotificationAsync(
                sl,
                "Freistellungsantrag wartet auf abschließende Genehmigung",
                $"""
                 Der Freistellungsantrag „{antrag.Titel}" von {antrag.Student.FirstName} {antrag.Student.LastName} für {FormatDateRange(antrag.Von, antrag.Bis)} wurde vom Sekretariat bestätigt und wartet auf Ihre Genehmigung.
                 Bitte melde dich in der Afra-App an, um den Antrag abschließend zu genehmigen.
                 """,
                TimeSpan.FromMinutes(5)
            );

        return new FreistellungsantragDto(antrag);
    }

    /// <summary>
    ///     Gets all leave requests relevant to the Schulleiter — those awaiting final approval
    ///     as well as those already approved — ordered newest-first.
    /// </summary>
    public async Task<List<FreistellungsantragDto>> GetAntraegeForSchulleiterAsync()
    {
        var antraege = await _dbContext.Freistellungsantraege
            .AsSplitQuery()
            .Include(a => a.Student)
            .Include(a => a.BetroffeneStunden)
            .ThenInclude(s => s.Lehrer)
            .Include(a => a.Entscheidungen)
            .ThenInclude(e => e.Lehrer)
            .Where(a => a.Status == FreistellungsStatus.Bestaetigt
                        || a.Status == FreistellungsStatus.SchulleiterBestaetigt
                        || a.Status == FreistellungsStatus.Abgelehnt)
            .OrderByDescending(a => a.Von)
            .ToListAsync();

        return antraege.Select(a => new FreistellungsantragDto(a)).ToList();
    }

    /// <summary>
    ///     Gives the final Schulleiter approval for a leave request that has been confirmed by Sekretariat.
    /// </summary>
    public async Task<FreistellungsantragDto> SchulleiterBestaetigenAsync(Guid antragId)
    {
        var antrag = await LoadAntragAsync(antragId);

        if (antrag is null)
            throw new KeyNotFoundException("Leave request not found.");

        if (antrag.Status != FreistellungsStatus.Bestaetigt)
            throw new InvalidOperationException("This leave request has not yet been confirmed by the Sekretariat.");

        antrag.Status = FreistellungsStatus.SchulleiterBestaetigt;
        await _dbContext.SaveChangesAsync();

        // Notify the student that their request has been finally approved
        await _notificationService.ScheduleNotificationAsync(
            antrag.Student,
            "Freistellungsantrag genehmigt",
            $"""
             Dein Freistellungsantrag „{antrag.Titel}" für {FormatDateRange(antrag.Von, antrag.Bis)} wurde vom Schulleiter endgültig genehmigt.
             Die Freistellung ist damit gültig. Melde dich in der Afra-App an, um die Details einzusehen.
             """,
            TimeSpan.FromMinutes(5)
        );

        return new FreistellungsantragDto(antrag);
    }

    /// <summary>
    ///     Returns true iff all decisions on the given request are approved.
    /// </summary>
    private static bool AllApproved(Domain.Models.Freistellungsantrag antrag)
        => antrag.Entscheidungen.All(e => e.Status == EntscheidungsStatus.Genehmigt);

    private async Task<Domain.Models.Freistellungsantrag?> LoadAntragAsync(Guid antragId)
        => await _dbContext.Freistellungsantraege
            .AsSplitQuery()
            .Include(a => a.Student)
            .Include(a => a.BetroffeneStunden)
            .ThenInclude(s => s.Lehrer)
            .Include(a => a.Entscheidungen)
            .ThenInclude(e => e.Lehrer)
            .FirstOrDefaultAsync(a => a.Id == antragId);

    private async Task NotifyStudentOnEntscheidungAsync(Domain.Models.Freistellungsantrag antrag, Person entscheider,
        EntscheidungDto dto)
    {
        var kommentarZeile = string.IsNullOrWhiteSpace(dto.Kommentar)
            ? string.Empty
            : $"\nKommentar: {dto.Kommentar}";
        var entscheidungText = dto.Status == EntscheidungsStatus.Genehmigt ? "genehmigt" : "abgelehnt";
        await _notificationService.ScheduleNotificationAsync(
            antrag.Student,
            $"Freistellungsantrag {entscheidungText}",
            $"""
             Dein Freistellungsantrag „{antrag.Titel}" für {FormatDateRange(antrag.Von, antrag.Bis)} wurde von {entscheider.FirstName} {entscheider.LastName} {entscheidungText}.{kommentarZeile}
             Melde dich in der Afra-App an, um den aktuellen Status zu sehen.
             """,
            TimeSpan.FromMinutes(5)
        );
    }

    private async Task NotifySekretariatIfAllApprovedAsync(Domain.Models.Freistellungsantrag antrag)
    {
        if (antrag.Status != FreistellungsStatus.AlleLehrerGenehmigt) return;

        var sekretariat = await _dbContext.Personen
            .Where(p => p.GlobalPermissions.Contains(GlobalPermission.Sekretariat)
                        || p.GlobalPermissions.Contains(GlobalPermission.Admin))
            .ToListAsync();

        foreach (var sekretariatMember in sekretariat)
            await _notificationService.ScheduleNotificationAsync(
                sekretariatMember,
                "Freistellungsantrag wartet auf Bestätigung",
                $"""
                 Der Freistellungsantrag „{antrag.Titel}" von {antrag.Student.FirstName} {antrag.Student.LastName} für {FormatDateRange(antrag.Von, antrag.Bis)} wurde von allen Lehrkräften und Mentor:innen genehmigt.
                 Bitte melde dich in der Afra-App an, um den Antrag abschließend zu bestätigen.
                 """,
                TimeSpan.FromMinutes(5)
            );
    }

    private static string FormatDateRange(DateTime von, DateTime bis)
        => von.Date == bis.Date
            ? $"den {von:dd.MM.yyyy}"
            : $"den Zeitraum {von:dd.MM.yyyy} – {bis:dd.MM.yyyy}";
}
