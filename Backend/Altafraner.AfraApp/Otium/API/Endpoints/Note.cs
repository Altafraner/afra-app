using Altafraner.AfraApp.Otium.Domain.DTO.Notiz;
using Altafraner.AfraApp.Otium.Services;
using Altafraner.AfraApp.User.Domain.Models;
using Altafraner.AfraApp.User.Services;

namespace Altafraner.AfraApp.Otium.API.Endpoints;

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

        var success = await service.TryAddNoteAsync(request.Content, request.StudentId, request.BlockId, user.Id);
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
        if (affected.Rolle is not Rolle.Mittelstufe and not Rolle.Oberstufe) return Results.BadRequest();

        var success = await service.UpdateNoteAsync(request.Content, request.StudentId, request.BlockId, user.Id);
        if (success) return Results.Ok();

        success = await service.TryAddNoteAsync(request.Content, request.StudentId, request.BlockId, user.Id);
        return success ? Results.Created() : Results.Conflict();
    }
}
