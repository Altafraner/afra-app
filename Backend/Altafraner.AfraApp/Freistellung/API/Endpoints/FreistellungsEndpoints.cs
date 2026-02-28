using Altafraner.AfraApp.Backbone.Authorization;
using Altafraner.AfraApp.Freistellung.Domain.DTO;
using Altafraner.AfraApp.Freistellung.Services;
using Altafraner.AfraApp.User.Services;

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
        var student = app.MapGroup("/sus")
            .RequireAuthorization(AuthorizationPolicies.StudentOnly);
        student.MapPost("/", CreateAntrag);
        student.MapGet("/", GetAntraegeForStudent);
        student.MapPut("/{antragId:guid}/erneut-einreichen", ErneutEinreichen);

        var lehrer = app.MapGroup("/lehrer")
            .RequireAuthorization(AuthorizationPolicies.TutorOnly);
        lehrer.MapGet("/", GetAntraegeForLehrer);
        lehrer.MapPut("/{antragId:guid}/entscheidung", RecordEntscheidung);

        var sekretariat = app.MapGroup("/sekretariat")
            .RequireAuthorization(AuthorizationPolicies.Sekretariat);
        sekretariat.MapGet("/", GetAntraegeForSekretariat);
        sekretariat.MapPut("/{antragId:guid}/bestaetigen", BestaetigeAntrag);
        sekretariat.MapPut("/{antragId:guid}/ablehnen", SekretariatAblehnen);

        var schulleiter = app.MapGroup("/schulleiter")
            .RequireAuthorization(AuthorizationPolicies.Schulleiter);
        schulleiter.MapGet("/", GetAntraegeForSchulleiter);
        schulleiter.MapPut("/{antragId:guid}/bestaetigen", SchulleiterBestaetigen);
        schulleiter.MapPut("/{antragId:guid}/ablehnen", SchulleiterAblehnen);
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

    private static async Task<IResult> SekretariatAblehnen(
        FreistellungsService service,
        Guid antragId,
        AblehnungDto dto)
    {
        try
        {
            var result = await service.SekretariatAblehnenAsync(antragId, dto);
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

    private static async Task<IResult> SchulleiterAblehnen(
        FreistellungsService service,
        Guid antragId,
        AblehnungDto dto)
    {
        try
        {
            var result = await service.SchulleiterAblehnenAsync(antragId, dto);
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

    private static async Task<IResult> ErneutEinreichen(
        FreistellungsService service,
        UserAccessor userAccessor,
        Guid antragId)
    {
        var student = await userAccessor.GetUserAsync();
        try
        {
            var result = await service.ErneutEinreichenAsync(student, antragId);
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
