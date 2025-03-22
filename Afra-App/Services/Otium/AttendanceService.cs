using Afra_App.Data;
using Afra_App.Data.Otium;
using Afra_App.Data.People;
using Afra_App.Data.Schuljahr;
using Microsoft.EntityFrameworkCore;

namespace Afra_App.Services.Otium;

/// <summary>
///     A service handling supervisions
/// </summary>
public class AttendanceService
{
    private readonly AfraAppContext _context;

    /// <summary>
    ///     Called by DI
    /// </summary>
    public AttendanceService(AfraAppContext context)
    {
        _context = context;
    }

    /// <summary>
    ///     Generates attendance for all students in the given block
    /// </summary>
    /// <param name="block"></param>
    public async Task InitializeAttendanceAsync(Block block)
    {
        var attendanceNeeded = from p in _context.Personen
            join a in _context.OtiaAnwesenheiten.Where(a => a.Block == block) on p equals a.Student into pa
            where p.Rolle == Rolle.Student && (pa is null || !pa.Any())
            select new Anwesenheit
            {
                Block = block,
                Student = p
            };

        await _context.OtiaAnwesenheiten.AddRangeAsync(attendanceNeeded);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    ///     Sets the attendance status for a student in a block
    /// </summary>
    public async Task SetAttendance(Block block, Person student, AnwesenheitsStatus status)
    {
        var attendance = await _context.OtiaAnwesenheiten.FirstAsync(
            a => a.Block == block && a.Student == student);
        attendance.Status = status;
        await _context.SaveChangesAsync();
    }

    /// <summary>
    ///     Sets the attendance status by id
    /// </summary>
    public async Task SetAttendance(Guid attendanceId, AnwesenheitsStatus status)
    {
        var attendance = await _context.OtiaAnwesenheiten.FindAsync(attendanceId);
        attendance!.Status = status;
        await _context.SaveChangesAsync();
    }

    /// <summary>
    ///     Gets the students that are missing in the given block
    /// </summary>
    public IAsyncEnumerable<Person> GetMissingStudentsAsyncEnumerable(Block block)
    {
        return _context.OtiaAnwesenheiten
            .Where(a => a.Block == block && a.Status == AnwesenheitsStatus.Fehlend)
            .Select(a => a.Student)
            .AsAsyncEnumerable();
    }

    /// <summary>
    ///     Gets all attendances for a block
    /// </summary>
    /// <param name="block">The block to get attendance for</param>
    /// <param name="check">Iff true, checks if the attendance has already been initialiced for the block</param>
    public async Task<IAsyncEnumerable<Anwesenheit>> GetAttendanceForBlockAsyncEnumerable(Block block, bool check)
    {
        if (!check)
            return GetAttendanceForBlockAsyncEnumerable(block);

        var studentCount = _context.Personen.Count(p => p.Rolle == Rolle.Student);
        var attendanceCount = _context.OtiaAnwesenheiten.Count(a => a.Block == block);

        if (studentCount > attendanceCount)
            await InitializeAttendanceAsync(block);

        return GetAttendanceForBlockAsyncEnumerable(block);
    }

    /// <summary>
    ///     Gets all attendances for a block
    /// </summary>
    /// <param name="block">The Block to get attendance for</param>
    public IAsyncEnumerable<Anwesenheit> GetAttendanceForBlockAsyncEnumerable(Block block)
    {
        return GetAttendanceForBlockQueryGenerator(block)
            .AsAsyncEnumerable();
    }

    /// <summary>
    ///     Gets all attendances and enrollments for a block
    /// </summary>
    /// <param name="block"></param>
    /// <returns></returns>
    public IAsyncEnumerable<(Anwesenheit, Einschreibung?)>
        GetAttendanceForBlockWithEnrollmentAsyncEnumerable(Block block)
    {
        var query = from a in GetAttendanceForBlockQueryGenerator(block)
            join e in _context.OtiaEinschreibungen.Where(oe => oe.Termin.Block == block)
                on a.Student equals e.BetroffenePerson into g
            select (a, g.FirstOrDefault());

        return query.AsAsyncEnumerable();
    }

    private IQueryable<Anwesenheit> GetAttendanceForBlockQueryGenerator(Block block)
    {
        return from p in _context.Personen
            join a in _context.OtiaAnwesenheiten.Where(e => e.Block == block) on p.Id equals a.Student.Id into grouping
            from a in grouping.DefaultIfEmpty()
            select a;
    }
}