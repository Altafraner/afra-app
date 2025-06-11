using Afra_App.Backbone.Authentication;
using Afra_App.User.Domain.DTO;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Afra_App.User.API.Endpoints;

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
            .RequireAuthorization(AuthorizationPolicies.TutorOnly);
    }

    private static Ok<IAsyncEnumerable<PersonInfoMinimal>> GetPeople(AfraAppContext dbContext,
        HttpContext httpContext)
    {
        var people = dbContext.Personen
            .OrderBy(p => p.Nachname)
            .ThenBy(p => p.Vorname)
            .Select(p => new PersonInfoMinimal(p))
            .AsAsyncEnumerable();

        return TypedResults.Ok(people);
    }
}