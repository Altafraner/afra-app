using Afra_App.Data;
using Afra_App.Services.Otium;
using Afra_App.Services.User;

namespace Afra_App.Endpoints.Otium;

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
        app.MapDelete("/{terminId:guid}", UnenrollAsync);
    }

    private static async Task<IResult> GetDay(OtiumEndpointService service, UserAccessor userAccessor,
        AfraAppContext dbContext, DateOnly date)
    {
        var user = await userAccessor.GetUserAsync();

        return Results.Ok(await service.GetKatalogForDay(user, date));
    }

    private static async Task<IResult> GetTermin(OtiumEndpointService service, Guid terminId, UserAccessor userAccessor,
        AfraAppContext dbContext)
    {
        var user = await userAccessor.GetUserAsync();

        var termin = await service.GetTerminAsync(terminId, user);
        if (termin == null) return Results.NotFound();

        return Results.Ok(termin);
    }

    private static async Task<IResult> EnrollAsync(OtiumEndpointService service, EnrollmentService enrollmentService,
        UserAccessor userAccessor, AfraAppContext dbContext, Guid terminId)
    {
        var user = await userAccessor.GetUserAsync();

        var termin = await enrollmentService.EnrollAsync(terminId, user);
        return termin is null ? Results.BadRequest() : Results.Ok(await service.GetTerminAsync(terminId, user));
    }

    private static async Task<IResult> UnenrollAsync(OtiumEndpointService service, EnrollmentService enrollmentService,
        UserAccessor userAccessor, AfraAppContext dbContext, Guid terminId)
    {
        var user = await userAccessor.GetUserAsync();

        var termin = await enrollmentService.UnenrollAsync(terminId, user);
        return termin is null ? Results.BadRequest() : Results.Ok(await service.GetTerminAsync(terminId, user));
    }
}
