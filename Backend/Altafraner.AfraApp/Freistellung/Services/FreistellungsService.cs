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

        foreach (var stunde in dto.Stunden)
            if (stunde.Datum < dto.DatumVon || stunde.Datum > dto.DatumBis)
                throw new ArgumentException(
                    $"Lesson date {stunde.Datum:dd.MM.yyyy} is outside the requested leave period.");

        var uniqueLehrerIds = dto.Stunden.Select(s => s.LehrerId).Distinct().ToList();

        var lehrer = await _dbContext.Personen
            .Where(p => uniqueLehrerIds.Contains(p.Id) && p.Rolle == Rolle.Tutor)
            .ToListAsync();

        if (lehrer.Count != uniqueLehrerIds.Count)
            throw new ArgumentException("One or more specified teacher IDs are invalid.");

        var lehrerById = lehrer.ToDictionary(l => l.Id);

        var antrag = new Domain.Models.Freistellungsantrag
        {
            Student = student,
            DatumVon = dto.DatumVon,
            DatumBis = dto.DatumBis,
            Grund = dto.Grund,
            BetroffeneStunden = dto.Stunden.Select(s => new Domain.Models.BetroffeneStunde
            {
                Datum = s.Datum,
                Block = s.Block,
                Fach = s.Fach,
                Lehrer = lehrerById[s.LehrerId],
                Freistellungsantrag = null!,
            }).ToList(),
            Entscheidungen = lehrer.Select(l => new LehrerEntscheidung
            {
                Lehrer = l,
                Freistellungsantrag = null!,
            }).ToList()
        };

        _dbContext.Freistellungsantraege.Add(antrag);
        await _dbContext.SaveChangesAsync();

        // Notify each assigned teacher
        foreach (var teacher in lehrer)
            await _notificationService.ScheduleNotificationAsync(
                teacher,
                "Neuer Freistellungsantrag",
                $"""
                 {student.FirstName} {student.LastName} hat einen Freistellungsantrag für {FormatDateRange(dto.DatumVon, dto.DatumBis)} gestellt.
                 Bitte melde dich in der Afra-App an, um den Antrag zu bearbeiten.

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
    ///     Gets all pending leave requests that need a decision from the given teacher.
    /// </summary>
    public async Task<List<FreistellungsantragDto>> GetPendingAntraegeForLehrerAsync(Person lehrer)
    {
        var antraege = await _dbContext.Freistellungsantraege
            .Include(a => a.Student)
            .Include(a => a.BetroffeneStunden)
            .ThenInclude(s => s.Lehrer)
            .Include(a => a.Entscheidungen)
            .ThenInclude(e => e.Lehrer)
            .Where(a => a.Entscheidungen.Any(e =>
                e.LehrerId == lehrer.Id && e.Status == EntscheidungsStatus.Ausstehend)
                && a.Status == FreistellungsStatus.Gestellt)
            .OrderBy(a => a.DatumVon)
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

        var antrag = await _dbContext.Freistellungsantraege
            .Include(a => a.Student)
            .Include(a => a.BetroffeneStunden)
            .ThenInclude(s => s.Lehrer)
            .Include(a => a.Entscheidungen)
            .ThenInclude(e => e.Lehrer)
            .FirstOrDefaultAsync(a => a.Id == antragId);

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
        else if (antrag.Entscheidungen.All(e => e.Status == EntscheidungsStatus.Genehmigt))
        {
            antrag.Status = FreistellungsStatus.AlleLehrerGenehmigt;
        }

        await _dbContext.SaveChangesAsync();

        // Notify the student about the teacher's decision
        var kommentarZeile = string.IsNullOrWhiteSpace(dto.Kommentar)
            ? string.Empty
            : $"\nKommentar: {dto.Kommentar}";
        var entscheidungText = dto.Status == EntscheidungsStatus.Genehmigt ? "genehmigt" : "abgelehnt";
        await _notificationService.ScheduleNotificationAsync(
            antrag.Student,
            $"Freistellungsantrag {entscheidungText}",
            $"""
             Dein Freistellungsantrag für {FormatDateRange(antrag.DatumVon, antrag.DatumBis)} wurde von {lehrer.FirstName} {lehrer.LastName} {entscheidungText}.{kommentarZeile}
             Melde dich in der Afra-App an, um den aktuellen Status zu sehen.
             """,
            TimeSpan.FromMinutes(5)
        );

        // If all teachers have now approved, also notify the Sekretariat
        if (antrag.Status == FreistellungsStatus.AlleLehrerGenehmigt)
        {
            var sekretariat = await _dbContext.Personen
                .Where(p => p.GlobalPermissions.Contains(GlobalPermission.Sekretariat))
                .ToListAsync();

            foreach (var sekretariatMember in sekretariat)
                await _notificationService.ScheduleNotificationAsync(
                    sekretariatMember,
                    "Freistellungsantrag wartet auf Bestätigung",
                    $"""
                     Der Freistellungsantrag von {antrag.Student.FirstName} {antrag.Student.LastName} für {FormatDateRange(antrag.DatumVon, antrag.DatumBis)} wurde von allen Lehrkräften genehmigt.
                     Bitte melde dich in der Afra-App an, um den Antrag abschließend zu bestätigen.
                     """,
                    TimeSpan.FromMinutes(5)
                );
        }

        return new FreistellungsantragDto(antrag);
    }

    /// <summary>
    ///     Gets all leave requests where all teachers have approved, waiting for Sekretariat confirmation.
    /// </summary>
    public async Task<List<FreistellungsantragDto>> GetAntraegeForSekretariatAsync()
    {
        var antraege = await _dbContext.Freistellungsantraege
            .Include(a => a.Student)
            .Include(a => a.BetroffeneStunden)
            .ThenInclude(s => s.Lehrer)
            .Include(a => a.Entscheidungen)
            .ThenInclude(e => e.Lehrer)
            .Where(a => a.Status == FreistellungsStatus.AlleLehrerGenehmigt)
            .OrderBy(a => a.DatumVon)
            .ToListAsync();

        return antraege.Select(a => new FreistellungsantragDto(a)).ToList();
    }

    /// <summary>
    ///     Confirms a leave request that has been fully approved by all teachers.
    /// </summary>
    public async Task<FreistellungsantragDto> BestaetigeAntragAsync(Guid antragId)
    {
        var antrag = await _dbContext.Freistellungsantraege
            .Include(a => a.Student)
            .Include(a => a.BetroffeneStunden)
            .ThenInclude(s => s.Lehrer)
            .Include(a => a.Entscheidungen)
            .ThenInclude(e => e.Lehrer)
            .FirstOrDefaultAsync(a => a.Id == antragId);

        if (antrag is null)
            throw new KeyNotFoundException("Leave request not found.");

        if (antrag.Status != FreistellungsStatus.AlleLehrerGenehmigt)
            throw new InvalidOperationException("This leave request has not been fully approved by all teachers.");

        antrag.Status = FreistellungsStatus.Bestaetigt;
        await _dbContext.SaveChangesAsync();

        // Notify the student that their request has been officially confirmed
        await _notificationService.ScheduleNotificationAsync(
            antrag.Student,
            "Freistellungsantrag bestätigt",
            $"""
             Dein Freistellungsantrag für {FormatDateRange(antrag.DatumVon, antrag.DatumBis)} wurde vom Sekretariat bestätigt.
             Die Freistellung ist damit gültig. Melde dich in der Afra-App an, um die Details einzusehen.
             """,
            TimeSpan.FromMinutes(5)
        );

        return new FreistellungsantragDto(antrag);
    }

    /// <summary>
    ///     Gets all leave requests that the given teacher has already decided on.
    /// </summary>
    public async Task<List<FreistellungsantragDto>> GetProcessedAntraegeForLehrerAsync(Person lehrer)
    {
        var antraege = await _dbContext.Freistellungsantraege
            .Include(a => a.Student)
            .Include(a => a.BetroffeneStunden)
            .ThenInclude(s => s.Lehrer)
            .Include(a => a.Entscheidungen)
            .ThenInclude(e => e.Lehrer)
            .Where(a => a.Entscheidungen.Any(e =>
                e.LehrerId == lehrer.Id && e.Status != EntscheidungsStatus.Ausstehend))
            .OrderByDescending(a => a.DatumVon)
            .ToListAsync();

        return antraege.Select(a => new FreistellungsantragDto(a)).ToList();
    }

    /// <summary>
    ///     Gets all leave requests that have been fully processed by the Sekretariat
    ///     (either confirmed or rejected).
    /// </summary>
    public async Task<List<FreistellungsantragDto>> GetProcessedAntraegeForSekretariatAsync()
    {
        var antraege = await _dbContext.Freistellungsantraege
            .Include(a => a.Student)
            .Include(a => a.BetroffeneStunden)
            .ThenInclude(s => s.Lehrer)
            .Include(a => a.Entscheidungen)
            .ThenInclude(e => e.Lehrer)
            .Where(a => a.Status == FreistellungsStatus.Bestaetigt
                        || a.Status == FreistellungsStatus.Abgelehnt)
            .OrderByDescending(a => a.DatumVon)
            .ToListAsync();

        return antraege.Select(a => new FreistellungsantragDto(a)).ToList();
    }

    private static string FormatDateRange(DateOnly von, DateOnly bis)
        => von == bis
            ? $"den {von:dd.MM.yyyy}"
            : $"den Zeitraum {von:dd.MM.yyyy} – {bis:dd.MM.yyyy}";
}
