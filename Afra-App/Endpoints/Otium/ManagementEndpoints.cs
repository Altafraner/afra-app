using Afra_App.Authentication;
using Afra_App.Data;
using Afra_App.Data.People;
using Afra_App.Services.Otium;
using Microsoft.AspNetCore.Mvc;
using DTO_Otium_Creation = Afra_App.Data.DTO.Otium.ManagementOtiumCreation;
using DTO_Termin_Creation = Afra_App.Data.DTO.Otium.ManagementTerminCreation;
using DTO_Wiederholung_Creation = Afra_App.Data.DTO.Otium.ManagementWiederholungCreation;

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
        app.MapGet("/management/otium", GetOtia);
        app.MapGet("/management/otium/{otiumId:guid}", GetOtium);
        app.MapPost("/management/otium", CreateOtium);
        app.MapDelete("/management/otium/{otiumId:guid}", DeleteOtium);
        app.MapPost("/management/otium/{otiumId:guid}/termine", CreateOtiumTermin);
        app.MapDelete("/management/otium/{otiumId:guid}/termine/{otiumTerminId:guid}", DeleteOtiumTermin);
        app.MapPost("/management/otium/{otiumId:guid}/wiederholungen", CreateOtiumWiederholung);
        app.MapPut("/management/otium/{otiumId:guid}/termine/{otiumTerminId:guid}/cancel", OtiumTerminAbsagen);
        app.MapPut("/management/otium/{otiumId:guid}/bezeichnung", OtiumSetBezeichnung);
        app.MapPut("/management/otium/{otiumId:guid}/beschreibung", OtiumSetBeschreibung);
        app.MapPost("/management/otium/{otiumId:guid}/verantwortliche", OtiumAddVerantwortlich);
        app.MapDelete("/management/otium/{otiumId:guid}/verantwortliche/{persId:guid}", OtiumRemoveVerantwortlich);
    }

    private static async Task<IResult> GetTerminForTeacher(OtiumEndpointService service, HttpContext httpContext,
        AfraAppContext context, Guid terminId)
    {
        var user = await httpContext.GetPersonAsync(context);
        if (user.Rolle != Rolle.Tutor) return Results.Unauthorized();

        return Results.Ok(service.GetTerminForTeacher(terminId, user));
    }

    private static async Task<IResult> GetOtia(OtiumEndpointService service, HttpContext httpContext,
        AfraAppContext context)
    {
        var user = await httpContext.GetPersonAsync(context);
        if (user.Rolle != Rolle.Tutor) return Results.Unauthorized();
        var otia = service.GetOtia();
        return Results.Ok(otia);
    }

    private static async Task<IResult> GetOtium(OtiumEndpointService service, HttpContext httpContext,
        AfraAppContext context, Guid otiumId)
    {
        var user = await httpContext.GetPersonAsync(context);
        if (user.Rolle != Rolle.Tutor) return Results.Unauthorized();
        try
        {
            var otium = service.GetOtium(otiumId);
            return Results.Ok(otium);
        }
        catch (OtiumEndpointService.EntityNotFoundException)
        {
            return Results.NotFound();
        }
    }

    private static async Task<IResult> CreateOtium(OtiumEndpointService service, HttpContext httpContext,
        AfraAppContext context, DTO_Otium_Creation otium)
    {
        var user = await httpContext.GetPersonAsync(context);
        if (user.Rolle != Rolle.Tutor) return Results.Unauthorized();
        try
        {
            var id = await service.CreateOtiumAsync(otium);
            return Results.Ok(id);
        }
        catch (ArgumentException e)
        {
            return Results.Conflict(e.Message);
        }
    }

    private static async Task<IResult> DeleteOtium(OtiumEndpointService service, HttpContext httpContext,
        AfraAppContext context, Guid otiumId)
    {
        var user = await httpContext.GetPersonAsync(context);
        if (user.Rolle != Rolle.Tutor) return Results.Unauthorized();
        try
        {
            await service.DeleteOtiumAsync(otiumId);
            return Results.Ok();
        }
        catch (OtiumEndpointService.EntityNotFoundException)
        {
            return Results.NotFound();
        }
        catch (OtiumEndpointService.EntityDeletionException e)
        {
            return Results.Conflict(e.Message);
        }
    }

    private static async Task<IResult> CreateOtiumTermin(OtiumEndpointService service, HttpContext httpContext,
        AfraAppContext context, Guid otiumId, DTO_Termin_Creation otiumTermin)
    {
        var user = await httpContext.GetPersonAsync(context);
        if (user.Rolle != Rolle.Tutor) return Results.Unauthorized();

        try
        {
            var id = await service.CreateOtiumTerminAsync(otiumId, otiumTermin);
            return Results.Ok(id);
        }
        catch (ArgumentException e)
        {
            return Results.Conflict(e.Message);
        }
    }

    private static async Task<IResult> DeleteOtiumTermin(OtiumEndpointService service, HttpContext httpContext,
        AfraAppContext context, Guid otiumId, Guid otiumTerminId)
    {
        var user = await httpContext.GetPersonAsync(context);
        if (user.Rolle != Rolle.Tutor) return Results.Unauthorized();

        try
        {
            await service.DeleteOtiumTerminAsync(otiumId, otiumTerminId);
            return Results.Ok();
        }
        catch (OtiumEndpointService.EntityNotFoundException)
        {
            return Results.NotFound();
        }
        catch (OtiumEndpointService.EntityDeletionException e)
        {
            return Results.Conflict(e.Message);
        }
    }

    private static async Task<IResult> CreateOtiumWiederholung(OtiumEndpointService service, HttpContext httpContext,
        AfraAppContext context, Guid otiumId, DTO_Wiederholung_Creation otiumWiederholung)
    {
        var user = await httpContext.GetPersonAsync(context);
        if (user.Rolle != Rolle.Tutor) return Results.Unauthorized();

        try
        {
            var id = await service.CreateOtiumWiederholungAsync(otiumId, otiumWiederholung);
            return Results.Ok(id);
        }
        catch (ArgumentException e)
        {
            return Results.Conflict(e.Message);
        }
    }

    private static async Task<IResult> DeleteOtiumWiederholung(OtiumEndpointService service, HttpContext httpContext,
        AfraAppContext context, Guid otiumId, Guid otiumWiederholungId)
    {
        var user = await httpContext.GetPersonAsync(context);
        if (user.Rolle != Rolle.Tutor) return Results.Unauthorized();

        try
        {
            await service.DeleteOtiumWiederholungAsync(otiumId, otiumWiederholungId);
            return Results.Ok();
        }
        catch (OtiumEndpointService.EntityNotFoundException)
        {
            return Results.NotFound();
        }
        catch (OtiumEndpointService.EntityDeletionException e)
        {
            return Results.Conflict(e.Message);
        }
    }

    private static async Task<IResult> OtiumTerminAbsagen(OtiumEndpointService service, HttpContext httpContext,
        AfraAppContext context, Guid otiumId, Guid otiumTerminId)
    {
        var user = await httpContext.GetPersonAsync(context);
        if (user.Rolle != Rolle.Tutor) return Results.Unauthorized();

        try
        {
            await service.OtiumTerminAbsagenAsync(otiumId, otiumTerminId);
            return Results.Ok();
        }
        catch (OtiumEndpointService.EntityNotFoundException)
        {
            return Results.NotFound();
        }
        catch (OtiumEndpointService.EntityDeletionException e)
        {
            return Results.Conflict(e.Message);
        }
    }

    private static async Task<IResult> OtiumSetBezeichnung(OtiumEndpointService service, HttpContext httpContext,
        AfraAppContext context, Guid otiumId, StringValue value)
    {
        if (string.IsNullOrWhiteSpace(value.Value) || value.Value.Length <= 3 || value.Value.Length > 50)
            return Results.BadRequest();

        var user = await httpContext.GetPersonAsync(context);
        if (user.Rolle != Rolle.Tutor) return Results.Unauthorized();

        try
        {
            await service.OtiumSetBezeichnungAsync(otiumId, value.Value);
            return Results.Ok();
        }
        catch (OtiumEndpointService.EntityNotFoundException)
        {
            return Results.NotFound();
        }
    }

    private static async Task<IResult> OtiumSetBeschreibung(OtiumEndpointService service, HttpContext httpContext,
        AfraAppContext context, Guid otiumId, StringValue value)
    {
        var user = await httpContext.GetPersonAsync(context);
        if (user.Rolle != Rolle.Tutor) return Results.Unauthorized();

        try
        {
            await service.OtiumSetBeschreibungAsync(otiumId, value.Value);
            return Results.Ok();
        }
        catch (OtiumEndpointService.EntityNotFoundException)
        {
            return Results.NotFound();
        }
    }

    private static async Task<IResult> OtiumAddVerantwortlich(OtiumEndpointService service, HttpContext httpContext,
        AfraAppContext context, Guid otiumId, [FromBody] Guid persId)
    {
        var user = await httpContext.GetPersonAsync(context);
        if (user.Rolle != Rolle.Tutor) return Results.Unauthorized();

        try
        {
            await service.OtiumAddVerantwortlichAsync(otiumId, persId);
            return Results.Ok();
        }
        catch (OtiumEndpointService.EntityNotFoundException)
        {
            return Results.NotFound();
        }
    }

    private static async Task<IResult> OtiumRemoveVerantwortlich(OtiumEndpointService service, HttpContext httpContext,
        AfraAppContext context, Guid otiumId, Guid persId)
    {
        var user = await httpContext.GetPersonAsync(context);
        if (user.Rolle != Rolle.Tutor) return Results.Unauthorized();

        try
        {
            await service.OtiumRemoveVerantwortlichAsync(otiumId, persId);
            return Results.Ok();
        }
        catch (OtiumEndpointService.EntityNotFoundException)
        {
            return Results.NotFound();
        }
    }

    private record StringValue(string Value);
}