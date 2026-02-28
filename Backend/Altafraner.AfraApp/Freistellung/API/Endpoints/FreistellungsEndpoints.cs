using System.Net.Mime;
using Altafraner.AfraApp.Backbone.Authorization;
using Altafraner.AfraApp.Freistellung.Domain.DTO;
using Altafraner.AfraApp.Freistellung.Services;
using Altafraner.AfraApp.User.Services;
using Microsoft.AspNetCore.Http.HttpResults;

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
        app.MapGet("/offene-anzahl", GetOffeneAnzahl);

        var student = app.MapGroup("/sus")
            .RequireAuthorization(AuthorizationPolicies.StudentOnly);
        student.MapPost("/", CreateAntrag);
        student.MapGet("/", GetAntraegeForStudent);
        student.MapPut("/{antragId:guid}/elternbestaetigung-nachreichen", ElternbestaetigungNachreichen);

        var lehrer = app.MapGroup("/lehrer")
            .RequireAuthorization(AuthorizationPolicies.TutorOnly);
        lehrer.MapGet("/", GetAntraegeForLehrer);
        lehrer.MapPut("/{antragId:guid}/entscheidung", RecordEntscheidung);

        var sekretariat = app.MapGroup("/sekretariat")
            .RequireAuthorization(AuthorizationPolicies.Sekretariat);
        sekretariat.MapGet("/", GetAntraegeForSekretariat);
        sekretariat.MapPut("/{antragId:guid}/elternbestaetigung-entscheidung", EntscheidungElternbestaetigung);
        sekretariat.MapPut("/{antragId:guid}/cevex-erledigt", CevexErledigt);
        sekretariat.MapGet("/{antragId:guid}.pdf", GetAntragPdf);

        var schulleiter = app.MapGroup("/schulleiter")
            .RequireAuthorization(AuthorizationPolicies.Schulleiter);
        schulleiter.MapGet("/", GetAntraegeForSchulleiter);
        schulleiter.MapPut("/{antragId:guid}/bestaetigen", SchulleiterBestaetigen);
        schulleiter.MapPut("/{antragId:guid}/ablehnen", SchulleiterAblehnen);
        schulleiter.MapGet("/{antragId:guid}.pdf", GetAntragPdf);
    }

    /// <summary>
    ///     Runs <paramref name="action" /> and maps the domain exceptions used across the
    ///     Freistellung service onto the corresponding HTTP results.
    /// </summary>
    private static async Task<IResult> ExecuteAsync<T>(Func<Task<T>> action, Func<T, IResult>? onSuccess = null)
    {
        try
        {
            var result = await action();
            return onSuccess is null ? Results.Ok(result) : onSuccess(result);
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

    private static async Task<IResult> GetOffeneAnzahl(FreistellungsService service, UserAccessor userAccessor)
    {
        var person = await userAccessor.GetUserAsync();
        var anzahl = await service.GetOffeneAntraegeAnzahlAsync(person);
        return Results.Ok(anzahl);
    }

    private static async Task<IResult> CreateAntrag(
        FreistellungsService service,
        UserAccessor userAccessor,
        CreateFreistellungsantragDto dto)
    {
        var student = await userAccessor.GetUserAsync();
        return await ExecuteAsync(
            () => service.CreateAntragAsync(student, dto),
            result => Results.Created($"/api/freistellung/sus/{result.Id}", result));
    }

    private static async Task<IResult> GetAntraegeForStudent(
        FreistellungsService service,
        UserAccessor userAccessor)
    {
        var student = await userAccessor.GetUserAsync();
        var result = await service.GetAntraegeForStudentAsync(student);
        return Results.Ok(result);
    }

    private static async Task<IResult> ElternbestaetigungNachreichen(
        FreistellungsService service,
        UserAccessor userAccessor,
        Guid antragId)
    {
        var student = await userAccessor.GetUserAsync();
        return await ExecuteAsync(() => service.ElternbestaetigungNachreichenAsync(student, antragId));
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
        return await ExecuteAsync(() => service.RecordEntscheidungAsync(lehrer, antragId, dto));
    }

    private static async Task<IResult> GetAntraegeForSekretariat(
        FreistellungsService service)
    {
        var result = await service.GetAntraegeForSekretariatAsync();
        return Results.Ok(result);
    }

    private static async Task<IResult> EntscheidungElternbestaetigung(
        FreistellungsService service,
        Guid antragId,
        EntscheidungElternbestaetigungDto dto)
        => await ExecuteAsync(() => service.EntscheidungElternbestaetigungAsync(antragId, dto));

    private static async Task<IResult> CevexErledigt(
        FreistellungsService service,
        Guid antragId)
        => await ExecuteAsync(() => service.CevexErledigtAsync(antragId));

    private static async Task<IResult> GetAntraegeForSchulleiter(
        FreistellungsService service)
    {
        var result = await service.GetAntraegeForSchulleiterAsync();
        return Results.Ok(result);
    }

    private static async Task<IResult> SchulleiterBestaetigen(
        FreistellungsService service,
        Guid antragId)
        => await ExecuteAsync(() => service.SchulleiterBestaetigenAsync(antragId));

    private static async Task<IResult> SchulleiterAblehnen(
        FreistellungsService service,
        Guid antragId,
        AblehnungDto dto)
        => await ExecuteAsync(() => service.SchulleiterAblehnenAsync(antragId, dto));

    private static async Task<IResult> GetAntragPdf(
        FreistellungsService service,
        Guid antragId)
        => await ExecuteAsync(
            () => service.GeneratePdfAsync(antragId),
            pdf => Results.File(pdf, MediaTypeNames.Application.Pdf, $"Freistellungsantrag_{antragId}.pdf"));
}
