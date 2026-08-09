using Altafraner.AfraApp.User.Domain.Contracts;
using Altafraner.AfraApp.User.Domain.Models;

namespace Altafraner.AfraApp.Attendance.Services;

internal class AttendanceUserEventHandler : IUserEventHandler
{
    private readonly AfraAppContext _dbContext;

    public AttendanceUserEventHandler(AfraAppContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task OnUserSoftDeletedAsync(Person user)
    {
        var notes = _dbContext.AttendanceNotes.Where(e => e.Author == user || e.Student == user);
        _dbContext.AttendanceNotes.RemoveRange(notes);
        return Task.CompletedTask;
    }
}
