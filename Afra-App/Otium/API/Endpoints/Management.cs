using Afra_App.Backbone.Authentication;
using Afra_App.Otium.Services;
using Afra_App.User.Services;
using DTO_Otium_Creation = Afra_App.Otium.Domain.DTO.ManagementOtiumCreation;
using DTO_Termin_Creation = Afra_App.Otium.Domain.DTO.ManagementTerminCreation;
using DTO_Wiederholung_Creation = Afra_App.Otium.Domain.DTO.ManagementWiederholungCreation;

namespace Afra_App.Otium.API.Endpoints;

/// <summary>
///     A class containing the endpoints for the management of otia.
/// </summary>
public static class Management
{
    /// <summary>
    ///     Maps the management endpoints to the given <see cref="IEndpointRouteBuilder" />.
    /// </summary>
    /// <param name="app"></param>
    public static void MapManagementEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/management")
            .WithOpenApi()
            .RequireAuthorization(AuthorizationPolicies.TutorOnly);

        group.MapGet("/otium", GetOtia);
        group.MapGet("/otium/{otiumId:guid}", GetOtium);
        group.MapPost("/otium", CreateOtium);
        group.MapDelete("/otium/{otiumId:guid}", DeleteOtium);
        group.MapPatch("/otium/{otiumId:guid}/bezeichnung", OtiumSetBezeichnung);
        group.MapPatch("/otium/{otiumId:guid}/beschreibung", OtiumSetBeschreibung);
        group.MapPatch("/otium/{otiumId:guid}/kategorie", OtiumSetKategorie);
        group.MapPost("/otium/{otiumId:guid}/verantwortliche", OtiumAddVerantwortlich);
        group.MapDelete("/otium/{otiumId:guid}/verantwortliche/{persId:guid}", OtiumRemoveVerantwortlich);

        group.MapGet("/termin/{otiumTerminId:guid}", GetTerminForTeacher);
        group.MapPost("/termin", CreateOtiumTermin);
        group.MapDelete("/termin/{otiumTerminId:guid}", DeleteOtiumTermin);
        group.MapPut("/termin/{otiumTerminId:guid}/cancel", OtiumTerminAbsagen);
        group.MapPatch("/termin/{otiumTerminId:guid}/maxEinschreibungen", OtiumTerminSetMaxEinschreibungen);
        group.MapPatch("/termin/{otiumTerminId:guid}/tutor", OtiumTerminSetTutor);
        group.MapPatch("/termin/{otiumTerminId:guid}/ort", OtiumTerminSetOrt);

        group.MapPost("/wiederholung", CreateOtiumWiederholung);
        group.MapDelete("/wiederholung/{otiumWiederholungId:guid}", DeleteOtiumWiederholung);
        group.MapPatch("/wiederholung/{otiumWiederholungId:guid}/discontinue", OtiumWiederholungDiscontinue);
    }

    private static async Task<IResult> GetTerminForTeacher(
        OtiumEndpointService service,
        UserAccessor userAccessor,
        Guid otiumTerminId)
    {
        var user = await userAccessor.GetUserAsync();

        var otium = await service.GetTerminForTeacher(otiumTerminId, user);

        return otium is null ? Results.BadRequest() : Results.Ok(otium);
    }

    private static IResult GetOtia(OtiumEndpointService service)
    {
        var otia = service.GetOtia();
        return Results.Ok(otia);
    }

    private static IResult GetOtium(OtiumEndpointService service, Guid otiumId)
    {
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

    private static async Task<IResult> CreateOtium(OtiumEndpointService service, DTO_Otium_Creation otium)
    {
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

    private static async Task<IResult> DeleteOtium(OtiumEndpointService service, Guid otiumId)
    {
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

    private static async Task<IResult> CreateOtiumTermin(OtiumEndpointService service, DTO_Termin_Creation otiumTermin)
    {
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

    private static async Task<IResult> DeleteOtiumTermin(OtiumEndpointService service, Guid otiumTerminId)
    {
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

    private static async Task<IResult> CreateOtiumWiederholung(OtiumEndpointService service,
        DTO_Wiederholung_Creation otiumWiederholung)
    {
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

    private static async Task<IResult> DeleteOtiumWiederholung(OtiumEndpointService service, Guid otiumWiederholungId)
    {
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
        Guid otiumWiederholungId, DateOnlyWrapper firstDayAfter)
    {
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

    private static async Task<IResult> OtiumTerminAbsagen(OtiumEndpointService service, Guid otiumTerminId)
    {
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

    private static async Task<IResult> OtiumSetBezeichnung(OtiumEndpointService service, Guid otiumId,
        StringWrapper value)
    {
        if (string.IsNullOrWhiteSpace(value.Value) || value.Value.Length <= 3 || value.Value.Length > 50)
            return Results.BadRequest();

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

    private static async Task<IResult> OtiumSetBeschreibung(OtiumEndpointService service, Guid otiumId,
        StringWrapper value)
    {
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

    private static async Task<IResult> OtiumAddVerantwortlich(OtiumEndpointService service, Guid otiumId, Guid persId)
    {
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

    private static async Task<IResult> OtiumRemoveVerantwortlich(OtiumEndpointService service, Guid otiumId,
        Guid persId)
    {
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

    private static async Task<IResult> OtiumSetKategorie(OtiumEndpointService service, Guid otiumId,
        GuidWrapper kategorie)
    {
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
        Guid otiumTerminId, IntOrNullWrapper maxEinschreibungen)
    {
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

    private static async Task<IResult> OtiumTerminSetTutor(OtiumEndpointService service, Guid otiumTerminId,
        GuidOrNullWrapper personId)
    {
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

    private static async Task<IResult> OtiumTerminSetOrt(OtiumEndpointService service, Guid otiumTerminId,
        StringWrapper ort)
    {
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