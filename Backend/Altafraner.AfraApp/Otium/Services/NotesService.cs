using Altafraner.AfraApp.Otium.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Altafraner.AfraApp.Otium.Services;

internal sealed class NotesService
{
    private readonly AfraAppContext _dbContext;

    public NotesService(AfraAppContext dbContext)
    {
        _dbContext = dbContext;
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
        return true;
    }

    public async Task<bool> UpdateNoteAsync(string content, Guid studentId, Guid blockId, Guid authorId)
    {
        var note = await _dbContext.OtiaEinschreibungsNotizen.FirstOrDefaultAsync(e => e.StudentId == studentId
            && e.BlockId == blockId
            && e.AuthorId == authorId);

        if (note == null) return false;
        note.Content = content;
        await _dbContext.SaveChangesAsync();

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
}
