using System.ComponentModel.DataAnnotations;
using Afra_App.Backbone.Authentication;
using Afra_App.Otium.Domain.Contracts.Services;
using Afra_App.Otium.Domain.DTO;
using Afra_App.Otium.Domain.Models;
using Afra_App.Otium.Services;
using Afra_App.User.Domain.Models;
using Afra_App.User.Services;
using Microsoft.EntityFrameworkCore;
using DB_Otium = Afra_App.Otium.Domain.Models.OtiumDefinition;
using DTO_KlassenLimits = Afra_App.Otium.Domain.DTO.KlassenLimits;
using DTO_Otium_Creation = Afra_App.Otium.Domain.DTO.ManagementOtiumCreation;
using DTO_Termin_Creation = Afra_App.Otium.Domain.DTO.ManagementTerminCreation;
using DTO_Wiederholung_Creation = Afra_App.Otium.Domain.DTO.ManagementWiederholungCreation;
using DTO_Wiederholung_Edit = Afra_App.Otium.Domain.DTO.ManagementWiederholungEdit;

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

        group.MapGet("/supervision/now", GetNowSupervising);

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
        group.MapPatch("/otium/{otiumId:guid}/klassenLimits", OtiumSetKlassenLimits);

        var termin = group.MapGroup("/termin");
        termin.MapGet("/{otiumTerminId:guid}", GetTerminForTeacher);
        termin.MapPost("", CreateOtiumTermin);
        termin.MapDelete("/{otiumTerminId:guid}", DeleteOtiumTermin);
        termin.MapPut("/{otiumTerminId:guid}/cancel", OtiumTerminAbsagen);
        termin.MapDelete("/{otiumTerminId:guid}/cancel", OtiumTerminFortsetzen);
        termin.MapPatch("/{otiumTerminId:guid}/maxEinschreibungen", OtiumTerminSetMaxEinschreibungen);
        termin.MapPatch("/{otiumTerminId:guid}/tutor", OtiumTerminSetTutor);
        termin.MapPatch("/{otiumTerminId:guid}/ort", OtiumTerminSetOrt);
        termin.MapPatch("/{otiumTerminId:guid}/bezeichnung", OtiumTerminSetBezeichnung);
        termin.MapPatch("/{otiumTerminId:guid}/beschreibung", OtiumTerminSetBeschreibung);
        termin.MapPost("/{otiumTerminId:guid}/student", OtiumTerminForceUnenroll);

        group.MapPost("/wiederholung", CreateOtiumWiederholung);
        group.MapDelete("/wiederholung/{otiumWiederholungId:guid}", DeleteOtiumWiederholung);
        group.MapPatch("/wiederholung/{otiumWiederholungId:guid}/discontinue", DiscontinueOtiumWiederholung);
        group.MapPut("/wiederholung/{otiumWiederholungId:guid}", UpdateOtiumWiederholung);
    }

    private static async Task<IResult> GetTerminForTeacher(
        OtiumEndpointService service,
        ManagementService managementService,
        UserAccessor userAccessor,
        UserAuthorizationHelper authHelper,
        HttpContext httpContext,
        Guid otiumTerminId)
    {
        var terminForTeacher = await service.GetTerminForTeacher(otiumTerminId, httpContext.User);
        return terminForTeacher is null ? Results.BadRequest() : Results.Ok(terminForTeacher);
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

    private static async Task<IResult> DiscontinueOtiumWiederholung(ManagementService managementService,
        UserAuthorizationHelper authHelper, OtiumEndpointService service,
        Guid otiumWiederholungId, ValueWrapper<DateOnly> firstDayAfter)
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
            await service.DiscontinueOtiumWiederholungAsync(otiumWiederholungId, firstDayAfter.Value);
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

    private static async Task<IResult> UpdateOtiumWiederholung(ManagementService managementService,
        UserAuthorizationHelper authHelper, OtiumEndpointService service,
        Guid otiumWiederholungId, DTO_Wiederholung_Edit otiumWiederholung)
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
            await service.UpdateOtiumWiederholungAsync(otiumWiederholungId, otiumWiederholung,
                DateOnly.FromDateTime(DateTime.Today));
            return Results.Ok();
        }
        catch (ArgumentException e)
        {
            return Results.Conflict(e.Message);
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
        ValueWrapper<string> value)
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

    private static async Task<IResult> OtiumSetKlassenLimits(ManagementService managementService,
        UserAuthorizationHelper authHelper, OtiumEndpointService service, Guid otiumId,
        DTO_KlassenLimits limits)
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

        var validationContext = new ValidationContext(limits);
        var validationResults = new List<ValidationResult>();
        if (!Validator.TryValidateObject(limits, validationContext, validationResults, true))
        {
            return Results.BadRequest(validationResults.Select(v => v.ErrorMessage));
        }

        try
        {
            await service.OtiumSetKlassenLimitsAsync(otiumId, limits);
            return Results.Ok();
        }
        catch (OtiumEndpointService.EntityNotFoundException)
        {
            return Results.NotFound();
        }
    }

    private static async Task<IResult> OtiumSetBeschreibung(ManagementService managementService,
        UserAuthorizationHelper authHelper, OtiumEndpointService service, Guid otiumId,
        ValueWrapper<string> value)
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
        ValueWrapper<Guid> kategorie)
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
        Guid otiumTerminId, ValueWrapper<int?> maxEinschreibungen)
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
        ValueWrapper<Guid?> personId)
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
        ValueWrapper<string> ort)
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

    private static async Task<IResult> OtiumTerminSetBezeichnung(ManagementService managementService,
        UserAuthorizationHelper authHelper, OtiumEndpointService service, Guid otiumTerminId,
        ValueWrapper<string?> bezeichnung)
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
            await service.OtiumTerminSetOverrideBezeichnungAsync(otiumTerminId, bezeichnung.Value);
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

    private static async Task<IResult> OtiumTerminSetBeschreibung(ManagementService managementService,
        UserAuthorizationHelper authHelper, OtiumEndpointService service, Guid otiumTerminId,
        ValueWrapper<string?> beschreibung)
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
            await service.OtiumTerminSetOverrideBeschreibungAsync(otiumTerminId, beschreibung.Value);
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

    private static async Task<IResult> OtiumTerminForceUnenroll(Guid otiumTerminId, ValueWrapper<Guid> personIdWrapper,
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

    private static async Task<IEnumerable<BlockInfo>> GetNowSupervising(AfraAppContext dbContext,
        BlockHelper blockHelper, IAttendanceService attendanceService, HttpContext context)
    {
        var today = DateOnly.FromDateTime(DateTime.Now);
        var blocksToday = await dbContext.Blocks
            .AsNoTracking()
            .Where(b => b.SchultagKey == today)
            .OrderBy(b => b.SchemaId)
            .ToListAsync();

        return blocksToday.Where(b => attendanceService.MaySupervise(context.User, b))
            .Select(b =>
            {
                var schema = blockHelper.Get(b.SchemaId)!;
                return new BlockInfo
                {
                    Id = b.Id,
                    SchemaId = b.SchemaId,
                    Name = schema.Bezeichnung,
                    Uhrzeit = schema.Interval,
                    Datum = b.SchultagKey
                };
            });
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

    private record ValueWrapper<T>(T Value);
}
