using Afra_App.Authentication;
using Afra_App.Data;
using Afra_App.Data.People;
using Afra_App.Services.Otium;

namespace Afra_App.Endpoints.Otium;

/// <summary>
///     A class containing the endpoints for the management of otia.
/// </summary>
public static class ManagementEndpoints
{
    /// <summary>
    ///     Maps the management endpoints to the given <see cref="IEndpointRouteBuilder" />.
    /// </summary>
    /// <param name="app"></param>
    public static void MapManagementEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/management/termin/{terminId:guid}", GetTerminForTeacher);
    }

    private static async Task<IResult> GetTerminForTeacher(OtiumEndpointService service, HttpContext httpContext,
        AfraAppContext context, Guid terminId)
    {
        var user = await httpContext.GetPersonAsync(context);
        if (user.Rolle != Rolle.Tutor) return Results.Unauthorized();

        return Results.Ok(service.GetTerminForTeacher(terminId, user));
    }
}