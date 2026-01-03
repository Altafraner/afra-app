using Altafraner.AfraApp.Backbone.Authorization;
using Altafraner.AfraApp.Profundum.Domain.DTO;
using Altafraner.AfraApp.Profundum.Services;
using Altafraner.AfraApp.User.Services;
using Microsoft.AspNetCore.Http.HttpResults;

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

        group.MapGet("/einwahlzeitraum", async (ProfundumManagementService svc) => TypedResults.Ok(await svc.GetEinwahlZeiträumeAsync()));
        group.MapPost("/einwahlzeitraum", async (ProfundumManagementService svc, DTOProfundumEinwahlZeitraumCreation zeitraum) => TypedResults.Ok((await svc.CreateEinwahlZeitraumAsync(zeitraum)).Id));
        group.MapPut("/einwahlzeitraum/{id:guid}", UpdateEinwahlZeitraumAsync);
        group.MapDelete("/einwahlzeitraum/{id:guid}", (ProfundumManagementService svc, Guid id) => svc.DeleteEinwahlZeitraumAsync(id));

        group.MapGet("/slot", async (ProfundumManagementService svc) => TypedResults.Ok(await svc.GetSlotsAsync()));
        group.MapPost("/slot", AddSlotAsync);
        group.MapPut("/slot/{id:guid}", UpdateSlotAsync);
        group.MapDelete("/slot/{id:guid}", (ProfundumManagementService svc, Guid id) => svc.DeleteSlotAsync(id));

        group.MapGet("/kategorie", (ProfundumManagementService svc) => svc.GetKategorienAsync());
        group.MapPost("/kategorie", AddKategorieAsync);
        group.MapPut("/kategorie/{kategorieId:guid}", UpdateKategorieAsync);
        group.MapDelete("/kategorie/{kategorieId:guid}", (ProfundumManagementService svc, Guid kategorieId) => svc.DeleteKategorieAsync(kategorieId));

        group.MapGet("/profundum/{profundumId:guid}", (ProfundumManagementService svc, Guid profundumId) => svc.GetProfundumAsync(profundumId));
        group.MapGet("/profundum", async (ProfundumManagementService svc) => TypedResults.Ok(await svc.GetProfundaAsync()));
        group.MapPost("/profundum", AddProfundumAsync);
        group.MapPut("/profundum/{profundumId:guid}", UpdateProfundumAsync);
        group.MapDelete("/profundum/{profundumId:guid}", (ProfundumManagementService svc, Guid profundumId) => svc.DeleteProfundumAsync(profundumId));
        group.MapPost("/instanz", AddInstanzAsync);
        group.MapGet("/instanz", async (ProfundumManagementService svc) => TypedResults.Ok(await svc.GetInstanzenAsync()));
        group.MapGet("/instanz/{instanzId:guid}", (ProfundumManagementService svc, Guid instanzId) => svc.GetInstanzAsync(instanzId));
        group.MapPut("/instanz/{instanzId:guid}", UpdateInstanzAsync);
        group.MapDelete("/instanz/{instanzId:guid}", (ProfundumManagementService svc, Guid instanzId) => svc.DeleteInstanzAsync(instanzId));

        group.MapGet("/instanz/{instanzId:guid}.pdf", GetInstanzPdfAsync);

        group.MapPost("/matching", (ProfundumMatchingService svc) => svc.PerformMatching());
        group.MapPost("/finalize", (ProfundumMatchingService svc) => svc.Finalize());
        group.MapGet("/enrollments", (ProfundumManagementService svc) => svc.GetAllEnrollmentsAsync());
        group.MapPut("/enrollment/{personId:guid}", (ProfundumManagementService svc, Guid personId, List<DTOProfundumEnrollment> enrollments) => svc.UpdateEnrollmentsAsync(personId, enrollments));
    }

    private static async Task<Results<Ok, NotFound>> UpdateEinwahlZeitraumAsync(
        ProfundumManagementService managementService,
        Guid id,
        DTOProfundumEinwahlZeitraumCreation dto)
    {
        var ok = await managementService.UpdateEinwahlZeitraumAsync(id, dto);
        return ok ? TypedResults.Ok() : TypedResults.NotFound();
    }

    private static async Task<Results<Ok, NotFound>> UpdateSlotAsync(
        ProfundumManagementService managementService,
        Guid id,
        DTOProfundumSlotCreation dto)
    {
        var ok = await managementService.UpdateSlotAsync(id, dto);
        return ok ? TypedResults.Ok() : TypedResults.NotFound();
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


    private static async Task<Results<FileContentHttpResult, NotFound>> GetInstanzPdfAsync(
        ProfundumManagementService svc,
        Guid instanzId)
    {
        var instanz = await svc.GetInstanzAsync(instanzId);
        if (instanz is null)
            return TypedResults.NotFound();
        var profundum = await svc.GetProfundumAsync(instanz.ProfundumId);
        if (profundum is null)
            return TypedResults.NotFound();
        var firstSlot = (await svc.GetSlotsAsync()).First(s => s.Id == instanz.Slots.First());
        return TypedResults.File((await svc.GetInstanzPdfAsync(instanzId))!,
            "application/pdf",
            $"{firstSlot.Jahr}_{firstSlot.Quartal}_{firstSlot.Wochentag}_{profundum.Bezeichnung}.pdf");
    }

    private static async Task<Results<NotFound, Ok<Guid>>> UpdateInstanzAsync(ProfundumManagementService svc,
        Guid instanzId,
        DTOProfundumInstanzCreation instanz)
    {
        var res = await svc.UpdateInstanzAsync(instanzId, instanz);
        return res is null ? TypedResults.NotFound() : TypedResults.Ok(res.Id);
    }
}
