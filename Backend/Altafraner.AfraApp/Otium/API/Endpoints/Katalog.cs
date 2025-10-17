using Altafraner.AfraApp.Backbone.Authorization;
using Altafraner.AfraApp.Otium.Services;
using Altafraner.AfraApp.User.Services;
using Microsoft.AspNetCore.Mvc;

namespace Altafraner.AfraApp.Otium.API.Endpoints;

/// <summary>
///     Contains endpoints for managing kategories.
/// </summary>
public static class Katalog
{
    /// <summary>
    ///     Maps the kategorie endpoints to the given <see cref="IEndpointRouteBuilder" />.
    /// </summary>
    public static void MapKatalogEndpoints(this IEndpointRouteBuilder routeBuilder)
    {
        routeBuilder.MapGet("/{date}", GetDay);
        routeBuilder.MapGet("/{terminId:guid}", GetTermin);
        routeBuilder.MapPut("/{terminId:guid}", EnrollAsync)
            .RequireAuthorization(AuthorizationPolicies.StudentOnly);
        routeBuilder.MapPost("/{terminId:guid}/note", AddNoteAsync)
            .RequireAuthorization(AuthorizationPolicies.StudentOnly);
        routeBuilder.MapPut("/{terminId:guid}/note", UpdateNoteAsync)
            .RequireAuthorization(AuthorizationPolicies.StudentOnly);
        routeBuilder.MapDelete("/{terminId:guid}/note", RemoveNoteAsync)
            .RequireAuthorization(AuthorizationPolicies.StudentOnly);
        routeBuilder.MapPut("/{terminId:guid}/multi-enroll", MultiEnrollAsync)
            .RequireAuthorization(AuthorizationPolicies.StudentOnly);
        routeBuilder.MapDelete("/{terminId:guid}", UnenrollAsync)
            .RequireAuthorization(AuthorizationPolicies.StudentOnly);
    }

    private static async Task<IResult> GetDay(OtiumEndpointService service, UserAccessor userAccessor, DateOnly date)
    {
        var user = await userAccessor.GetUserAsync();

        return Results.Ok(await service.GetKatalogForDay(user, date));
    }

    private static async Task<IResult> GetTermin(OtiumEndpointService service, Guid terminId, UserAccessor userAccessor)
    {
        var user = await userAccessor.GetUserAsync();

        var termin = await service.GetTerminAsync(terminId, user);
        if (termin == null) return Results.NotFound();

        return Results.Ok(termin);
    }

    private static async Task<IResult> EnrollAsync(OtiumEndpointService service, EnrollmentService enrollmentService,
        UserAccessor userAccessor, Guid terminId)
    {
        var user = await userAccessor.GetUserAsync();

        var termin = await enrollmentService.EnrollAsync(terminId, user);
        return termin is null ? Results.BadRequest() : Results.Ok(await service.GetTerminAsync(terminId, user));
    }

    private static async Task<IResult> MultiEnrollAsync(OtiumEndpointService service,
        EnrollmentService enrollmentService,
        UserAccessor userAccessor, Guid terminId, [FromBody] IEnumerable<DateOnly> dates)
    {
        var user = await userAccessor.GetUserAsync();

        try
        {
            var result = await enrollmentService.EnrollAsync(terminId, dates, user);
            return Results.Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return Results.NotFound();
        }
        catch (InvalidOperationException)
        {
            return Results.BadRequest("You may not enroll in this termin.");
        }
    }

    private static async Task<IResult> UnenrollAsync(OtiumEndpointService service, EnrollmentService enrollmentService,
        UserAccessor userAccessor, Guid terminId)
    {
        var user = await userAccessor.GetUserAsync();

        var termin = await enrollmentService.UnenrollAsync(terminId, user);
        return termin is null ? Results.BadRequest() : Results.Ok(await service.GetTerminAsync(terminId, user));
    }

    private static async Task<IResult> AddNoteAsync(
        ManagementService managementService,
        NotesService notesService,
        UserAccessor userAccessor,
        Guid terminId,
        ValueWrapper<string> content)
    {
        var blockId = await managementService.GetBlockIdOfTerminIdAsync(terminId);
        var userId = userAccessor.GetUserId();
        var success = await notesService.TryAddNoteAsync(content.Value, blockId, userId, userId);
        return success ? Results.Ok() : Results.Conflict();
    }

    private static async Task<IResult> UpdateNoteAsync(
        ManagementService managementService,
        NotesService notesService,
        UserAccessor userAccessor,
        Guid terminId,
        ValueWrapper<string> content)
    {
        var blockId = await managementService.GetBlockIdOfTerminIdAsync(terminId);
        var userId = userAccessor.GetUserId();
        var success = await notesService.UpdateNoteAsync(content.Value, blockId, userId, userId);
        return success ? Results.Ok() : Results.NotFound();
    }

    private static async Task<IResult> RemoveNoteAsync(
        ManagementService managementService,
        NotesService notesService,
        UserAccessor userAccessor,
        Guid terminId)
    {
        var blockId = await managementService.GetBlockIdOfTerminIdAsync(terminId);
        var userId = userAccessor.GetUserId();
        var success = await notesService.RemoveNoteAsync(blockId, userId, userId);
        return success ? Results.Ok() : Results.NotFound();
    }
}
