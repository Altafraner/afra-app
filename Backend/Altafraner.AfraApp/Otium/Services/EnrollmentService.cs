using Altafraner.AfraApp.Otium.Domain.Contracts.Rules;
using Altafraner.AfraApp.Otium.Domain.Contracts.Services;
using Altafraner.AfraApp.Otium.Domain.DTO.Katalog;
using Altafraner.AfraApp.Otium.Domain.DTO.Notiz;
using Altafraner.AfraApp.Otium.Domain.Models.TimeInterval;
using Altafraner.AfraApp.Schuljahr.Domain.Models;
using Altafraner.AfraApp.User.Domain.DTO;
using Altafraner.AfraApp.User.Domain.Models;
using Altafraner.Backbone.EmailSchedulingModule;
using Altafraner.Backbone.Utils;
using Microsoft.EntityFrameworkCore;
using Models_OtiumEinschreibung = Altafraner.AfraApp.Otium.Domain.Models.OtiumEinschreibung;
using Models_OtiumTermin = Altafraner.AfraApp.Otium.Domain.Models.OtiumTermin;
using Models_Person = Altafraner.AfraApp.User.Domain.Models.Person;

namespace Altafraner.AfraApp.Otium.Services;

/// <summary>
///     A service for handling enrollments.
/// </summary>
internal class EnrollmentService
{
    private readonly BlockHelper _blockHelper;
    private readonly AfraAppContext _dbContext;
    private readonly ILogger _logger;
    private readonly IRulesFactory _rulesFactory;
    private readonly INotificationService _notificationService;
    private readonly NotesService _notesService;

    /// <summary>
    ///     Constructs the EnrollmentService. Usually called by the DI container.
    /// </summary>
    public EnrollmentService(AfraAppContext dbContext,
        ILogger<EnrollmentService> logger,
        BlockHelper blockHelper,
        IRulesFactory rulesFactory,
        INotificationService notificationService,
        NotesService notesService)
    {
        _dbContext = dbContext;
        _logger = logger;
        _blockHelper = blockHelper;
        _rulesFactory = rulesFactory;
        _notificationService = notificationService;
        _notesService = notesService;
    }

    /// <summary>
    ///     Enrolls a user in a termin for the subblock starting at a given time.
    /// </summary>
    /// <param name="terminId">The is of the termin entity to enroll to</param>
    /// <param name="student">The student wanting to enroll</param>
    /// <returns>null, iff the user may not enroll into the termin; Otherwise the Termin entity.</returns>
    public async Task<Models_OtiumTermin?> EnrollAsync(Guid terminId, Models_Person student)
    {
        var termin = await _dbContext.OtiaTermine
            .Include(termin => termin.Block)
            .ThenInclude(block => block.Schultag)
            .Include(termin => termin.Otium)
            .ThenInclude(otium => otium.Kategorie)
            .Include(termin => termin.Tutor)
            .FirstOrDefaultAsync(t => t.Id == terminId);

        if (termin == null) return null;

        var mayEnroll = await MayEnroll(student, termin);
        if (!mayEnroll.IsValid) return null;

        var einschreibung = new Models_OtiumEinschreibung
        {
            Termin = termin,
            BetroffenePerson = student,
            Interval = _blockHelper.Get(termin.Block.SchemaId)!.Interval
        };

        _dbContext.OtiaEinschreibungen.Add(einschreibung);
        await _dbContext.SaveChangesAsync();
        return termin;
    }

    /// <summary>
    ///     Checks if a student is enrolled to a termin
    /// </summary>
    /// <param name="terminId">The termin the student could be enrolled to</param>
    /// <param name="personId">The person to check enrollment for</param>
    public async Task<bool> IsEnrolledToAsync(Guid terminId, Guid personId)
    {
        return await _dbContext.OtiaEinschreibungen.AnyAsync(e =>
            e.Termin.Id == terminId && e.BetroffenePerson.Id == personId);
    }

    /// <summary>
    ///     Enrolls a user in a termin for the date of the termin and all specified dates of the termin's recurrence.
    /// </summary>
    /// <param name="terminId">The id of the first termin to enroll in</param>
    /// <param name="dates">the dates to also enroll for</param>
    /// <param name="student">The student to enroll</param>
    /// <returns>A <see cref="MultiEnrollmentStatus" /> object with information on the success of all enrollments.</returns>
    /// <exception cref="KeyNotFoundException">No termin with the specified <paramref name="terminId" /> could be found.</exception>
    /// <exception cref="InvalidOperationException">
    ///     The user may not enroll for the termin with the specified
    ///     <paramref name="terminId" />
    /// </exception>
    public async Task<MultiEnrollmentStatus> EnrollAsync(Guid terminId, IEnumerable<DateOnly> dates,
        Models_Person student)
    {
        // Okay, this is ugly, but it works.
        var startingTermin = await _dbContext.OtiaTermine
            .Include(termin => termin.Block)
            .ThenInclude(block => block.Schultag)
            .Include(termin => termin.Otium)
            .ThenInclude(otium => otium.Kategorie)
            .Include(termin => termin.Wiederholung)
            .ThenInclude(wdh => wdh!.Termine)
            .ThenInclude(termin => termin.Block)
            .ThenInclude(block => block.Schultag)
            .Include(termin => termin.Wiederholung)
            .ThenInclude(wdh => wdh!.Termine)
            .ThenInclude(termin => termin.Otium)
            .ThenInclude(otium => otium.Kategorie)
            .AsSplitQuery()
            .FirstOrDefaultAsync(t => t.Id == terminId);

        if (startingTermin == null) throw new KeyNotFoundException("Der Termin konnte nicht gefunden werden.");
        var mayEnroll = await MayEnroll(student, startingTermin);

        if (!mayEnroll.IsValid) throw new InvalidOperationException("Der Nutzer darf sich nicht einschreiben.");
        var einschreibung = new Models_OtiumEinschreibung
        {
            Termin = startingTermin,
            BetroffenePerson = student,
            Interval = _blockHelper.Get(startingTermin.Block.SchemaId)!.Interval
        };

        List<DateOnly> success = [startingTermin.Block.SchultagKey];
        List<DateOnly> failure = [];
        _dbContext.OtiaEinschreibungen.Add(einschreibung);
        foreach (var date in dates)
        {
            var recurringTermin = startingTermin.Wiederholung?.Termine.FirstOrDefault(t => t.Block.SchultagKey == date);
            if (recurringTermin is null)
            {
                failure.Add(date);
                continue;
            }

            var mayEnrollReg = await MayEnroll(student, recurringTermin);
            if (!mayEnrollReg.IsValid)
            {
                failure.Add(date);
                continue;
            }

            var einschreibungRec = new Models_OtiumEinschreibung
            {
                Termin = recurringTermin,
                BetroffenePerson = student,
                Interval = _blockHelper.Get(recurringTermin.Block.SchemaId)!.Interval
            };
            _dbContext.OtiaEinschreibungen.Add(einschreibungRec);
            success.Add(recurringTermin.Block.SchultagKey);
        }

        await _dbContext.SaveChangesAsync();
        return new MultiEnrollmentStatus(success, failure);
    }

    /// <summary>
    ///     Unenrolls a user from a termin for the subblock starting at a given time.
    /// </summary>
    /// <param name="terminId">the id of the termin entity</param>
    /// <param name="student">the student wanting to enroll</param>
    /// <param name="force">
    ///     If true, will forcefully delete the users enrollment, even if normally not allowed. For use within
    ///     system components only.
    /// </param>
    /// <param name="save">If true, will persist changes to database. Useful for bulk operations.</param>
    /// <returns>null, if the user may not enroll with the given parameters; Otherwise the termin the user has unenrolled from.</returns>
    public async Task<Models_OtiumTermin?> UnenrollAsync(Guid terminId, Models_Person student, bool force = false,
        bool save = true)
    {
        var enrollment = await _dbContext.OtiaEinschreibungen
            .Include(e => e.Termin)
            .ThenInclude(t => t.Block)
            .ThenInclude(b => b.Schultag)
            .Include(e => e.Termin)
            .ThenInclude(t => t.Otium)
            .Include(e => e.BetroffenePerson)
            .FirstOrDefaultAsync(e => e.BetroffenePerson.Id == student.Id && e.Termin.Id == terminId);

        if (enrollment == null) return null;

        if (!force)
            try
            {
                var mayUnenroll = await MayUnenroll(student, enrollment);
                if (!mayUnenroll.IsValid) return null;
            }
            catch (InvalidOperationException)
            {
                return null;
            }

        _dbContext.OtiaEinschreibungen.Remove(enrollment);

        if (!save) return enrollment.Termin;

        await _dbContext.SaveChangesAsync();

        var sendNotification = force && !_blockHelper.IsBlockDoneOrRunning(enrollment.Termin.Block);
        if (sendNotification)
            await _notificationService.ScheduleNotificationAsync(enrollment.BetroffenePerson, "Abmeldung von Termin",
                $"""
                 Du wurdest aus dem Termin {enrollment.Termin.Bezeichnung} am {enrollment.Termin.Block.Schultag.Datum:dd.MM.yyyy} im Block {_blockHelper.Get(enrollment.Termin.Block.SchemaId)?.Bezeichnung ?? "unbekannt"} abgemeldet.

                 Schreibe dich für den Block ggf. erneut ein.
                 """,
                TimeSpan.FromMinutes(5));

        return enrollment.Termin;
    }

    /// <summary>
    ///     Gets all blocks that the user is not enrolled in.
    /// </summary>
    /// <param name="enrollments">The users enrollment</param>
    /// <param name="blocks">All blocks the user should be enrolled in</param>
    public IEnumerable<Block> GetNotEnrolledBlocks(IEnumerable<Models_OtiumEinschreibung> enrollments,
        IEnumerable<Block> blocks)
    {
        var enrolledBlocks = enrollments
            .Select(e => e.Termin.Block.Id)
            .Distinct();

        return blocks.Where(b => !enrolledBlocks.Contains(b.Id));
    }

    /// <summary>
    ///     Checks if a user may enroll in a termin for all subblocks in a termin.
    /// </summary>
    /// <param name="user">The user to check for</param>
    /// <param name="termin">The termin to check in</param>
    /// <returns></returns>
    public async Task<EinschreibungsPreview> GetEnrolmentPreview(Models_Person user, Models_OtiumTermin termin)
    {
        var terminEinschreibungen = await _dbContext.OtiaEinschreibungen
            .AsNoTracking()
            .Where(e => e.Termin == termin)
            .ToListAsync();

        var schema = _blockHelper.Get(termin.Block.SchemaId);
        if (schema == null)
        {
            _logger.LogWarning(
                "Schema with id {Id} not found. This should not happen. Please check the configuration.",
                termin.Block.SchemaId);
            throw new KeyNotFoundException(
                $"Schema with id {termin.Block.SchemaId} not found. This should not happen. Please check the configuration.");
        }

        var countEnrolled = terminEinschreibungen.Count;
        var usersEnrollment = await _dbContext.OtiaEinschreibungen
            .Include(e => e.Termin)
            .ThenInclude(e => e.Block)
            .FirstOrDefaultAsync(e => e.Termin == termin && e.BetroffenePerson == user);
        var changeResult = usersEnrollment != null
            ? await MayUnenroll(user, usersEnrollment)
            : await MayEnroll(user, termin);
        var notes = await _notesService.GetNotesAsync(user.Id, termin.Block.Id);
        var myNote = notes.FirstOrDefault(n => n.AuthorId == user.Id);
        return new EinschreibungsPreview(countEnrolled,
            changeResult.IsValid,
            string.Join(Environment.NewLine, changeResult.Messages),
            usersEnrollment != null,
            myNote is not null ? new Notiz(myNote) : null,
            notes.Select(n => new Notiz(n)),
            schema.Interval);
    }

    /// <summary>
    ///     Calculates the load for a given termin.
    /// </summary>
    /// <param name="termin">The termin for which to calculate the load.</param>
    /// <returns>The calculated load as a double, or null if MaxEinschreibungen is null.</returns>
    public async Task<int?> GetLoadPercent(Models_OtiumTermin termin)
    {
        if (termin.MaxEinschreibungen is null)
            return null;

        var numEnrollments = await _dbContext.OtiaEinschreibungen.AsNoTracking()
            .CountAsync(e => e.Termin == termin);

        return termin.MaxEinschreibungen == null
            ? null
            : (int)Math.Round((double)numEnrollments / termin.MaxEinschreibungen.Value * 100);
    }

    /// <summary>
    ///     Moves a student to a termin. This will remove the student from any other termins in the same block.
    /// </summary>
    /// <param name="studentId">The id of the student to move</param>
    /// <param name="toTerminId">The id of the termin to move to</param>
    /// <returns>The id of the termin the student was previously enrolled in and the id of the block affected</returns>
    /// <exception cref="KeyNotFoundException">Eiter the student or termin could not be found</exception>
    public async Task<(Guid oldTerminId, Guid blockId)> ForceMove(Guid studentId, Guid toTerminId)
    {
        var toTermin = await _dbContext.OtiaTermine
            .Include(t => t.Block)
            .FirstOrDefaultAsync(t => t.Id == toTerminId);
        if (toTermin == null)
            throw new KeyNotFoundException("Der Termin konnte nicht gefunden werden.");

        var currentEnrollment = await _dbContext.OtiaEinschreibungen
            .Include(e => e.Termin)
            .Where(e => e.BetroffenePerson.Id == studentId && e.Termin.Block == toTermin.Block)
            .ToListAsync();
        _dbContext.OtiaEinschreibungen.RemoveRange(currentEnrollment);

        var blockSchema = _blockHelper.Get(toTermin.Block.SchemaId)!;
        await _dbContext.OtiaEinschreibungen.AddAsync(new Models_OtiumEinschreibung
        {
            BetroffenePerson = await _dbContext.Personen.FindAsync(studentId)
                               ?? throw new KeyNotFoundException("Die Person konnte nicht gefunden werden."),
            Termin = toTermin,
            Interval = blockSchema.Interval
        });

        await _dbContext.SaveChangesAsync();
        return (GetOldTerminId(),
            toTermin.Block.Id);

        Guid GetOldTerminId()
        {
            var oldEnrollment = currentEnrollment
                .OrderBy(e => e.Interval.End)
                .LastOrDefault();

            return oldEnrollment is null ? Guid.Empty : oldEnrollment.Termin.Id;
        }
    }

    /// <summary>
    ///     Moves a student from one running termin to another, keeping track of the time the change was made.
    /// </summary>
    /// <param name="studentId">The id of the student to move</param>
    /// <param name="fromTerminId">
    ///     The id of the termin the student is moving from. Use Guid.Empty if you expect the student to
    ///     not be enrolled.
    /// </param>
    /// <param name="toTerminId">The id of the termin the student should be enrolled for</param>
    /// <exception cref="KeyNotFoundException">Either the student, ore one of the termine could not be found.</exception>
    /// <exception cref="InvalidOperationException">
    ///     One of the termines is not running. Consider using <see cref="ForceMove" />
    /// </exception>
    public async Task ForceMoveNow(Guid studentId, Guid fromTerminId, Guid toTerminId)
    {
        var now = DateTime.Now;
        var nowTime = TimeOnly.FromDateTime(now);
        var today = DateOnly.FromDateTime(now);
        if (fromTerminId != Guid.Empty)
        {
            // EF Core struggles with the OrderBy here, so I'll load all the einschreibungen and order them in memory.
            var fromEinschreibung = (await _dbContext.OtiaEinschreibungen
                    .Include(e => e.Termin)
                    .ThenInclude(e => e.Block)
                    .Where(e => e.BetroffenePerson.Id == studentId && e.Termin.Id == fromTerminId)
                    .ToListAsync())
                .OrderByDescending(e => e.Interval.End)
                .FirstOrDefault();
            if (fromEinschreibung == null)
                throw new KeyNotFoundException("Die Einschreibung konnte nicht gefunden werden.");

            if (fromEinschreibung.Termin.Block.SchultagKey != today || !fromEinschreibung.Interval.Contains(nowTime))
                throw new InvalidOperationException(
                    "Sie können keine Einschreibung jetzt beenden, wenn die Einschreibung nicht grade stattfindet!");

            fromEinschreibung.Interval = new TimeOnlyInterval(fromEinschreibung.Interval.Start, nowTime);
        }

        var toTermin = await _dbContext.OtiaTermine
            .Include(t => t.Block)
            .FirstOrDefaultAsync(t => t.Id == toTerminId);

        if (toTermin == null)
        {
            await _dbContext.SaveChangesAsync();
            return;
        }

        if (toTermin.Block.SchultagKey != today)
            throw new InvalidOperationException(
                "Sie können keine Einschreibung jetzt beginnen, wenn der Termin nicht heute ist!");

        var blockSchema = _blockHelper.Get(toTermin.Block.SchemaId)!;
        if (!blockSchema.Interval.Contains(nowTime))
            throw new InvalidOperationException(
                "Sie können keine Einschreibung jetzt beginnen, wenn der Termin nicht grade stattfindet!");

        _dbContext.OtiaEinschreibungen.Add(new Models_OtiumEinschreibung
        {
            BetroffenePerson = await _dbContext.Personen.FindAsync(studentId)
                               ?? throw new KeyNotFoundException("Die Person konnte nicht gefunden werden."),
            Termin = toTermin,
            Interval = new TimeOnlyInterval(nowTime, blockSchema.Interval.End)
        });

        await _dbContext.SaveChangesAsync();
    }


    /// <summary>
    ///     Gets a list of all persons that are not enrolled for a specific day.
    /// </summary>
    /// <param name="date">The date to get the Persons for</param>
    /// <returns>A set containing all found persons.</returns>
    public async Task<HashSet<PersonInfoMinimal>> GetNotEnrolledPersonsForDayAsync(DateOnly date)
    {
        var allBlocks = await _dbContext.Blocks
            .AsNoTracking()
            .Where(b => b.SchultagKey == date)
            .ToListAsync();

        var mandatoryBlocks = allBlocks
            .Where(b => _blockHelper.Get(b.SchemaId)?.Verpflichtend ?? false)
            .ToList();

        if (mandatoryBlocks.Count == 0) return [];

        HashSet<PersonInfoMinimal> missingPersons = [];

        foreach (var block in mandatoryBlocks)
        {
            var missingPersonsInBlock = await _dbContext.Personen
                .Where(p => p.Rolle != Rolle.Tutor)
                .Where(p => !_dbContext.OtiaEinschreibungen
                    .Any(e => e.BetroffenePerson.Id == p.Id &&
                              e.BetroffenePerson.Rolle == Rolle.Mittelstufe &&
                              e.Termin.Block.Id == block.Id))
                .Select(p => new PersonInfoMinimal
                {
                    Id = p.Id,
                    Vorname = p.FirstName,
                    Nachname = p.LastName,
                    Rolle = p.Rolle
                })
                .ToHashSetAsync();

            missingPersons.UnionWith(missingPersonsInBlock);
        }

        return missingPersons;
    }

    private async Task<RuleStatus> MayUnenroll(Models_Person user, Models_OtiumEinschreibung einschreibung)
    {
        var independentRules = _rulesFactory.GetIndependentRules();
        foreach (var rule in independentRules)
        {
            var result = await rule.MayUnenrollAsync(user, einschreibung);
            if (!result.IsValid) return result;
        }

        var blockRules = _rulesFactory.GetBlockRules();
        var usersEnrollmentsInBlock = await GetPersonsEnrollmentsInBlock(user, einschreibung.Termin.Block);
        foreach (var rule in blockRules)
        {
            var result = await rule.MayUnenrollAsync(user, usersEnrollmentsInBlock, einschreibung);
            if (!result.IsValid) return result;
        }

        var (schultageInWeek, usersEnrollmentsInWeek) =
            await GetSchultageAndPersonsEnrollmentsInWeek(user, einschreibung.Termin.Block.SchultagKey);
        var weekRules = _rulesFactory.GetWeekRules();

        foreach (var rule in weekRules)
        {
            var result = await rule.MayUnenrollAsync(user, schultageInWeek, usersEnrollmentsInWeek, einschreibung);
            if (!result.IsValid) return result;
        }

        return RuleStatus.Valid;
    }

    private async Task<RuleStatus> MayEnroll(Models_Person user, Models_OtiumTermin termin)
    {
        var independentRules = _rulesFactory.GetIndependentRules();
        foreach (var rule in independentRules)
        {
            var result = await rule.MayEnrollAsync(user, termin);
            if (!result.IsValid) return result;
        }

        var blockRules = _rulesFactory.GetBlockRules();
        var usersEnrollmentsInBlock = await GetPersonsEnrollmentsInBlock(user, termin.Block);
        foreach (var rule in blockRules)
        {
            var result = await rule.MayEnrollAsync(user, usersEnrollmentsInBlock, termin);
            if (!result.IsValid) return result;
        }

        var (schultageInWeek, usersEnrollmentsInWeek) =
            await GetSchultageAndPersonsEnrollmentsInWeek(user, termin.Block.SchultagKey);
        var weekRules = _rulesFactory.GetWeekRules();

        foreach (var rule in weekRules)
        {
            var result = await rule.MayEnrollAsync(user, schultageInWeek, usersEnrollmentsInWeek, termin);
            if (!result.IsValid) return result;
        }

        return RuleStatus.Valid;
    }

    private async Task<List<Models_OtiumEinschreibung>> GetPersonsEnrollmentsInBlock(Models_Person user, Block block)
    {
        return await _dbContext.OtiaEinschreibungen
            .Include(e => e.Termin)
            .ThenInclude(t => t.Block)
            .Include(e => e.Termin)
            .ThenInclude(t => t.Otium)
            .ThenInclude(o => o.Kategorie)
            .Where(e => e.BetroffenePerson == user && e.Termin.Block == block)
            .ToListAsync();
    }

    private async Task<(List<Schultag> schultage, List<Models_OtiumEinschreibung>)>
        GetSchultageAndPersonsEnrollmentsInWeek(
            Models_Person user, DateOnly dateInWeek)
    {
        var monday = dateInWeek.GetStartOfWeek();
        var endOfWeek = monday.AddDays(7);
        var usersEnrollmentsInWeek = await _dbContext.OtiaEinschreibungen
            .Include(e => e.Termin)
            .ThenInclude(t => t.Block)
            .Include(e => e.Termin)
            .ThenInclude(t => t.Otium)
            .ThenInclude(o => o.Kategorie)
            .Where(e => e.BetroffenePerson == user)
            .Where(e => e.Termin.Block.SchultagKey >= monday && e.Termin.Block.SchultagKey < endOfWeek)
            .ToListAsync();
        var schultageInWeek = await _dbContext.Schultage
            .Include(s => s.Blocks)
            .Where(s => s.Datum >= monday && s.Datum < endOfWeek)
            .ToListAsync();
        return (schultageInWeek, usersEnrollmentsInWeek);
    }
}
