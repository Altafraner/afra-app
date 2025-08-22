using Afra_App.Backbone.Authentication;
using Afra_App.User.Domain.DTO;
using Afra_App.User.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Afra_App.User.API.Endpoints;

/// <summary>
/// A class containing extension methods for the people endpoint.
/// </summary>
public static class People
{
    /// <summary>
    /// Maps endpoints for getting people.
    /// </summary>
    public static void MapPeopleEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/people", GetPeople)
            .WithName("GetPeople")
            .RequireAuthorization(AuthorizationPolicies.TeacherOrAdmin);
        app.MapGet("/api/people/{id:guid}/mentor", GetPersonMentors)
            .WithName("GetPersonMentors")
            .RequireAuthorization(AuthorizationPolicies.TeacherOrAdmin);
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

    private static async Task<IResult> GetPersonMentors(AfraAppContext dbContext, UserService userService, Guid id)
    {
        var student = (await dbContext.Personen
            .Include(p => p.Mentors)
            .Where(p => p.Id == id)
            .ToArrayAsync())
            .FirstOrDefault(defaultValue: null);

        if (student is null)
        {
            return Results.NotFound();
        }

        var mentors = student.Mentors
            .Select(m => new PersonInfoMinimal(m));
        return Results.Ok(mentors);
    }
}
