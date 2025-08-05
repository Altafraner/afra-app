using Afra_App.Backbone.Domain.TimeInterval;
using Afra_App.Otium.Domain.DTO.Katalog;
using Afra_App.Otium.Domain.Models;
using Afra_App.Otium.Domain.Models.Schuljahr;
using Afra_App.User.Domain.DTO;
using Afra_App.User.Domain.Models;
using Afra_App.User.Services;
using Microsoft.EntityFrameworkCore;
using OtiumEinschreibung = Afra_App.Otium.Domain.Models.OtiumEinschreibung;
using OtiumTermin = Afra_App.Otium.Domain.Models.OtiumTermin;
using Person = Afra_App.User.Domain.Models.Person;
using Schultag = Afra_App.Otium.Domain.Models.Schuljahr.Schultag;

namespace Afra_App.Otium.Services;

/// <summary>
///     A service for handling enrollments.
/// </summary>
public class EnrollmentService
{
    private readonly BlockHelper _blockHelper;
    private readonly AfraAppContext _dbContext;
    private readonly KategorieService _kategorieService;
    private readonly ILogger _logger;
    private readonly UserService _userService;

    /// <summary>
    ///     Constructs the EnrollmentService. Usually called by the DI container.
    /// </summary>
    public EnrollmentService(AfraAppContext dbContext,
        KategorieService kategorieService,
        ILogger<EnrollmentService> logger,
        BlockHelper blockHelper, UserService userService)
    {
        _dbContext = dbContext;
        _kategorieService = kategorieService;
        _logger = logger;
        _blockHelper = blockHelper;
        _userService = userService;
    }

    /// <summary>
    ///     Enrolls a user in a termin for the subblock starting at a given time.
    /// </summary>
    /// <param name="terminId">The is of the termin entity to enroll to</param>
    /// <param name="student">The student wanting to enroll</param>
    /// <returns>null, iff the user may not enroll into the termin; Otherwise the Termin entity.</returns>
    public async Task<OtiumTermin?> EnrollAsync(Guid terminId, Person student)
    {
        var termin = await _dbContext.OtiaTermine
            .Include(termin => termin.Block)
            .ThenInclude(block => block.Schultag)
            .Include(termin => termin.Otium)
            .ThenInclude(otium => otium.Kategorie)
            .Include(termin => termin.Tutor)
            .FirstOrDefaultAsync(t => t.Id == terminId);

        if (termin == null) return null;

        var (mayEnroll, _) = await MayEnroll(student, termin);
        if (!mayEnroll) return null;

        var einschreibung = new OtiumEinschreibung
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
    public async Task<MultiEnrollmentStatus> EnrollAsync(Guid terminId, IEnumerable<DateOnly> dates, Person student)
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
        var (mayEnroll, _) = await MayEnroll(student, startingTermin);

        if (!mayEnroll) throw new InvalidOperationException("Der Nutzer darf sich nicht einschreiben.");
        var einschreibung = new OtiumEinschreibung
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

            var (mayEnrollRec, _) = await MayEnroll(student, recurringTermin);
            if (!mayEnrollRec)
            {
                failure.Add(date);
                continue;
            }

            var einschreibungRec = new OtiumEinschreibung
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
    /// <returns>null, if the user may not enroll with the given parameters; Otherwise the termin the user has enrolled in.</returns>
    public async Task<OtiumTermin?> UnenrollAsync(Guid terminId, Person student, bool force = false, bool save = true)
    {
        var enrollment = await _dbContext.OtiaEinschreibungen
            .Include(e => e.Termin)
            .ThenInclude(t => t.Block)
            .ThenInclude(b => b.Schultag)
            .FirstOrDefaultAsync(e => e.BetroffenePerson.Id == student.Id && e.Termin.Id == terminId);

        if (enrollment == null) return null;

        if (!force)
            try
            {
                var (mayUnenroll, _) = await MayUnenroll(student, enrollment.Termin);
                if (!mayUnenroll) return null;
            }
            catch (InvalidOperationException)
            {
                return null;
            }

        _dbContext.OtiaEinschreibungen.Remove(enrollment);

        if (save) await _dbContext.SaveChangesAsync();
        return enrollment.Termin;
    }

    /// <summary>
    ///     Gets the times of all non-optional blocks that the student is not enrolled in.
    /// </summary>
    /// <param name="blocks">The blocks to check for</param>
    /// <param name="user">The users to get the times the user is not enrolled in</param>
    /// <returns>A timeline containing all times the user must enroll in but has not done so.</returns>
    public Timeline<TimeOnly> GetNotEnrolledTimes(IEnumerable<Block> blocks, Person user)
    {
        var timeline = new Timeline<TimeOnly>();

        var blockList = blocks.ToList();

        var notEnrolledBlocks = from block in _dbContext.Blocks
                                where blockList.Contains(block)
                                join einschreibung in _dbContext.OtiaEinschreibungen
                                    on block.Id equals einschreibung.Termin.Block.Id
                                    into einschreibungen
                                where einschreibungen.All(e => e.BetroffenePerson != user)
                                select block.SchemaId;

        foreach (var schemaId in notEnrolledBlocks)
        {
            var schema = _blockHelper.Get(schemaId)!;
            if (schema.Verpflichtend) timeline.Add(schema.Interval);
        }

        return timeline;
    }

    /// <summary>
    ///     Gets the times of all non-optional blocks that the student is not enrolled in.
    /// </summary>
    /// <param name="blocks">The blocks to check for</param>
    /// <param name="einschreibungen">The enrollments the user is enrolled in</param>
    /// <returns>A timeline containing all times the user must enroll in but has not done so.</returns>
    public Timeline<TimeOnly> GetNotEnrolledTimes(IEnumerable<Block> blocks, IEnumerable<OtiumEinschreibung> einschreibungen)
    {
        var timeline = new Timeline<TimeOnly>();
        var enrolledBlocks = einschreibungen
            .Select(e => e.Termin.Block)
            .DistinctBy(e => e.Id);

        var notEnrolledBlocks = blocks.Where(b => !enrolledBlocks.Contains(b));
        foreach (var block in notEnrolledBlocks)
        {
            var schema = _blockHelper.Get(block.SchemaId)!;
            if (schema.Verpflichtend) timeline.Add(schema.Interval);
        }

        return timeline;
    }

    /// <summary>
    ///     Gets all blocks that the user is not enrolled in.
    /// </summary>
    /// <param name="enrollments">The users enrollment</param>
    /// <param name="blocks">All blocks the user should be enrolled in</param>
    public IEnumerable<Block> GetNotEnrolledBlocks(IEnumerable<OtiumEinschreibung> enrollments, IEnumerable<Block> blocks)
    {
        var enrolledBlocks = enrollments
            .Select(e => e.Termin.Block.Id)
            .Distinct();

        return blocks.Where(b => !enrolledBlocks.Contains(b.Id));
    }

    /// <summary>
    ///     Checks if a set of einschreibungen covers all non-optional blocks of a schultag.
    /// </summary>
    /// <param name="schultag">The day to check for</param>
    /// <param name="einschreibungen">The set of enrollments to aggregate the intervals from</param>
    /// <returns>True, iff all non optional blocks are enrolled</returns>
    public bool AreAllNonOptionalBlocksEnrolled(Schultag schultag, IEnumerable<OtiumEinschreibung> einschreibungen)
    {
        var blocksOnSchoolday = schultag.Blocks;
        var blocksEnrolled = einschreibungen
            .Select(e => e.Termin.Block)
            .Where(b => b.Schultag == schultag)
            .Distinct();

        return blocksOnSchoolday.Count == blocksEnrolled.Count();
    }

    /// <summary>
    ///     Gets all categories that are required for a set of enrollments and not included in the categories of enrollments.
    /// </summary>
    /// <param name="enrollments">The enrollments to exclude the (transitive) categories from</param>
    /// <returns>
    ///     A List of all Bezeichnungen of <see cref="OtiumKategorie">Kategorien</see> that are required but not covered by the
    ///     <paramref name="enrollments" />
    /// </returns>
    public async Task<List<string>> GetMissingKategories(IEnumerable<OtiumEinschreibung> enrollments)
    {
        // Get required categories
        var requiredKategories = (await _kategorieService.GetRequiredKategorienAsync()).Select(c => c.Id).ToHashSet();
        foreach (var enrollment in enrollments)
        {
            if (enrollment.Interval.Duration < _blockHelper.Get(enrollment.Termin.Block.SchemaId)!.Interval.Duration)
                continue;
            var requiredParent = await _kategorieService.GetRequiredParentIdAsync(enrollment.Termin.Otium.Kategorie);
            if (requiredParent != null) requiredKategories.Remove(requiredParent.Value);
        }

        return await _dbContext.OtiaKategorien
            .Where(k => requiredKategories.Contains(k.Id))
            .Select(k => k.Bezeichnung)
            .ToListAsync();
    }

    /// <summary>
    ///     Checks if a set of einschreibungen fulfills the required kategories for each week.
    /// </summary>
    /// <param name="allEinschreibungen">A list of einschreibungen to check if it fulfils the required Kategorien</param>
    /// <returns>
    ///     A <see cref="Dictionary{TKey,TValue}" /> with <see cref="DateTimeInterval" />s representing the weeks as keys
    ///     and a boolean value that is true iff the rule is fulfilled
    /// </returns>
    public async Task<Dictionary<DateOnly, bool>> CheckAllKategoriesInWeeks(
        List<OtiumEinschreibung> allEinschreibungen)
    {
        var requiredKategories = (await _kategorieService.GetRequiredKategorienAsync())
            .Select(k => k.Id)
            .ToList();

        var einschreibungenByWeek = new Dictionary<DateOnly, List<OtiumEinschreibung>>();
        var blockDurations = allEinschreibungen.Select(e => e.Termin.Block.SchemaId).Distinct()
            .ToDictionary(id => id, id => _blockHelper.Get(id)!.Interval.Duration);
        foreach (var einschreibung in allEinschreibungen)
        {
            var day = einschreibung.Termin.Block.Schultag.Datum;
            var monday = day.AddDays(-(int)day.DayOfWeek + 1);
            einschreibungenByWeek.TryAdd(monday, []);
            einschreibungenByWeek[monday].Add(einschreibung);
        }

        var resultByWeek = new Dictionary<DateOnly, bool>();
        foreach (var (week, weekEinschreibungen) in einschreibungenByWeek)
        {
            // Check if the user is enrolled in all required kategories
            var localRequiredKategories = requiredKategories.ToHashSet();
            foreach (var einschreibung in weekEinschreibungen)
            {
                if (einschreibung.Interval.Duration < blockDurations[einschreibung.Termin.Block.SchemaId])
                    continue;
                var requiredParent =
                    await _kategorieService.GetRequiredParentIdAsync(einschreibung.Termin.Otium.Kategorie);
                if (requiredParent != null) localRequiredKategories.Remove(requiredParent.Value);
            }

            var kategorieRuleFulfilled = localRequiredKategories.Count == 0;
            resultByWeek[week] = kategorieRuleFulfilled;
        }

        return resultByWeek;
    }

    /// <summary>
    ///     Checks if a user may enroll in a termin for all subblocks in a termin.
    /// </summary>
    /// <param name="user">The user to check for</param>
    /// <param name="termin">The termin to check in</param>
    /// <returns></returns>
    public async Task<EinschreibungsPreview> GetEnrolmentPreview(Person user, OtiumTermin termin)
    {
        var terminEinschreibungen = await _dbContext.OtiaEinschreibungen.AsNoTracking()
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
            .FirstOrDefaultAsync(e => e.Termin == termin && e.BetroffenePerson == user);
        var (mayEdit, reason) = usersEnrollment != null
            ? await MayUnenroll(user, termin)
            : await MayEnroll(user, termin, countEnrolled);
        return new EinschreibungsPreview(countEnrolled, mayEdit, reason, usersEnrollment != null,
            schema.Interval);
    }

    /// <summary>
    ///     Calculates the load for a given termin.
    /// </summary>
    /// <param name="termin">The termin for which to calculate the load.</param>
    /// <returns>The calculated load as a double, or null if MaxEinschreibungen is null.</returns>
    public async Task<int?> GetLoadPercent(OtiumTermin termin)
    {
        if (termin.MaxEinschreibungen is null)
            return null;

        var numEnrollments = await _dbContext.OtiaEinschreibungen.AsNoTracking()
            .CountAsync(e => e.Termin == termin);

        return termin.MaxEinschreibungen == null
            ? null
            : (int)Math.Round((double)numEnrollments / termin.MaxEinschreibungen.Value) * 100;
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
        await _dbContext.OtiaEinschreibungen.AddAsync(new OtiumEinschreibung
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
            // EF Core struggles with the OrderBy here, so i'll load all the einschreibungen and order them in memory.
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
            throw new KeyNotFoundException("Der Termin konnte nicht gefunden werden.");

        if (toTermin.Block.SchultagKey != today)
            throw new InvalidOperationException(
                "Sie können keine Einschreibung jetzt beginnen, wenn der Termin nicht heute ist!");

        var blockSchema = _blockHelper.Get(toTermin.Block.SchemaId)!;
        if (!blockSchema.Interval.Contains(nowTime))
            throw new InvalidOperationException(
                "Sie können keine Einschreibung jetzt beginnen, wenn der Termin nicht grade stattfindet!");

        _dbContext.OtiaEinschreibungen.Add(new OtiumEinschreibung
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
            .Where(b => _blockHelper.Get(b.SchemaId)?.Verpflichtend == true)
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
                    Vorname = p.Vorname,
                    Nachname = p.Nachname,
                    Rolle = p.Rolle
                })
                .ToHashSetAsync();

            missingPersons.UnionWith(missingPersonsInBlock);
        }

        return missingPersons;
    }

    /// <summary>
    ///     Gets a list of all students that are missing required categories in a week starting at the given monday.
    /// </summary>
    /// <param name="monday">The monday the week starts on</param>
    public async Task<Dictionary<Person, HashSet<string>>> GetStudentsWithMissingCategoriesInWeek(DateOnly monday)
    {
        var endOfWeek = monday.AddDays(7);
        var einschreibungenByUser = await _dbContext.Personen
            .Where(person => person.Rolle == Rolle.Mittelstufe)
            .GroupJoin(
                _dbContext.OtiaEinschreibungen
                    .Include(e => e.Termin)
                    .ThenInclude(e => e.Otium)
                    .ThenInclude(o => o.Kategorie)
                    .Include(e => e.Termin)
                    .ThenInclude(e => e.Block)
                    .Where(e => e.Termin.Block.SchultagKey >= monday && e.Termin.Block.SchultagKey < endOfWeek),
                person => person,
                einschreibung => einschreibung.BetroffenePerson,
                (person, einschreibungen) => new { person, einschreibungen })
            .ToListAsync();

        var result = new Dictionary<Person, HashSet<string>>();
        foreach (var userWithEnrollments in einschreibungenByUser)
        {
            var missingCategories = await GetMissingKategories(userWithEnrollments.einschreibungen);
            if (missingCategories.Count == 0)
                continue;
            if (!result.ContainsKey(userWithEnrollments.person)) result[userWithEnrollments.person] = [];
            result[userWithEnrollments.person].UnionWith(missingCategories);
        }

        var allUsers = await _userService.GetUsersWithRoleAsync(Rolle.Mittelstufe);
        var allCategories = (await _kategorieService.GetRequiredKategorienAsync())
            .Select(c => c.Bezeichnung)
            .ToHashSet();

        // Ensure all users without enrollments are included
        foreach (var user in allUsers) result.TryAdd(user, allCategories);

        return result;
    }

    private (bool MayUnEnroll, string? Reason) CommonMayUnEnroll(Person user, OtiumTermin termin)
    {
        var schema = _blockHelper.Get(termin.Block.SchemaId);

        if (schema == null)
        {
            _logger.LogWarning("A specified block id {id} cannot be found. Denying enrollment.", termin.Block.SchemaId);
            return (false, "Ein interner Fehler ist aufgetreten");
        }

        var startDateTime = new DateTime(termin.Block.Schultag.Datum, schema.Interval.Start);
        if (startDateTime <= DateTime.Now) return (false, "Der Termin hat bereits begonnen.");

        if (user.Rolle is not Rolle.Mittelstufe and not Rolle.Oberstufe)
            return (false, "Nur Schüler:innen können sich einschreiben.");

        return (true, null);
    }

    private Task<(bool MayUnenroll, string? Reason)> MayUnenroll(Person user, OtiumTermin termin)
    {
        var common = CommonMayUnEnroll(user, termin);
        if (!common.MayUnEnroll) return Task.FromResult(common);

        return Task.FromResult<(bool MayUnenroll, string? Reason)>((true, null));
    }

    private async Task<(bool, string?)> MayEnroll(Person user, OtiumTermin termin)
    {
        var countEnrolled = await _dbContext.OtiaEinschreibungen.AsNoTracking()
            .CountAsync(e => e.Termin == termin);

        return await MayEnroll(user, termin, countEnrolled);
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
    private async Task<(bool MayEnroll, string? Reason)> MayEnroll(Person user, OtiumTermin termin,
        int countEnrolled)
    {
        var common = CommonMayUnEnroll(user, termin);
        // Check common conditions with unenrollment
        if (!common.MayUnEnroll) return common;

        // Check if canceled
        if (termin.IstAbgesagt)
            return (false, "Der Termin wurde abgesagt.");

        // Check if the termin is already full
        if (termin.MaxEinschreibungen is not null && countEnrolled >= termin.MaxEinschreibungen)
            return (false, "Der Termin ist bereits vollständig belegt.");

        // Get enrollments for the same block
        var parallelEnrollment = await _dbContext.OtiaEinschreibungen
            .AnyAsync(e => e.BetroffenePerson == user && e.Termin.Block.Id == termin.Block.Id);

        if (parallelEnrollment)
            return (false, "Du bist bereits zur selben Zeit eingeschrieben");

        // Further rules do not apply to students in the Oberstufe
        if (user.Rolle == Rolle.Oberstufe)
            return (true, null);

        // Check if the user is adhering to the required categories
        var lastAvailableBlockRuleFulfilled = await LastAvailableBlockRuleFulfilled(user, termin);
        return lastAvailableBlockRuleFulfilled is LastAvailableBlockRuleStatus.Fulfillable
            or LastAvailableBlockRuleStatus.ImpossibleButHelping
            ? (true, null)
            : (false,
                "Du bist nicht in allen erforderlichen Kategorien eingeschrieben. Durch diese Einschreibung wäre das nicht mehr möglich.");
    }

    // Come here for some hideous shit; Optimizing this is a problem for future me.
    // This currently needs three separate SQL-Queries + loading all categories.
    private async Task<LastAvailableBlockRuleStatus> LastAvailableBlockRuleFulfilled(Person user, OtiumTermin termin)
    {
        // Find all required categories the user is not enrolled to. -> notEnrolled[]
        // This will break if we start having blocks on sundays
        var now = DateTime.Now;
        var time = TimeOnly.FromDateTime(now);
        var today = DateOnly.FromDateTime(now);
        var firstDayOfWeek = termin.Block.Schultag.Datum.AddDays(-(int)termin.Block.Schultag.Datum.DayOfWeek + 1);
        var lastDayOfWeek = firstDayOfWeek.AddDays(7);
        var weekInterval = new DateTimeInterval(new DateTime(firstDayOfWeek, TimeOnly.MinValue),
            TimeSpan.FromDays(7));

        var usersEnrollmentsInWeek = await _dbContext.OtiaEinschreibungen
            .Where(e => e.BetroffenePerson == user)
            .Include(e => e.Termin)
            .ThenInclude(t => t.Block)
            .Include(e => e.Termin)
            .ThenInclude(e => e.Otium)
            .ThenInclude(e => e.Kategorie)
            .Where(e => DateOnly.FromDateTime(weekInterval.Start) <= e.Termin.Block.Schultag.Datum &&
                        e.Termin.Block.Schultag.Datum < DateOnly.FromDateTime(weekInterval.End))
            .ToListAsync();

        var usersBlocks = usersEnrollmentsInWeek.Select(e => e.Termin.Block.Id);
        var blockDurations = usersEnrollmentsInWeek
            .ToDictionary(e => e.Termin.Block.Id, e => _blockHelper.Get(e.Termin.Block.SchemaId)!.Interval.Duration);

        var usersCategoriesInWeek = usersEnrollmentsInWeek
            .Where(e => e.Interval.Duration >= blockDurations[e.Termin.Block.Id])
            .Select(e => e.Termin.Otium.Kategorie)
            .Distinct()
            .ToList();

        var requiredCategories = (await _kategorieService.GetRequiredKategorienAsync())
            .Select(k => k.Id)
            .ToHashSet();

        // Once we have async linq we can do this with a set minus
        foreach (var cat in usersCategoriesInWeek)
            if (await _kategorieService.GetRequiredParentIdAsync(cat) is { } required)
                requiredCategories.Remove(required);

        if (requiredCategories.Count == 0)
            return LastAvailableBlockRuleStatus.Fulfillable;

        var allowedSchemas = _blockHelper.GetAll()
            .Where(b => b.Interval.Start > time)
            .Select(b => b.Id)
            .ToHashSet();

        var openBlocksInWeekWhenUnenrolled = await _dbContext.Blocks
            .Where(b => b.Schultag.Datum >= firstDayOfWeek && b.Schultag.Datum < lastDayOfWeek &&
                        (b.Schultag.Datum > today ||
                         (b.Schultag.Datum == today && allowedSchemas.Contains(b.SchemaId))))
            .Where(b => !usersBlocks.Contains(b.Id))
            .Include(b => b.Schultag)
            .ToListAsync();

        var openBlocksInWeekWhenEnrolled = openBlocksInWeekWhenUnenrolled
            .Where(b => b.Id != termin.Block.Id)
            .ToList();

        var terminsRequiredCategory = await _kategorieService.GetRequiredParentIdAsync(termin.Otium.Kategorie);

        var blockCategories = new Dictionary<Guid, HashSet<Guid>>();

        var catsByBlock = await _dbContext.OtiaTermine
            .Where(t => !t.IstAbgesagt)
            .Where(t => openBlocksInWeekWhenUnenrolled.Contains(t.Block))
            .GroupBy(t => t.Block.Id)
            .Select(e => new { Block = e.Key, Category = e.Select(t => t.Otium.Kategorie).Distinct().ToHashSet() })
            .ToDictionaryAsync(t => t.Block, t => t.Category);

        foreach (var block in openBlocksInWeekWhenUnenrolled)
        {
            blockCategories.TryAdd(block.Id, []);
            var cats = catsByBlock.TryGetValue(block.Id, out var set) ? set : [];

            foreach (var cat in cats)
            {
                var reqCat = await _kategorieService.GetRequiredParentIdAsync(cat);
                if (reqCat is not null && requiredCategories.Contains(reqCat.Value))
                    blockCategories[block.Id].Add(reqCat.Value);
            }
        }

        var blockIds = openBlocksInWeekWhenEnrolled.Select(b => b.Id).ToArray();

        var terminsRequiredCategoryStillRequired = terminsRequiredCategory is not null &&
                                                   requiredCategories.Contains(terminsRequiredCategory.Value);
        if (terminsRequiredCategory != null) requiredCategories.Remove(terminsRequiredCategory.Value);

        // Check if fulfillment is possible when enrolled
        if (Backtrack([])) return LastAvailableBlockRuleStatus.Fulfillable;

        // Check if fulfillment is at all possible
        if (terminsRequiredCategoryStillRequired) requiredCategories.Add(terminsRequiredCategory!.Value);
        blockIds = openBlocksInWeekWhenUnenrolled.Select(b => b.Id).ToArray();
        if (Backtrack([])) return LastAvailableBlockRuleStatus.Blocking;

        var stillRequiredCategoriesFulfillableInBlock = blockCategories[termin.Block.Id].Intersect(requiredCategories);

        if (terminsRequiredCategoryStillRequired || stillRequiredCategoriesFulfillableInBlock.Any())
            return LastAvailableBlockRuleStatus.ImpossibleButHelping;

        return LastAvailableBlockRuleStatus.ImpossibleButBetterOptionsAvailable;


        bool Backtrack(HashSet<Guid> catsSelected, int blockIndex = 0)
        {
            if (requiredCategories.All(catsSelected.Contains)) return true;
            if (blockIndex >= blockIds.Length) return false;

            foreach (var cat in blockCategories[blockIds[blockIndex]])
            {
                if (!catsSelected.Add(cat)) continue;

                if (Backtrack(catsSelected, blockIndex + 1))
                    return true;
                catsSelected.Remove(cat);
            }

            // If there are no required categories in this block, we can skip it. Otherwise, we can always choose one and the problem gets easier
            return blockCategories[blockIds[blockIndex]].Count == 0 &&
                   Backtrack(catsSelected, blockIndex + 1);
        }
    }

    private enum LastAvailableBlockRuleStatus
    {
        Fulfillable,
        Blocking,
        ImpossibleButHelping,
        ImpossibleButBetterOptionsAvailable
    }
}
