using Afra_App.Backbone.Authentication;
using Afra_App.Otium.Domain.Models;
using Afra_App.Otium.Services;
using Afra_App.User.Domain.Models;
using Afra_App.User.Services;
using Microsoft.EntityFrameworkCore;
using DB_Otium = Afra_App.Otium.Domain.Models.OtiumDefinition;
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
        group.MapPost("/otium", CreateOtium)
            .RequireAuthorization(AuthorizationPolicies.Otiumsverantwortlich);
        group.MapDelete("/otium/{otiumId:guid}", DeleteOtium)
            .RequireAuthorization(AuthorizationPolicies.Otiumsverantwortlich);
        group.MapPatch("/otium/{otiumId:guid}/bezeichnung", OtiumSetBezeichnung);
        group.MapPatch("/otium/{otiumId:guid}/beschreibung", OtiumSetBeschreibung);
        group.MapPatch("/otium/{otiumId:guid}/kategorie", OtiumSetKategorie);
        group.MapPost("/otium/{otiumId:guid}/verantwortliche", OtiumAddVerantwortlich)
            .RequireAuthorization(AuthorizationPolicies.Otiumsverantwortlich);
        group.MapDelete("/otium/{otiumId:guid}/verantwortliche/{persId:guid}", OtiumRemoveVerantwortlich)
            .RequireAuthorization(AuthorizationPolicies.Otiumsverantwortlich);

        var termin = group.MapGroup("/termin");
        termin.MapGet("/{otiumTerminId:guid}", GetTerminForTeacher);
        termin.MapPost("", CreateOtiumTermin);
        termin.MapDelete("/{otiumTerminId:guid}", DeleteOtiumTermin);
        termin.MapPut("/{otiumTerminId:guid}/cancel", OtiumTerminAbsagen);
        termin.MapDelete("/{otiumTerminId:guid}/cancel", OtiumTerminFortsetzen);
        termin.MapPatch("/{otiumTerminId:guid}/maxEinschreibungen", OtiumTerminSetMaxEinschreibungen);
        termin.MapPatch("/{otiumTerminId:guid}/tutor", OtiumTerminSetTutor);
        termin.MapPatch("/{otiumTerminId:guid}/ort", OtiumTerminSetOrt);
        termin.MapPost("/{otiumTerminId:guid}/student", OtiumTerminForceUnenroll);

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

    private static async Task<IResult> CreateOtiumTermin(ManagementService managementService,
        UserAuthorizationHelper authHelper, OtiumEndpointService service, DTO_Termin_Creation otiumTermin)
    {
        DB_Otium otium;
        try
        {
            otium = await managementService.GetOtiumByIdAsync(otiumTermin.OtiumId);
        }
        catch (KeyNotFoundException)
        {
            return Results.NotFound("Otium not found.");
        }

        if (!await MayEditAsync(authHelper, managementService, otium)) return Results.Forbid();

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

    private static async Task<IResult> DeleteOtiumTermin(ManagementService managementService,
        UserAuthorizationHelper authHelper, OtiumEndpointService service, Guid otiumTerminId)
    {
        DB_Otium otium;
        try
        {
            var termin = await managementService.GetTerminByIdAsync(otiumTerminId);
            otium = await managementService.GetOtiumOfTerminAsync(termin);
        }
        catch (KeyNotFoundException)
        {
            return Results.NotFound("Otium not found.");
        }

        if (!await MayEditAsync(authHelper, managementService, otium)) return Results.Forbid();

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

    private static async Task<IResult> CreateOtiumWiederholung(ManagementService managementService,
        UserAuthorizationHelper authHelper, OtiumEndpointService service,
        DTO_Wiederholung_Creation otiumWiederholung)
    {
        DB_Otium otium;
        try
        {
            otium = await managementService.GetOtiumByIdAsync(otiumWiederholung.OtiumId);
        }
        catch (KeyNotFoundException)
        {
            return Results.NotFound("Otium not found.");
        }

        if (!await MayEditAsync(authHelper, managementService, otium)) return Results.Forbid();

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

    private static async Task<IResult> DeleteOtiumWiederholung(ManagementService managementService,
        UserAuthorizationHelper authHelper, OtiumEndpointService service, Guid otiumWiederholungId)
    {
        DB_Otium otium;
        try
        {
            var wdh = await managementService.GetWiederholungByIdAsync(otiumWiederholungId);
            otium = await managementService.GetOtiumOfWiederholungAsync(wdh);
        }
        catch (KeyNotFoundException)
        {
            return Results.NotFound("Otium not found.");
        }

        if (!await MayEditAsync(authHelper, managementService, otium)) return Results.Forbid();
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

    private static async Task<IResult> OtiumWiederholungDiscontinue(ManagementService managementService,
        UserAuthorizationHelper authHelper, OtiumEndpointService service,
        Guid otiumWiederholungId, DateOnlyWrapper firstDayAfter)
    {
        DB_Otium otium;
        try
        {
            var wdh = await managementService.GetWiederholungByIdAsync(otiumWiederholungId);
            otium = await managementService.GetOtiumOfWiederholungAsync(wdh);
        }
        catch (KeyNotFoundException)
        {
            return Results.NotFound("Otium not found.");
        }

        if (!await MayEditAsync(authHelper, managementService, otium)) return Results.Forbid();
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

    private static async Task<IResult> OtiumTerminAbsagen(ManagementService managementService,
        UserAuthorizationHelper authHelper, OtiumEndpointService service, Guid otiumTerminId)
    {
        DB_Otium otium;
        try
        {
            var termin = await managementService.GetTerminByIdAsync(otiumTerminId);
            otium = await managementService.GetOtiumOfTerminAsync(termin);
        }
        catch (KeyNotFoundException)
        {
            return Results.NotFound("Otium not found.");
        }

        if (!await MayEditAsync(authHelper, managementService, otium)) return Results.Forbid();

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

    private static async Task<IResult> OtiumTerminFortsetzen(ManagementService managementService,
        UserAuthorizationHelper authHelper, Guid otiumTerminId)
    {
        DB_Otium otium;
        OtiumTermin termin;
        try
        {
            termin = await managementService.GetTerminByIdAsync(otiumTerminId);
            otium = await managementService.GetOtiumOfTerminAsync(termin);
        }
        catch (KeyNotFoundException)
        {
            return Results.NotFound("Otium not found.");
        }

        if (!await MayEditAsync(authHelper, managementService, otium)) return Results.Forbid();

        try
        {
            await managementService.ContinueTerminAsync(termin);
            return Results.Ok();
        }
        catch (InvalidOperationException)
        {
            return Results.ValidationProblem(
                new Dictionary<string, string[]>
                {
                    ["otiumTerminId"] = ["The termin is not cancelled."]
                }
            );
        }
    }

    private static async Task<IResult> OtiumSetBezeichnung(ManagementService managementService,
        UserAuthorizationHelper authHelper, OtiumEndpointService service, Guid otiumId,
        StringWrapper value)
    {
        DB_Otium otium;
        try
        {
            otium = await managementService.GetOtiumByIdAsync(otiumId);
        }
        catch (KeyNotFoundException)
        {
            return Results.NotFound("Otium not found.");
        }

        if (!await MayEditAsync(authHelper, managementService, otium)) return Results.Forbid();

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

    private static async Task<IResult> OtiumSetBeschreibung(ManagementService managementService,
        UserAuthorizationHelper authHelper, OtiumEndpointService service, Guid otiumId,
        StringWrapper value)
    {
        DB_Otium otium;
        try
        {
            otium = await managementService.GetOtiumByIdAsync(otiumId);
        }
        catch (KeyNotFoundException)
        {
            return Results.NotFound("Otium not found.");
        }

        if (!await MayEditAsync(authHelper, managementService, otium)) return Results.Forbid();

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

    private static async Task<IResult> OtiumSetKategorie(ManagementService managementService,
        UserAuthorizationHelper authHelper, OtiumEndpointService service, Guid otiumId,
        GuidWrapper kategorie)
    {
        DB_Otium otium;
        try
        {
            otium = await managementService.GetOtiumByIdAsync(otiumId);
        }
        catch (KeyNotFoundException)
        {
            return Results.NotFound("Otium not found.");
        }

        if (!await MayEditAsync(authHelper, managementService, otium)) return Results.Forbid();

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

    private static async Task<IResult> OtiumTerminSetMaxEinschreibungen(ManagementService managementService,
        UserAuthorizationHelper authHelper, OtiumEndpointService service,
        Guid otiumTerminId, IntOrNullWrapper maxEinschreibungen)
    {
        DB_Otium otium;
        try
        {
            var termin = await managementService.GetTerminByIdAsync(otiumTerminId);
            otium = await managementService.GetOtiumOfTerminAsync(termin);
        }
        catch (KeyNotFoundException)
        {
            return Results.NotFound("Otium not found.");
        }

        if (!await MayEditAsync(authHelper, managementService, otium)) return Results.Forbid();

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

    private static async Task<IResult> OtiumTerminSetTutor(ManagementService managementService,
        UserAuthorizationHelper authHelper, OtiumEndpointService service, Guid otiumTerminId,
        GuidOrNullWrapper personId)
    {
        DB_Otium otium;
        try
        {
            var termin = await managementService.GetTerminByIdAsync(otiumTerminId);
            otium = await managementService.GetOtiumOfTerminAsync(termin);
        }
        catch (KeyNotFoundException)
        {
            return Results.NotFound("Otium not found.");
        }

        if (!await MayEditAsync(authHelper, managementService, otium)) return Results.Forbid();

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

    private static async Task<IResult> OtiumTerminSetOrt(ManagementService managementService,
        UserAuthorizationHelper authHelper, OtiumEndpointService service, Guid otiumTerminId,
        StringWrapper ort)
    {
        DB_Otium otium;
        try
        {
            var termin = await managementService.GetTerminByIdAsync(otiumTerminId);
            otium = await managementService.GetOtiumOfTerminAsync(termin);
        }
        catch (KeyNotFoundException)
        {
            return Results.NotFound("Otium not found.");
        }

        if (!await MayEditAsync(authHelper, managementService, otium)) return Results.Forbid();

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

    private static async Task<IResult> OtiumTerminForceUnenroll(Guid otiumTerminId, GuidWrapper personIdWrapper,
        UserAuthorizationHelper authHelper, AfraAppContext dbContext, UserService userService,
        EnrollmentService enrollmentService, BlockHelper blockHelper)
    {
        var user = await authHelper.GetUserAsync();
        var termin = await dbContext.OtiaTermine
            .AsNoTracking()
            .Include(t => t.Otium)
            .ThenInclude(o => o.Verantwortliche)
            .Include(t => t.Block)
            .Where(t => t.Id == otiumTerminId)
            .Select(t => new
            {
                t.Block,
                t.Otium.Verantwortliche,
                IsTutor = t.Tutor != null && t.Tutor.Id == user.Id
            })
            .FirstOrDefaultAsync();

        if (termin is null) return Results.NotFound("Termin oder Otium nicht gefunden");

        if (!termin.IsTutor && !await MayEditAsync(user, authHelper, termin.Verantwortliche))
            return Results.Forbid();

        if (blockHelper.IsBlockDoneOrRunning(termin.Block))
            return Results.BadRequest("Der Block ist bereits abgeschlossen oder läuft.");

        try
        {
            var student = await userService.GetUserByIdAsync(personIdWrapper.Value);
            await enrollmentService.UnenrollAsync(otiumTerminId, student, true);
            return Results.Ok();
        }
        catch (OtiumEndpointService.EntityNotFoundException)
        {
            return Results.NotFound();
        }
    }

    private static async Task<bool> MayEditAsync(UserAuthorizationHelper authHelper,
        ManagementService managementService, DB_Otium otium)
    {
        var currentUser = await authHelper.GetUserAsync();
        var verantwortliche = await managementService.GetVerantwortlicheAsync(otium);

        return await MayEditAsync(currentUser, authHelper, verantwortliche);
    }

    private static async Task<bool> MayEditAsync(Person user,
        UserAuthorizationHelper authHelper, ICollection<Person> verantwortliche)
    {
        if (await authHelper.CurrentUserHasGlobalPermission(GlobalPermission.Otiumsverantwortlich))
            return true;
        return verantwortliche.Any(p => p.Id == user.Id);
    }

    private record StringWrapper(string Value);

    private record IntOrNullWrapper(int? Value);

    private record GuidWrapper(Guid Value);

    private record GuidOrNullWrapper(Guid? Value);

    private record DateOnlyWrapper(DateOnly Value);
}
