using Altafraner.AfraApp.Backbone.Authorization;
using Altafraner.AfraApp.Freistellung.Domain.DTO;
using Altafraner.AfraApp.Freistellung.Services;
using Altafraner.AfraApp.User.Domain.DTO;
using Altafraner.AfraApp.User.Domain.Models;
using Altafraner.AfraApp.User.Services;
using Microsoft.EntityFrameworkCore;

namespace Altafraner.AfraApp.Freistellung.API.Endpoints;

/// <summary>
///     Contains endpoints for managing leave requests (Freistellungsanträge).
/// </summary>
public static class FreistellungsEndpoints
{
    /// <summary>
    ///     Maps the Freistellung endpoints to the given <see cref="IEndpointRouteBuilder" />.
    /// </summary>
    public static void MapFreistellungsEndpoints(this IEndpointRouteBuilder app)
    {
        // Common endpoint for students and teachers to get teacher list
        app.MapGet("/lehrer-liste", GetLehrerListe)
            .RequireAuthorization(AuthorizationPolicies.StudentOnly);

        // Student endpoints
        var student = app.MapGroup("/sus")
            .RequireAuthorization(AuthorizationPolicies.StudentOnly);
        student.MapPost("/", CreateAntrag);
        student.MapGet("/", GetAntraegeForStudent);

        // Teacher and mentor endpoints (both are just approvers)
        var lehrer = app.MapGroup("/lehrer")
            .RequireAuthorization(AuthorizationPolicies.TutorOnly);
        lehrer.MapGet("/", GetAntraegeForLehrer);
        lehrer.MapPut("/{antragId:guid}/entscheidung", RecordEntscheidung);

        // Sekretariat endpoints
        var sekretariat = app.MapGroup("/sekretariat")
            .RequireAuthorization(AuthorizationPolicies.Sekretariat);
        sekretariat.MapGet("/", GetAntraegeForSekretariat);
        sekretariat.MapPut("/{antragId:guid}/bestaetigen", BestaetigeAntrag);

        // Schulleiter endpoints
        var schulleiter = app.MapGroup("/schulleiter")
            .RequireAuthorization(AuthorizationPolicies.Schulleiter);
        schulleiter.MapGet("/", GetAntraegeForSchulleiter);
        schulleiter.MapPut("/{antragId:guid}/bestaetigen", SchulleiterBestaetigen);
    }

    private static IResult GetLehrerListe(AfraAppContext dbContext)
    {
        var lehrer = dbContext.Personen
            .Where(p => p.Rolle == Rolle.Tutor)
            .OrderBy(p => p.LastName)
            .ThenBy(p => p.FirstName)
            .Select(p => new PersonInfoMinimal(p))
            .AsEnumerable();
        return Results.Ok(lehrer);
    }

    private static async Task<IResult> CreateAntrag(
        FreistellungsService service,
        UserAccessor userAccessor,
        CreateFreistellungsantragDto dto)
    {
        var student = await userAccessor.GetUserAsync();
        try
        {
            var result = await service.CreateAntragAsync(student, dto);
            return Results.Created($"/api/freistellung/sus/{result.Id}", result);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> GetAntraegeForStudent(
        FreistellungsService service,
        UserAccessor userAccessor)
    {
        var student = await userAccessor.GetUserAsync();
        var result = await service.GetAntraegeForStudentAsync(student);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetAntraegeForLehrer(
        FreistellungsService service,
        UserAccessor userAccessor)
    {
        var lehrer = await userAccessor.GetUserAsync();
        var result = await service.GetAntraegeForLehrerAsync(lehrer);
        return Results.Ok(result);
    }

    private static async Task<IResult> RecordEntscheidung(
        FreistellungsService service,
        UserAccessor userAccessor,
        Guid antragId,
        EntscheidungDto dto)
    {
        var lehrer = await userAccessor.GetUserAsync();
        try
        {
            var result = await service.RecordEntscheidungAsync(lehrer, antragId, dto);
            return Results.Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return Results.NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> GetAntraegeForSekretariat(
        FreistellungsService service)
    {
        var result = await service.GetAntraegeForSekretariatAsync();
        return Results.Ok(result);
    }

    private static async Task<IResult> BestaetigeAntrag(
        FreistellungsService service,
        Guid antragId)
    {
        try
        {
            var result = await service.BestaetigeAntragAsync(antragId);
            return Results.Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return Results.NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static async Task<IResult> GetAntraegeForSchulleiter(
        FreistellungsService service)
    {
        var result = await service.GetAntraegeForSchulleiterAsync();
        return Results.Ok(result);
    }

    private static async Task<IResult> SchulleiterBestaetigen(
        FreistellungsService service,
        Guid antragId)
    {
        try
        {
            var result = await service.SchulleiterBestaetigenAsync(antragId);
            return Results.Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return Results.NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }
}
