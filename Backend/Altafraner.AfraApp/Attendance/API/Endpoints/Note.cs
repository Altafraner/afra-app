using Altafraner.AfraApp.Attendance.Domain.Dto.Notiz;
using Altafraner.AfraApp.Attendance.Domain.Models;
using Altafraner.AfraApp.Attendance.Services;
using Altafraner.AfraApp.User.Domain.Models;
using Altafraner.AfraApp.User.Services;

namespace Altafraner.AfraApp.Attendance.API.Endpoints;

internal static class Note
{
    internal static void MapNoteEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/notes", AddNote);
        app.MapPut("/notes", UpdateNote);
    }

    private static async Task<IResult> AddNote(NotesService service,
        UserAccessor userAccessor,
        UserService userService,
        NotizCreationRequest request)
    {
        var user = await userAccessor.GetUserAsync();

        if (user.Id != request.StudentId && user.Rolle != Rolle.Tutor) return Results.Forbid();

        var affected = await userService.GetUserByIdAsync(request.StudentId);
        if (affected.Rolle is not Rolle.Mittelstufe and not Rolle.Oberstufe) return Results.BadRequest();

        var success = await service.TryAddNoteAsync(request.Scope,
            request.SlotId,
            request.Content,
            request.StudentId,
            user.Id);
        return success ? Results.Created() : Results.Conflict();
    }

    private static async Task<IResult> UpdateNote(NotesService service,
        UserAccessor userAccessor,
        UserService userService,
        NotizCreationRequest request)
    {
        var user = await userAccessor.GetUserAsync();

        if (user.Id != request.StudentId && user.Rolle != Rolle.Tutor) return Results.Forbid();

        var affected = await userService.GetUserByIdAsync(request.StudentId);
        if (affected.Rolle is not Rolle.Mittelstufe and not Rolle.Oberstufe)
            return Results.BadRequest("The person represented by studentId is not a student.");

        List<AttendanceNote> notes;
        var success = await service.UpdateNoteAsync(request.Scope,
            request.SlotId,
            request.Content,
            request.StudentId,
            user.Id);
        if (success)
        {
            notes = await service.GetNotesAsync(request.Scope, request.SlotId, affected.Id);
            return Results.Ok(notes.Select(n => new Notiz(n)));
        }

        success = await service.TryAddNoteAsync(request.Scope,
            request.SlotId,
            request.Content,
            request.StudentId,
            user.Id);
        if (!success) return Results.Conflict();

        notes = await service.GetNotesAsync(request.Scope, request.SlotId, affected.Id);
        return Results.Ok(notes.Select(n => new Notiz(n)));
    }
}
