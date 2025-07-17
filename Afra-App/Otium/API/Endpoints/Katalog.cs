using Afra_App.Otium.Services;
using Afra_App.User.Services;
using Microsoft.AspNetCore.Mvc;

namespace Afra_App.Otium.API.Endpoints;

/// <summary>
///     Contains endpoints for managing kategories.
/// </summary>
public static class Katalog
{
    /// <summary>
    ///     Maps the kategorie endpoints to the given <see cref="IEndpointRouteBuilder" />.
    /// </summary>
    public static void MapKatalogEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/{date}", GetDay);
        app.MapGet("/{terminId:guid}", GetTermin);
        app.MapPut("/{terminId:guid}", EnrollAsync);
        app.MapPut("/{terminId:guid}/multi-enroll", MultiEnrollAsync);
        app.MapDelete("/{terminId:guid}", UnenrollAsync);
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
}
