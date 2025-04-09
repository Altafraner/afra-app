using Afra_App.Authentication;
using Afra_App.Data;
using Afra_App.Data.DTO;
using Afra_App.Data.People;
using Microsoft.EntityFrameworkCore;

namespace Afra_App.Endpoints;

/// <summary>
/// A class containing extension methods for the people endpoint.
/// </summary>
public static class PeopleEndpoints
{
    /// <summary>
    /// Maps endpoints for getting people.
    /// </summary>
    public static void MapPeopleEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/people", GetPeople)
            .WithName("GetPeople")
            .RequireAuthorization();
    }

    private static async Task<IResult> GetPeople(AfraAppContext dbContext, HttpContext httpContext)
    {
        var user = await httpContext.GetPersonAsync(dbContext);
        if (user.Rolle != Rolle.Tutor)
            return Results.Unauthorized();

        var people = dbContext.Personen
            .OrderBy(p => p.Nachname)
            .ThenBy(p => p.Vorname)
            .Select(p => new PersonInfoMinimal(p))
            .AsAsyncEnumerable();

        return Results.Ok(people);
    }
}