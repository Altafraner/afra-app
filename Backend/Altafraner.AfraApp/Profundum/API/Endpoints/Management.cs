using Altafraner.AfraApp.Backbone.Authorization;
using Altafraner.AfraApp.Profundum.Domain.DTO;
using Altafraner.AfraApp.Profundum.Services;
using Match = Altafraner.AfraApp.Profundum.Services.ProfundumMatchingService;
using Mgmt = Altafraner.AfraApp.Profundum.Services.ProfundumManagementService;

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
        var gp = app.MapGroup("/management")
            .RequireAuthorization(AuthorizationPolicies.ProfundumsVerantwortlich);

        var ez = gp.MapGroup("einwahlzeitraum");
        ez.MapGet("/", (Mgmt svc) => svc.GetEinwahlZeiträumeAsync());
        ez.MapPost("/", async (Mgmt svc, DTOProfundumEinwahlZeitraumCreation zeitraum) => (await svc.CreateEinwahlZeitraumAsync(zeitraum)).Id);
        ez.MapPut("/{id:guid}", (ProfundumManagementService managementService, Guid id, DTOProfundumEinwahlZeitraumCreation dto) => managementService.UpdateEinwahlZeitraumAsync(id, dto));
        ez.MapDelete("/{id:guid}", (Mgmt svc, Guid id) => svc.DeleteEinwahlZeitraumAsync(id));

        gp.MapGet("/slot", (Mgmt svc) => svc.GetSlotsAsync());
        gp.MapPost("/slot", async (Mgmt svc, DTOProfundumSlotCreation slot) => (await svc.CreateSlotAsync(slot)).Id);
        gp.MapPut("/slot/{id:guid}", (Mgmt svc, Guid id, DTOProfundumSlotCreation dto) => svc.UpdateSlotAsync(id, dto));
        gp.MapDelete("/slot/{id:guid}", (Mgmt svc, Guid id) => svc.DeleteSlotAsync(id));

        var kat = gp.MapGroup("kategorie");
        kat.MapGet("/", (Mgmt svc) => svc.GetKategorienAsync());
        kat.MapPost("/", async (Mgmt svc, DTOProfundumKategorieCreation kategorie) => (await svc.CreateKategorieAsync(kategorie)).Id);
        kat.MapPut("/{id:guid}", (Mgmt svc, Guid id, DTOProfundumKategorieCreation kategorie) => svc.UpdateKategorieAsync(id, kategorie));
        kat.MapDelete("/{id:guid}", (Mgmt svc, Guid id) => svc.DeleteKategorieAsync(id));

        var pf = gp.MapGroup("profundum");
        pf.MapGet("/{id:guid}", (Mgmt svc, Guid id) => svc.GetProfundumAsync(id));
        pf.MapGet("/", (Mgmt svc) => svc.GetProfundaAsync());
        pf.MapPost("/", async (Mgmt svc, DTOProfundumDefinitionCreation definition) => (await svc.CreateProfundumAsync(definition)).Id);
        pf.MapPut("/{id:guid}", async (Mgmt svc, Guid id, DTOProfundumDefinitionCreation definition) => (await svc.UpdateProfundumAsync(id, definition)).Id);
        pf.MapDelete("/{id:guid}", (Mgmt svc, Guid id) => svc.DeleteProfundumAsync(id));

        var ins = gp.MapGroup("instanz");
        ins.MapPost("/", async (Mgmt svc, DTOProfundumInstanzCreation instanz) => (await svc.CreateInstanzAsync(instanz)).Id);
        ins.MapGet("/", (Mgmt svc) => svc.GetInstanzenAsync());
        ins.MapGet("/{id:guid}", (Mgmt svc, Guid id) => svc.GetInstanzAsync(id));
        ins.MapPut("/{id:guid}", async (Mgmt svc, Guid id, DTOProfundumInstanzCreation instanz) => (await svc.UpdateInstanzAsync(id, instanz)).Id);
        ins.MapDelete("/{id:guid}", (Mgmt svc, Guid id) => svc.DeleteInstanzAsync(id));
        ins.MapGet("/{id:guid}.pdf", async (Mgmt svc, Guid id) => TypedResults.File((await svc.GetInstanzPdfAsync(id)), "application/pdf", $"{id}.pdf"));

        gp.MapPost("/matching", (Match svc) => svc.PerformMatching());
        gp.MapPost("/finalize", (Match svc) => svc.Finalize());
        gp.MapGet("/enrollments", (Mgmt svc) => svc.GetAllEnrollmentsAsync());
        gp.MapPut("/enrollment/{personId:guid}", (Mgmt svc, Guid personId, List<DTOProfundumEnrollment> enrollments) => svc.UpdateEnrollmentsAsync(personId, enrollments));
    }
}
