using Altafraner.AfraApp.Backbone.Authentication;
using Altafraner.AfraApp.User.Domain.DTO;
using Altafraner.AfraApp.User.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Altafraner.AfraApp.User.API.Endpoints;

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
        app.MapGet("/api/klassen", GetKlassen)
            .RequireAuthorization();
    }

    private static Ok<IAsyncEnumerable<PersonInfoMinimal>> GetPeople(AfraAppContext dbContext,
        HttpContext httpContext)
    {
        var people = dbContext.Personen
            .OrderBy(p => p.LastName)
            .ThenBy(p => p.FirstName)
            .Select(p => new PersonInfoMinimal(p))
            .AsAsyncEnumerable();

        return TypedResults.Ok(people);
    }

    private static async Task<IResult> GetPersonMentors(AfraAppContext dbContext, UserService userService, Guid id)
    {
        try
        {
            var student = await userService.GetUserByIdAsync(id);
            var mentors = await userService.GetMentorsAsync(student);
            return Results.Ok(mentors.Select(s => new PersonInfoMinimal(s)));
        }
        catch (KeyNotFoundException)
        {
            return Results.NotFound();
        }
        catch (InvalidOperationException)
        {
            return Results.Ok(new List<PersonInfoMinimal>());
        }
    }

    private static IResult GetKlassen(AfraAppContext dbContext, UserService userService)
    {
        return Results.Ok(userService.GetKlassenstufen());
    }
}