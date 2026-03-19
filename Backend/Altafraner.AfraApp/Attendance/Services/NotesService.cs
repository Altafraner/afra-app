using Altafraner.AfraApp.Attendance.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Altafraner.AfraApp.Attendance.Services;

internal sealed class NotesService
{
    private readonly AfraAppContext _dbContext;
    private readonly SimpleAttendanceNotificationService _simpleAttendanceNotificationService;

    public NotesService(AfraAppContext dbContext,
        SimpleAttendanceNotificationService simpleAttendanceNotificationService)
    {
        _dbContext = dbContext;
        _simpleAttendanceNotificationService = simpleAttendanceNotificationService;
    }

    public async Task<bool> TryAddNoteAsync(AttendanceScope scope,
        Guid slotId,
        string content,
        Guid studentId,
        Guid authorId)
    {
        if (await HasNoteAsync(scope, slotId, studentId, authorId)) return false;

        await _dbContext.AttendanceNotes.AddAsync(new AttendanceNote
        {
            Scope = scope,
            SlotId = slotId,
            Content = content,
            AuthorId = authorId,
            StudentId = studentId,
        });
        await _dbContext.SaveChangesAsync();
        await SendRealtimeUpdate(scope, slotId, studentId);
        return true;
    }

    public async Task<bool> UpdateNoteAsync(AttendanceScope scope,
        Guid slotId,
        string content,
        Guid studentId,
        Guid authorId)
    {
        var note = await _dbContext.AttendanceNotes.FirstOrDefaultAsync(e => e.StudentId == studentId
                                                                             && e.Scope == scope
                                                                             && e.SlotId == slotId
                                                                             && e.AuthorId == authorId);

        if (note == null) return false;
        if (string.IsNullOrWhiteSpace(content))
        {
            _dbContext.AttendanceNotes.Remove(note);
            await _dbContext.SaveChangesAsync();
            await SendRealtimeUpdate(scope, slotId, studentId);
            return true;
        }

        note.Content = content;
        await _dbContext.SaveChangesAsync();
        await SendRealtimeUpdate(scope, slotId, studentId);
        return true;
    }

    public async Task<bool> RemoveNoteAsync(AttendanceScope scope, Guid slotId, Guid studentId, Guid authorId)
    {
        if (!await HasNoteAsync(scope, slotId, studentId, authorId)) return false;

        _dbContext.Remove(new AttendanceNote
        {
            Content = null!,
            Scope = scope,
            SlotId = slotId,
            StudentId = studentId,
            AuthorId = authorId
        });
        await _dbContext.SaveChangesAsync();
        await SendRealtimeUpdate(scope, slotId, studentId);
        return true;
    }

    public async Task<bool> HasNoteAsync(AttendanceScope scope, Guid slotId, Guid studentId, Guid authorId)
    {
        return await _dbContext.AttendanceNotes.AnyAsync(e => e.AuthorId == authorId
                                                              && e.StudentId == studentId
                                                              && e.Scope == scope
                                                              && e.SlotId == slotId);
    }

    public async Task<bool> HasNoteAsync(AttendanceScope scope, Guid slotId, Guid studentId)
    {
        return await _dbContext.AttendanceNotes.AnyAsync(e => e.StudentId == studentId
                                                              && e.SlotId == slotId
                                                              && e.Scope == scope);
    }

    public async Task<List<AttendanceNote>> GetNotesAsync(AttendanceScope scope, Guid slotId, Guid studentId)
    {
        return await _dbContext.AttendanceNotes
            .Include(e => e.Author)
            .Where(e => e.StudentId == studentId && e.SlotId == slotId && e.Scope == scope)
            .OrderByDescending(n => n.LastModified)
            .ToListAsync();
    }

    public async Task<Dictionary<Guid, AttendanceNote[]>> GetNotesBySlotAsync(AttendanceScope scope, Guid slotId)
    {
        return await _dbContext.AttendanceNotes
            .Include(e => e.Author)
            .Where(e => e.SlotId == slotId && e.Scope == scope)
            .OrderByDescending(n => n.LastModified)
            .GroupBy(e => e.StudentId)
            .ToDictionaryAsync(e => e.Key, e => e.ToArray());
    }

    private async Task SendRealtimeUpdate(AttendanceScope scope, Guid slotId, Guid studentId)
    {
        var notes = await GetNotesAsync(scope, slotId, studentId);
        await _simpleAttendanceNotificationService.UpdateNotesForSingleStudent(scope,
            slotId,
            studentId,
            notes);
    }
}
