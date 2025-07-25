﻿using Afra_App.Otium.Domain.Models;
using Afra_App.User.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Afra_App.Otium.Services;

/// <summary>
/// A service for managing attendance in the Otium module of the Afra App.
/// </summary>
public class AttendanceService : IAttendanceService
{
    private const AnwesenheitsStatus DefaultAttendanceStatus = AnwesenheitsStatus.Fehlend;
    private readonly BlockHelper _blockHelper;
    private readonly AfraAppContext _dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="AttendanceService"/> class.
    /// </summary>
    public AttendanceService(AfraAppContext dbContext, BlockHelper blockHelper)
    {
        _dbContext = dbContext;
        _blockHelper = blockHelper;
    }

    /// <inheritdoc />
    public async Task<AnwesenheitsStatus> GetAttendanceForEnrollmentAsync(Guid enrollmentId)
    {
        var enrollment = await _dbContext.OtiaEinschreibungen
            .Include(e => e.Termin)
            .ThenInclude(t => t.Block)
            .Include(t => t.BetroffenePerson)
            .Where(e => e.Id == enrollmentId)
            .Select(e => new
            {
                PersonId = e.BetroffenePerson.Id,
                BlockId = e.Termin.Block.Id
            })
            .FirstOrDefaultAsync();

        if (enrollment == null)
            throw new KeyNotFoundException($"Enrollment ID {enrollmentId} not found");

        // If we were to Select (a => a.Status), we could not check for null, as it would return the default value of AnwesenheitsStatus and not null;
        var attendance = await _dbContext.OtiaAnwesenheiten
            .Where(a => a.StudentId == enrollment.PersonId &&
                        a.BlockId == enrollment.BlockId)
            .Select(a => new { a.Status })
            .FirstOrDefaultAsync();

        return attendance?.Status ?? DefaultAttendanceStatus;
    }

    /// <inheritdoc />
    public async Task<Dictionary<Person, AnwesenheitsStatus>> GetAttendanceForTerminAsync(Guid terminId)
    {
        var blockIdWrapper = await _dbContext.OtiaTermine
            .AsNoTracking()
            .Where(t => t.Id == terminId)
            .Select(t => new { t.Block.Id })
            .FirstOrDefaultAsync();
        if (blockIdWrapper is null)
            throw new KeyNotFoundException($"Termin ID {terminId} not found");

        var blockId = blockIdWrapper.Id;

        var persons = await _dbContext.OtiaEinschreibungen
            .AsNoTracking()
            .Include(e => e.BetroffenePerson)
            .Where(e => e.Termin.Id == terminId)
            .Select(e => e.BetroffenePerson)
            .OrderBy(e => e.Vorname)
            .ThenBy(e => e.Nachname)
            .ToListAsync();

        var attendances = await _dbContext.OtiaAnwesenheiten
            .AsNoTracking()
            .Where(a => persons.Select(e => e.Id).Contains(a.StudentId) &&
                        a.BlockId == blockId)
            .ToDictionaryAsync(a => a.StudentId, a => new { a.Status });

        return persons.ToDictionary(p => p,
            p => attendances.TryGetValue(p.Id, out var status) ? status.Status : DefaultAttendanceStatus);
    }

    /// <inheritdoc />
    public async Task<(Dictionary<Termin, Dictionary<Person, AnwesenheitsStatus>> termine,
            Dictionary<Person, AnwesenheitsStatus> missingPersons, bool missingPersonsChecked)>
        GetAttendanceForBlockAsync(Guid blockId)
    {
        var block = await _dbContext.Blocks
            .AsNoTracking()
            .Where(b => b.Id == blockId)
            .Select(b => new
                { b.SchemaId, SindAnwesenheitenFehlernderErfasst = b.SindAnwesenheitenFehlernderKontrolliert })
            .FirstOrDefaultAsync();

        if (block is null)
            throw new KeyNotFoundException($"Block ID {blockId} not found");

        var termine = await _dbContext.OtiaTermine
            .AsNoTracking()
            .Where(t => t.Block.Id == blockId)
            .Include(t => t.Otium)
            .ToListAsync();

        var terminAttendance = new Dictionary<Termin, Dictionary<Person, AnwesenheitsStatus>>();
        foreach (var termin in termine)
        {
            var attendance = await GetAttendanceForTerminAsync(termin.Id);
            terminAttendance[termin] = attendance;
        }

        // If the block is not mandatory, we return the attendance without checking for missing students
        var blockSchema = _blockHelper.Get(block.SchemaId);
        if (!blockSchema!.Verpflichtend)
        {
            return (terminAttendance, new Dictionary<Person, AnwesenheitsStatus>(), true);
        }

        var personIds = terminAttendance.Values
            .SelectMany(a => a.Keys)
            .Select(p => p.Id)
            .Distinct()
            .ToList();

        var missingPersons = await _dbContext.Personen
            .Where(p => p.Rolle == Rolle.Mittelstufe)
            .Where(p => !personIds.Contains(p.Id))
            .ToListAsync();

        var missingPersonIds = missingPersons.Select(p => p.Id).ToList();

        var missingPersonsAttendance = await _dbContext.OtiaAnwesenheiten
            .AsNoTracking()
            .Where(a => a.BlockId == blockId)
            .Where(a => missingPersonIds.Contains(a.StudentId))
            .ToDictionaryAsync(a => a.StudentId, a => new { a.Status });

        var missingPersonsAttendanceDict = missingPersons.ToDictionary(
            p => p,
            p => missingPersonsAttendance.TryGetValue(p.Id, out var status) ? status.Status : DefaultAttendanceStatus);

        return (terminAttendance, missingPersonsAttendanceDict, block.SindAnwesenheitenFehlernderErfasst);
    }

    /// <inheritdoc />
    public async Task SetAttendanceForEnrollmentAsync(Guid enrollmentId, AnwesenheitsStatus status)
    {
        var einschreibung = _dbContext.OtiaEinschreibungen
            .AsNoTracking()
            .Where(e => e.Id == enrollmentId)
            .Select(e => new { BlockId = e.Termin.Block.Id, StudentId = e.BetroffenePerson.Id })
            .FirstOrDefault();

        if (einschreibung is null)
            throw new KeyNotFoundException($"Enrollment ID {enrollmentId} not found");

        await CreateOrUpdate(einschreibung.StudentId, einschreibung.BlockId, status);
        await _dbContext.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task SetAttendanceForStudentInBlockAsync(Guid studentId, Guid blockId, AnwesenheitsStatus status)
    {
        var blockExists = await _dbContext.Blocks
            .AsNoTracking()
            .AnyAsync(b => b.Id == blockId);
        if (!blockExists)
            throw new KeyNotFoundException($"Block ID {blockId} not found");

        var studentExists = await _dbContext.Personen
            .AsNoTracking()
            .AnyAsync(p => p.Id == studentId && p.Rolle == Rolle.Mittelstufe);
        if (!studentExists)
            throw new KeyNotFoundException($"Student ID {studentId} not found");

        await CreateOrUpdate(studentId, blockId, status);
        await _dbContext.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task SetStatusForTerminAsync(Guid terminId, bool status)
    {
        var termin = await _dbContext.OtiaTermine
            .FirstOrDefaultAsync(t => t.Id == terminId);
        if (termin is null)
            throw new KeyNotFoundException($"Termin ID {terminId} not found");

        termin.SindAnwesenheitenKontrolliert = status;
        await _dbContext.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task SetStatusForMissingPersonsAsync(Guid blockId, bool status)
    {
        var block = await _dbContext.Blocks
            .FirstOrDefaultAsync(b => b.Id == blockId);
        if (block is null)
            throw new KeyNotFoundException($"Block ID {blockId} not found");

        block.SindAnwesenheitenFehlernderKontrolliert = status;
        await _dbContext.SaveChangesAsync();
    }

    private async Task CreateOrUpdate(Guid studentId, Guid blockId, AnwesenheitsStatus status)
    {
        var attendance = await _dbContext.OtiaAnwesenheiten
            .FirstOrDefaultAsync(a => a.StudentId == studentId && a.BlockId == blockId);

        if (attendance is not null)
            attendance.Status = status;
        else
        {
            await _dbContext.OtiaAnwesenheiten.AddAsync(new Anwesenheit
            {
                BlockId = blockId,
                StudentId = studentId,
                Status = status
            });
        }
    }
}