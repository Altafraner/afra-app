using Altafraner.AfraApp.Otium.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Altafraner.AfraApp.Otium.Services;

internal sealed class NotesService
{
    private readonly AfraAppContext _dbContext;
    private readonly AttendanceRealtimeService _attendanceRealtimeService;

    public NotesService(AfraAppContext dbContext, AttendanceRealtimeService attendanceRealtimeService)
    {
        _dbContext = dbContext;
        _attendanceRealtimeService = attendanceRealtimeService;
    }

    public async Task<bool> TryAddNoteAsync(string content, Guid studentId, Guid blockId, Guid authorId)
    {
        if (await HasNoteAsync(studentId, blockId, authorId)) return false;

        await _dbContext.OtiaEinschreibungsNotizen.AddAsync(new OtiumAnwesenheitsNotiz
        {
            Content = content,
            AuthorId = authorId,
            StudentId = studentId,
            BlockId = blockId
        });
        await _dbContext.SaveChangesAsync();
        await SendRealtimeUpdate(studentId, blockId);
        return true;
    }

    public async Task<bool> UpdateNoteAsync(string content, Guid studentId, Guid blockId, Guid authorId)
    {
        var note = await _dbContext.OtiaEinschreibungsNotizen.FirstOrDefaultAsync(e => e.StudentId == studentId
            && e.BlockId == blockId
            && e.AuthorId == authorId);

        if (note == null) return false;
        if (string.IsNullOrWhiteSpace(content))
        {
            _dbContext.OtiaEinschreibungsNotizen.Remove(note);
            await _dbContext.SaveChangesAsync();
            await SendRealtimeUpdate(studentId, blockId);
            return true;
        }

        note.Content = content;
        await _dbContext.SaveChangesAsync();
        await SendRealtimeUpdate(studentId, blockId);
        return true;
    }

    public async Task<bool> RemoveNoteAsync(Guid studentId, Guid blockId, Guid authorId)
    {
        if (!await HasNoteAsync(studentId, blockId, authorId)) return false;

        _dbContext.Remove(new OtiumAnwesenheitsNotiz
        {
            Content = null!,
            BlockId = blockId,
            StudentId = studentId,
            AuthorId = authorId
        });
        await _dbContext.SaveChangesAsync();
        await SendRealtimeUpdate(studentId, blockId);
        return true;
    }

    public async Task<bool> HasNoteAsync(Guid studentId, Guid blockId, Guid authorId)
    {
        return await _dbContext.OtiaEinschreibungsNotizen.AnyAsync(e => e.AuthorId == authorId
                                                                        && e.StudentId == studentId
                                                                        && e.BlockId == blockId);
    }

    public async Task<bool> HasNoteAsync(Guid studentId, Guid blockId)
    {
        return await _dbContext.OtiaEinschreibungsNotizen.AnyAsync(e => e.StudentId == studentId
                                                                        && e.BlockId == blockId);
    }

    public async Task<List<OtiumAnwesenheitsNotiz>> GetNotesAsync(Guid studentId, Guid blockId)
    {
        return await _dbContext.OtiaEinschreibungsNotizen
            .Include(e => e.Author)
            .Where(e => e.StudentId == studentId)
            .Where(e => e.BlockId == blockId)
            .OrderByDescending(n => n.LastModified)
            .ToListAsync();
    }

    public async Task<Dictionary<Guid, OtiumAnwesenheitsNotiz[]>> GetNotesByBlockAsync(Guid blockId)
    {
        return await _dbContext.OtiaEinschreibungsNotizen
            .Include(e => e.Author)
            .Where(e => e.BlockId == blockId)
            .OrderByDescending(n => n.LastModified)
            .GroupBy(e => e.StudentId)
            .ToDictionaryAsync(e => e.Key, e => e.ToArray());
    }

    private async Task SendRealtimeUpdate(Guid studentId, Guid blockId)
    {
        var notes = await GetNotesAsync(studentId, blockId);
        await _attendanceRealtimeService.SendNoteUpdate(studentId, blockId, notes);
    }
}
