using Afra_App.Authentication;
using Afra_App.Data;
using Afra_App.Services.Otium;

namespace Afra_App.Endpoints.Otium;

/// <summary>
///     Contains endpoints for managing kategories.
/// </summary>
public static class KatalogEndpoints
{
    /// <summary>
    ///     Maps the kategorie endpoints to the given <see cref="IEndpointRouteBuilder" />.
    /// </summary>
    public static void MapKatalogEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/{date}/{block:int}", GetDay);
        app.MapGet("/{terminId:guid}", GetTermin);
        app.MapPut("/{terminId:guid}/{start}", EnrollAsync);
        app.MapDelete("/{terminId:guid}/{start}", UnenrollAsync);
    }

    private static IResult GetDay(OtiumEndpointService service, DateOnly date, byte block)
    {
        return Results.Ok(service.GetKatalogForDayAndBlock(date, block));
    }

    private static async Task<IResult> GetTermin(OtiumEndpointService service, Guid terminId, HttpContext httpContext,
        AfraAppContext context)
    {
        var user = await httpContext.GetPersonAsync(context);

        var termin = await service.GetTerminAsync(terminId, user);
        if (termin == null) return Results.NotFound();

        return Results.Ok(termin);
    }

    private static async Task<IResult> EnrollAsync(OtiumEndpointService service, EnrollmentService enrollmentService,
        HttpContext httpContext, AfraAppContext context, Guid terminId, TimeOnly start)
    {
        var user = await httpContext.GetPersonAsync(context);

        var termin = await enrollmentService.EnrollAsync(terminId, user, start);
        return termin is null ? Results.BadRequest() : Results.Ok(await service.GetTerminAsync(terminId, user));
    }

    private static async Task<IResult> UnenrollAsync(OtiumEndpointService service, EnrollmentService enrollmentService,
        HttpContext httpContext, AfraAppContext context, Guid terminId, TimeOnly start)
    {
        var user = await httpContext.GetPersonAsync(context);

        var termin = await enrollmentService.UnenrollAsync(terminId, user, start);
        return termin is null ? Results.BadRequest() : Results.Ok(await service.GetTerminAsync(terminId, user));
    }
}