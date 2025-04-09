using Afra_App.Authentication;
using Afra_App.Data;
using Afra_App.Data.People;
using Afra_App.Services.Otium;
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
        app.MapGet("/management/otium", GetOtia);
        app.MapGet("/management/otium/{otiumId:guid}", GetOtium);
        app.MapPost("/management/otium", CreateOtium);
        app.MapDelete("/management/otium/{otiumId:guid}", DeleteOtium);
        app.MapPatch("/management/otium/{otiumId:guid}/bezeichnung", OtiumSetBezeichnung);
        app.MapPatch("/management/otium/{otiumId:guid}/beschreibung", OtiumSetBeschreibung);
        app.MapPatch("/management/otium/{otiumId:guid}/kategorie", OtiumSetKategorie);
        app.MapPost("/management/otium/{otiumId:guid}/verantwortliche", OtiumAddVerantwortlich);
        app.MapDelete("/management/otium/{otiumId:guid}/verantwortliche/{persId:guid}", OtiumRemoveVerantwortlich);

        app.MapGet("/management/termin/{otiumTerminId:guid}", GetTerminForTeacher);
        app.MapPost("/management/termin", CreateOtiumTermin);
        app.MapDelete("/management/termin/{otiumTerminId:guid}", DeleteOtiumTermin);
        app.MapPut("/management/termin/{otiumTerminId:guid}/cancel", OtiumTerminAbsagen);
        app.MapPatch("/management/termin/{otiumTerminId:guid}/maxEinschreibungen", OtiumTerminSetMaxEinschreibungen);
        app.MapPatch("/management/termin/{otiumTerminId:guid}/tutor", OtiumTerminSetTutor);
        app.MapPatch("/management/termin/{otiumTerminId:guid}/ort", OtiumTerminSetOrt);

        app.MapPost("/management/wiederholung", CreateOtiumWiederholung);
        app.MapDelete("/management/wiederholung/{otiumWiederholungId:guid}", DeleteOtiumWiederholung);
        app.MapPatch("/management/wiederholung/{otiumWiederholungId:guid}/discontinue", OtiumWiederholungDiscontinue);
    }

    private static async Task<IResult> GetTerminForTeacher(OtiumEndpointService service, HttpContext httpContext,
        AfraAppContext context, Guid otiumTerminId)
    {
        var user = await httpContext.GetPersonAsync(context);
        if (user.Rolle != Rolle.Tutor) return Results.Unauthorized();

        var otium = await service.GetTerminForTeacher(otiumTerminId, user);

        return otium is null ? Results.BadRequest() : Results.Ok(otium);
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
        AfraAppContext context, DTO_Termin_Creation otiumTermin)
    {
        var user = await httpContext.GetPersonAsync(context);
        if (user.Rolle != Rolle.Tutor) return Results.Unauthorized();

        try
        {
            var id = await service.CreateOtiumTerminAsync(otiumTermin);
            return Results.Ok(id);
        }
        catch (ArgumentNullException e)
        {
            return Results.BadRequest(e.Message);
        }
        catch (ArgumentException e)
        {
            return Results.Conflict(e.Message);
        }
    }

    private static async Task<IResult> DeleteOtiumTermin(OtiumEndpointService service, HttpContext httpContext,
        AfraAppContext context, Guid otiumTerminId)
    {
        var user = await httpContext.GetPersonAsync(context);
        if (user.Rolle != Rolle.Tutor) return Results.Unauthorized();

        try
        {
            await service.DeleteOtiumTerminAsync(otiumTerminId);
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
        AfraAppContext context, DTO_Wiederholung_Creation otiumWiederholung)
    {
        var user = await httpContext.GetPersonAsync(context);
        if (user.Rolle != Rolle.Tutor) return Results.Unauthorized();

        try
        {
            var id = await service.CreateOtiumWiederholungAsync(otiumWiederholung);
            return Results.Ok(id);
        }
        catch (ArgumentException e)
        {
            return Results.Conflict(e.Message);
        }
    }

    private static async Task<IResult> DeleteOtiumWiederholung(OtiumEndpointService service, HttpContext httpContext,
        AfraAppContext context, Guid otiumWiederholungId)
    {
        var user = await httpContext.GetPersonAsync(context);
        if (user.Rolle != Rolle.Tutor) return Results.Unauthorized();

        try
        {
            await service.DeleteOtiumWiederholungAsync(otiumWiederholungId);
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

    private static async Task<IResult> OtiumWiederholungDiscontinue(OtiumEndpointService service,
        HttpContext httpContext,
        AfraAppContext context, Guid otiumWiederholungId, DateOnlyWrapper firstDayAfter)
    {
        var user = await httpContext.GetPersonAsync(context);
        if (user.Rolle != Rolle.Tutor) return Results.Unauthorized();

        try
        {
            await service.OtiumWiederholungDiscontinueAsync(otiumWiederholungId, firstDayAfter.Value);
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
        catch (ArgumentException e)
        {
            return Results.BadRequest(e.Message);
        }
    }

    private static async Task<IResult> OtiumTerminAbsagen(OtiumEndpointService service, HttpContext httpContext,
        AfraAppContext context, Guid otiumTerminId)
    {
        var user = await httpContext.GetPersonAsync(context);
        if (user.Rolle != Rolle.Tutor) return Results.Unauthorized();

        try
        {
            await service.OtiumTerminAbsagenAsync(otiumTerminId);
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
        AfraAppContext context, Guid otiumId, StringWrapper value)
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
        AfraAppContext context, Guid otiumId, StringWrapper value)
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
        AfraAppContext context, Guid otiumId, Guid persId)
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

    private static async Task<IResult> OtiumSetKategorie(OtiumEndpointService service, HttpContext httpContext,
        AfraAppContext context, Guid otiumId, GuidWrapper kategorie)
    {
        var user = await httpContext.GetPersonAsync(context);
        if (user.Rolle != Rolle.Tutor) return Results.Unauthorized();

        try
        {
            await service.OtiumSetKategorieAsync(otiumId, kategorie.Value);
            return Results.Ok();
        }
        catch (OtiumEndpointService.EntityNotFoundException)
        {
            return Results.NotFound();
        }
        catch (InvalidOperationException e)
        {
            return Results.BadRequest(e.Message);
        }
    }

    private static async Task<IResult> OtiumTerminSetMaxEinschreibungen(OtiumEndpointService service,
        HttpContext httpContext,
        AfraAppContext context, Guid otiumTerminId, IntOrNullWrapper maxEinschreibungen)
    {
        var user = await httpContext.GetPersonAsync(context);
        if (user.Rolle != Rolle.Tutor) return Results.Unauthorized();

        try
        {
            await service.OtiumTerminSetMaxEinschreibungenAsync(otiumTerminId, maxEinschreibungen.Value);
            return Results.Ok();
        }
        catch (OtiumEndpointService.EntityNotFoundException)
        {
            return Results.NotFound();
        }
        catch (InvalidOperationException)
        {
            return Results.BadRequest();
        }
    }

    private static async Task<IResult> OtiumTerminSetTutor(OtiumEndpointService service,
        HttpContext httpContext,
        AfraAppContext context, Guid otiumTerminId, GuidOrNullWrapper personId)
    {
        var user = await httpContext.GetPersonAsync(context);
        if (user.Rolle != Rolle.Tutor) return Results.Unauthorized();

        try
        {
            await service.OtiumTerminSetTutorAsync(otiumTerminId, personId.Value);
            return Results.Ok();
        }
        catch (OtiumEndpointService.EntityNotFoundException)
        {
            return Results.NotFound();
        }
        catch (InvalidOperationException)
        {
            return Results.BadRequest();
        }
    }

    private static async Task<IResult> OtiumTerminSetOrt(OtiumEndpointService service,
        HttpContext httpContext,
        AfraAppContext context, Guid otiumTerminId, StringWrapper ort)
    {
        var user = await httpContext.GetPersonAsync(context);
        if (user.Rolle != Rolle.Tutor) return Results.Unauthorized();

        try
        {
            await service.OtiumTerminSetOrtAsync(otiumTerminId, ort.Value);
            return Results.Ok();
        }
        catch (OtiumEndpointService.EntityNotFoundException)
        {
            return Results.NotFound();
        }
        catch (InvalidOperationException)
        {
            return Results.BadRequest();
        }
    }

    private record StringWrapper(string Value);

    private record IntOrNullWrapper(int? Value);

    private record GuidWrapper(Guid Value);

    private record GuidOrNullWrapper(Guid? Value);

    private record DateOnlyWrapper(DateOnly Value);
}