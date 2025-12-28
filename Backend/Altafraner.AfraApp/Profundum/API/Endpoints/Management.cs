using Altafraner.AfraApp.Backbone.Authorization;
using Altafraner.AfraApp.Profundum.Domain.DTO;
using Altafraner.AfraApp.Profundum.Domain.Models;
using Altafraner.AfraApp.Profundum.Services;
using Altafraner.AfraApp.User.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace Altafraner.AfraApp.Profundum.API.Endpoints;

/// <summary>
///     Contains endpoints for managing profunda
/// </summary>
public static class Management
{
    /// <summary>
    ///     Maps the Profunda Management endpoints to the given <see cref="IEndpointRouteBuilder" />.
    /// </summary>
    public static void MapManagementEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/management")
            .RequireAuthorization(AuthorizationPolicies.ProfundumsVerantwortlich);

        group.MapGet("/einwahlzeitraum", GetEinwahlZeitraeumeAsync);
        group.MapPost("/einwahlzeitraum", AddEinwahlZeitraumAsync);
        group.MapPut("/einwahlzeitraum/{id:guid}", UpdateEinwahlZeitraumAsync);
        group.MapDelete("/einwahlzeitraum/{id:guid}", DeleteEinwahlZeitraumAsync);

        group.MapGet("/slot", GetSlotsAsync);
        group.MapPost("/slot", AddSlotAsync);
        group.MapPut("/slot/{id:guid}", UpdateSlotAsync);
        group.MapDelete("/slot/{id:guid}", DeleteSlotAsync);

        group.MapGet("/kategorie", GetKategorienAsync);
        group.MapPost("/kategorie", AddKategorieAsync);
        group.MapPut("/kategorie/{kategorieId:guid}", UpdateKategorieAsync);
        group.MapDelete("/kategorie/{kategorieId:guid}", DeleteKategorieAsync);

        group.MapGet("/profundum/{profundumId:guid}", GetProfundumAsync);
        group.MapGet("/profundum", GetProfundaAsync);
        group.MapPost("/profundum", AddProfundumAsync);
        group.MapPut("/profundum/{profundumId:guid}", UpdateProfundumAsync);
        group.MapDelete("/profundum/{profundumId:guid}", DeleteProfundumAsync);

        group.MapPost("/instanz", AddInstanzAsync);
        group.MapGet("/instanz", GetInstanzenAsync);
        group.MapGet("/instanz/{instanzId:guid}", GetInstanzAsync);
        group.MapGet("/instanz/{instanzId:guid}.pdf", GetInstanzPdfAsync);
        group.MapPut("/instanz/{instanzId:guid}", UpdateInstanzAsync);
        group.MapDelete("/instanz/{instanzId:guid}", DeleteInstanzAsync);


        group.MapPost("/matching", DoMatchingAsync);
        group.MapGet("/enrollments", GetAllEnrollmentsAsync);
    }

    private static async Task<Ok<Guid>> AddEinwahlZeitraumAsync(ProfundumManagementService managementService,
        UserAccessor userAccessor,
        AfraAppContext dbContext,
        ILogger<ProfundumEnrollmentService> logger,
        DTOProfundumEinwahlZeitraumCreation zeitraum)
    {
        var res = await managementService.CreateEinwahlZeitraumAsync(zeitraum);
        return TypedResults.Ok(res.Id);
    }

    private static async Task<Ok<DTOProfundumEinwahlZeitraum[]>> GetEinwahlZeitraeumeAsync(
        ProfundumManagementService managementService,
        UserAccessor userAccessor,
        AfraAppContext dbContext,
        ILogger<ProfundumEnrollmentService> logger)
    {
        return TypedResults.Ok(await managementService.GetEinwahlZeiträumeAsync());
    }


    private static async Task<Results<Ok, NotFound>> UpdateEinwahlZeitraumAsync(
        ProfundumManagementService managementService,
        Guid id,
        DTOProfundumEinwahlZeitraumCreation dto)
    {
        var ok = await managementService.UpdateEinwahlZeitraumAsync(id, dto);
        return ok ? TypedResults.Ok() : TypedResults.NotFound();
    }

    private static async Task<Ok> DeleteEinwahlZeitraumAsync(
        ProfundumManagementService managementService,
        Guid id)
    {
        await managementService.DeleteEinwahlZeitraumAsync(id);
        return TypedResults.Ok();
    }

    private static async Task<Results<Ok, NotFound>> UpdateSlotAsync(
        ProfundumManagementService managementService,
        Guid id,
        DTOProfundumSlotCreation dto)
    {
        var ok = await managementService.UpdateSlotAsync(id, dto);
        return ok ? TypedResults.Ok() : TypedResults.NotFound();
    }

    private static async Task<Ok> DeleteSlotAsync(
        ProfundumManagementService managementService,
        Guid id)
    {
        await managementService.DeleteSlotAsync(id);
        return TypedResults.Ok();
    }

    private static async Task<Ok<DTOProfundumSlot[]>> GetSlotsAsync(ProfundumManagementService managementService,
        UserAccessor userAccessor,
        AfraAppContext dbContext,
        ILogger<ProfundumEnrollmentService> logger)
    {
        return TypedResults.Ok(await managementService.GetSlotsAsync());
    }

    private static async Task<Results<Ok<Guid>, BadRequest<string>>> AddSlotAsync(
        ProfundumManagementService managementService,
        UserAccessor userAccessor,
        AfraAppContext dbContext,
        ILogger<ProfundumEnrollmentService> logger,
       DTOProfundumSlotCreation slot)
    {
        var res = await managementService.CreateSlotAsync(slot);
        if (res is null) return TypedResults.BadRequest("Could not create slot");

        return TypedResults.Ok(res.Id);
    }

    private static async Task<Ok<DTOProfundumKategorie[]>> GetKategorienAsync(
        ProfundumManagementService managementService,
        UserAccessor userAccessor,
        AfraAppContext dbContext,
        ILogger<ProfundumEnrollmentService> logger)
    {
        return TypedResults.Ok(await managementService.GetKategorienAsync());
    }

    private static async Task<Results<Ok<Guid>, BadRequest<string>>> AddKategorieAsync(
        ProfundumManagementService managementService,
        UserAccessor userAccessor,
        AfraAppContext dbContext,
        ILogger<ProfundumEnrollmentService> logger,
        DTOProfundumKategorieCreation kategorie)
    {
        var res = await managementService.CreateKategorieAsync(kategorie);
        if (res is null) return TypedResults.BadRequest("Could not create kategorie");

        return TypedResults.Ok(res.Id);
    }

    private static async Task<Ok> UpdateKategorieAsync(ProfundumManagementService managementService,
        UserAccessor userAccessor,
        AfraAppContext dbContext,
        ILogger<ProfundumEnrollmentService> logger,
        Guid kategorieId,
        DTOProfundumKategorieCreation kategorie)
    {
        await managementService.UpdateKategorieAsync(kategorieId, kategorie);
        return TypedResults.Ok();
    }

    private static async Task<Ok> DeleteKategorieAsync(ProfundumManagementService managementService,
        UserAccessor userAccessor,
        AfraAppContext dbContext,
        ILogger<ProfundumEnrollmentService> logger,
        Guid kategorieId)
    {
        await managementService.DeleteKategorieAsync(kategorieId);
        return TypedResults.Ok();
    }

    private static async Task<Results<Ok<Guid>, BadRequest<string>>> AddProfundumAsync(
        ProfundumManagementService managementService,
        UserAccessor userAccessor,
        AfraAppContext dbContext,
        ILogger<ProfundumEnrollmentService> logger,
        DTOProfundumDefinitionCreation definition)
    {
        var res = await managementService.CreateProfundumAsync(definition);
        if (res is null) return TypedResults.BadRequest("Could not create profundum");

        return TypedResults.Ok(res.Id);
    }

    private static async Task<Results<Ok<Guid>, BadRequest<string>>> UpdateProfundumAsync(
        ProfundumManagementService managementService,
        UserAccessor userAccessor,
        AfraAppContext dbContext,
        ILogger<ProfundumEnrollmentService> logger,
        Guid profundumId,
        DTOProfundumDefinitionCreation definition)
    {
        var res = await managementService.UpdateProfundumAsync(profundumId, definition);
        if (res is null) return TypedResults.BadRequest("Could not update profundum");
        return TypedResults.Ok(res.Id);
    }

    private static async Task<Ok> DeleteProfundumAsync(ProfundumManagementService managementService,
        UserAccessor userAccessor,
        AfraAppContext dbContext,
        ILogger<ProfundumEnrollmentService> logger,
        Guid profundumId)
    {
        await managementService.DeleteProfundumAsync(profundumId);
        return TypedResults.Ok();
    }

    private static async Task<Ok<DTOProfundumDefinition>> GetProfundumAsync(
        ProfundumManagementService managementService,
        UserAccessor userAccessor,
        AfraAppContext dbContext,
        ILogger<ProfundumEnrollmentService> logger,
        Guid profundumId)
    {
        return TypedResults.Ok(await managementService.GetProfundumAsync(profundumId));
    }

    private static async Task<Ok<DTOProfundumDefinition[]>> GetProfundaAsync(
        ProfundumManagementService managementService,
        UserAccessor userAccessor,
        AfraAppContext dbContext,
        ILogger<ProfundumEnrollmentService> logger)
    {
        return TypedResults.Ok(await managementService.GetProfundaAsync());
    }

    private static async Task<Results<Ok<Guid>, BadRequest<string>>> AddInstanzAsync(
        ProfundumManagementService managementService,
        UserAccessor userAccessor,
        AfraAppContext dbContext,
        ILogger<ProfundumEnrollmentService> logger,
        DTOProfundumInstanzCreation instanz)
    {
        var res = await managementService.CreateInstanzAsync(instanz);
        if (res is null) return TypedResults.BadRequest("Could not create instanz");

        return TypedResults.Ok(res.Id);
    }

    private static async Task<Ok<DTOProfundumInstanz[]>> GetInstanzenAsync(ProfundumManagementService svc)
    {
        return TypedResults.Ok(await svc.GetInstanzenAsync());
    }

    private static async Task<Ok<DTOProfundumInstanz>> GetInstanzAsync(ProfundumManagementService svc, Guid instanzId)
    {
        return TypedResults.Ok(await svc.GetInstanzAsync(instanzId));
    }

    private static async Task<FileContentHttpResult> GetInstanzPdfAsync(ProfundumManagementService svc, Guid instanzId)
    {
        return TypedResults.File((await svc.GetInstanzPdfAsync(instanzId))!, "application/pdf");
    }

    private static async Task<Results<NotFound, Ok<Guid>>> UpdateInstanzAsync(ProfundumManagementService svc,
        Guid instanzId,
        DTOProfundumInstanzCreation instanz)
    {
        var res = await svc.UpdateInstanzAsync(instanzId, instanz);
        return res is null ? TypedResults.NotFound() : TypedResults.Ok(res.Id);
    }

    private static async Task<Ok> DeleteInstanzAsync(ProfundumManagementService svc, Guid instanzId)
    {
        await svc.DeleteInstanzAsync(instanzId);
        return TypedResults.Ok();
    }


    ///
    private static async Task<Results<Ok<MatchingStats>, NotFound<string>>> DoMatchingAsync(
        ProfundumEnrollmentService enrollmentService,
        UserAccessor userAccessor,
        AfraAppContext dbContext,
        ILogger<ProfundumEnrollmentService> logger)
    {
        var einwahlZeitraum = (await dbContext.ProfundumEinwahlZeitraeume
                .Include(ez => ez.Slots)
                .ToArrayAsync())
            .FirstOrDefault((ProfundumEinwahlZeitraum?)null);
        if (einwahlZeitraum is null) return TypedResults.NotFound("Kein offener Einwahlzeitraum");

        var result = await enrollmentService.PerformMatching(einwahlZeitraum, true);
        return TypedResults.Ok(result);
    }

    ///
    private static async Task<Ok<Dictionary<Guid, DTOProfundumEnrollment[]>>> GetAllEnrollmentsAsync(
        ProfundumManagementService managementService,
        UserAccessor userAccessor,
        AfraAppContext dbContext,
        ILogger<ProfundumManagementService> logger)
    {
        var result = await managementService.GetAllEnrollmentsAsync();
        return TypedResults.Ok(result);
    }
}
