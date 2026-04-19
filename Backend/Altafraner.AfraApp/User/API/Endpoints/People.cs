using Altafraner.AfraApp.Attendance.AbsenceProviders.Cevex;
using Altafraner.AfraApp.Attendance.Configuration;
using Altafraner.AfraApp.Backbone.Authorization;
using Altafraner.AfraApp.User.Domain.DTO;
using Altafraner.AfraApp.User.Domain.Models;
using Altafraner.AfraApp.User.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Altafraner.AfraApp.User.API.Endpoints;

/// <summary>
/// A class containing extension methods for the people endpoint.
/// </summary>
internal static class People
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
        var attendanceConfiguration = app.ServiceProvider.GetService<IOptions<AttendanceConfiguration>>();
        if (string.IsNullOrWhiteSpace(attendanceConfiguration?.Value.Cevex?.FilePath)) return;
        app.MapGet("/api/people/cevex", GetCevex)
            .RequireAuthorization(AuthorizationPolicies.AdminOnly);
        app.MapPost("/api/people/cevex", SetCevex)
            .RequireAuthorization(AuthorizationPolicies.AdminOnly);
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

    private static async Task<IResult> GetCevex(AfraAppContext dbContext,
        CevexDataParser cevexParser)
    {
        var cevexData = (await cevexParser.ReadFile()).ToArray();
        await cevexParser.GetMatches();
        var cevexDict = cevexData.ToDictionary(data => data.Guid);
        var people = await dbContext.Personen
            .AsNoTracking()
            .Where(p => p.Rolle == Rolle.Mittelstufe || p.Rolle == Rolle.Oberstufe)
            .OrderByDescending(p => p.CevexId == null)
            .ThenBy(p => p.LastName)
            .ThenBy(p => p.FirstName)
            .AsAsyncEnumerable()
            .Select(person => new CevexMatch
            {
                Cevex = person.CevexId is not null && cevexDict.TryGetValue(person.CevexId, out var cevexUser)
                    ? new CevexEntity(cevexUser)
                    : null,
                User = new PersonInfoMinimal(person)
            })
            .ToArrayAsync();
        var usedCevexIds = people
            .Where(p => p.Cevex is not null)
            .Select(p => p.Cevex!.Value.Id!)
            .ToHashSet();
        var missingCevexIds = cevexData.Select(c => c.Guid).ToHashSet();
        missingCevexIds.ExceptWith(usedCevexIds);

        return Results.Ok(new
        {
            Available = missingCevexIds.Select(c => new CevexEntity(cevexDict[c])),
            Matches = people
        });
    }

    private static async Task<IResult> SetCevex(CevexChangeRequest request,
        CevexDataParser cevexParser,
        UserService userService,
        AfraAppContext dbContext)
    {
        var cevexData = (await cevexParser.ReadFile()).ToArray();
        var cevexDict = cevexData.ToDictionary(data => data.Guid);
        var usedIds = await dbContext.Personen
            .Where(p => (p.Rolle == Rolle.Mittelstufe || p.Rolle == Rolle.Oberstufe) && p.CevexId != null)
            .Select(p => p.CevexId!)
            .ToHashSetAsync();
        if (usedIds.Contains(request.CevexId)) return Results.Conflict();
        if (!cevexDict.ContainsKey(request.CevexId)) return Results.NotFound();
        try
        {
            var user = await userService.GetUserByIdAsync(request.UserId);
            user.CevexId = request.CevexId;
            user.CevexIdManuallyEntered = true;
            dbContext.Update(user);
            await dbContext.SaveChangesAsync();
            return Results.NoContent();
        }
        catch (InvalidOperationException)
        {
            return Results.NotFound();
        }
    }
}
