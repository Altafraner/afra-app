using Altafraner.AfraApp.Backbone.Authorization;
using Altafraner.AfraApp.Profundum.Domain.DTO;
using Altafraner.AfraApp.Profundum.Services;

using mgmtSvc = Altafraner.AfraApp.Profundum.Services.ProfundumManagementService;
using matSvc = Altafraner.AfraApp.Profundum.Services.ProfundumMatchingService;

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
        ez.MapGet("/", (mgmtSvc svc) => svc.GetEinwahlZeiträumeAsync());
        ez.MapPost("/", async (mgmtSvc svc, DTOProfundumEinwahlZeitraumCreation zeitraum) => (await svc.CreateEinwahlZeitraumAsync(zeitraum)).Id);
        ez.MapPut("/{id:guid}", (ProfundumManagementService managementService, Guid id, DTOProfundumEinwahlZeitraumCreation dto) => managementService.UpdateEinwahlZeitraumAsync(id, dto));
        ez.MapDelete("/{id:guid}", (mgmtSvc svc, Guid id) => svc.DeleteEinwahlZeitraumAsync(id));

        gp.MapGet("/slot", (mgmtSvc svc) => svc.GetSlotsAsync());
        gp.MapPost("/slot", async (mgmtSvc svc, DTOProfundumSlotCreation slot) => (await svc.CreateSlotAsync(slot)).Id);
        gp.MapPut("/slot/{id:guid}", (mgmtSvc svc, Guid id, DTOProfundumSlotCreation dto) => svc.UpdateSlotAsync(id, dto));
        gp.MapDelete("/slot/{id:guid}", (mgmtSvc svc, Guid id) => svc.DeleteSlotAsync(id));

        var kat = gp.MapGroup("kategorie");
        kat.MapGet("/", (mgmtSvc svc) => svc.GetKategorienAsync());
        kat.MapPost("/", async (mgmtSvc svc, DTOProfundumKategorieCreation kategorie) => (await svc.CreateKategorieAsync(kategorie)).Id);
        kat.MapPut("/{id:guid}", (mgmtSvc svc, Guid id, DTOProfundumKategorieCreation kategorie) => svc.UpdateKategorieAsync(id, kategorie));
        kat.MapDelete("/{id:guid}", (mgmtSvc svc, Guid id) => svc.DeleteKategorieAsync(id));

        var pf = gp.MapGroup("profundum");
        pf.MapGet("/{id:guid}", (mgmtSvc svc, Guid id) => svc.GetProfundumAsync(id));
        pf.MapGet("/", (mgmtSvc svc) => svc.GetProfundaAsync());
        pf.MapPost("/", async (mgmtSvc svc, DTOProfundumDefinitionCreation definition) => (await svc.CreateProfundumAsync(definition)).Id);
        pf.MapPut("/{id:guid}", async (mgmtSvc svc, Guid id, DTOProfundumDefinitionCreation definition) => (await svc.UpdateProfundumAsync(id, definition)).Id);
        pf.MapDelete("/{id:guid}", (mgmtSvc svc, Guid id) => svc.DeleteProfundumAsync(id));

        var ins = gp.MapGroup("instanz");
        ins.MapPost("/", async (mgmtSvc svc, DTOProfundumInstanzCreation instanz) => (await svc.CreateInstanzAsync(instanz)).Id);
        ins.MapGet("/", (mgmtSvc svc) => svc.GetInstanzenAsync());
        ins.MapGet("/{id:guid}", (mgmtSvc svc, Guid id) => svc.GetInstanzAsync(id));
        ins.MapPut("/{id:guid}", async (mgmtSvc svc, Guid id, DTOProfundumInstanzCreation instanz) => (await svc.UpdateInstanzAsync(id, instanz)).Id);
        ins.MapDelete("/{id:guid}", (mgmtSvc svc, Guid id) => svc.DeleteInstanzAsync(id));
        ins.MapGet("/{id:guid}.pdf", async (mgmtSvc svc, Guid id) => TypedResults.File((await svc.GetInstanzPdfAsync(id)), "application/pdf", $"{id}.pdf"));

        gp.MapPost("/matching", (matSvc svc) => svc.PerformMatching());
        gp.MapPost("/finalize", (matSvc svc) => svc.Finalize());
        gp.MapGet("/enrollments", (mgmtSvc svc) => svc.GetAllEnrollmentsAsync());
        gp.MapPut("/enrollment/{personId:guid}", (mgmtSvc svc, Guid personId, List<DTOProfundumEnrollment> enrollments) => svc.UpdateEnrollmentsAsync(personId, enrollments));
    }
}
