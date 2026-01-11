using System.Net.Mime;
using System.Text;
using Altafraner.AfraApp.Backbone.Authorization;
using Altafraner.AfraApp.Otium.API.Endpoints;
using Altafraner.AfraApp.Profundum.Domain.DTO;
using Altafraner.AfraApp.Profundum.Domain.Models;
using Altafraner.AfraApp.Profundum.Services;
using Match = Altafraner.AfraApp.Profundum.Services.ProfundumMatchingService;
using Mgmt = Altafraner.AfraApp.Profundum.Services.ProfundumManagementService;
using Altafraner.AfraApp.User.Domain.Models;
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
        ins.MapGet("/{id:guid}.pdf", async (Mgmt svc, Guid id) => TypedResults.File((await svc.GetInstanzPdfAsync(id)), MediaTypeNames.Application.Pdf, $"{id}.pdf"));

        var fachbereich = gp.MapGroup("fachbereich");
        fachbereich.MapGet("/",
            async (ProfundumFachbereicheService fbs) =>
                TypedResults.Ok((await fbs.GetFachbereicheAsync()).Select(fb => new DtoProfundumFachbereich(fb))));
        fachbereich.MapPost("/",
            async (ProfundumFachbereicheService fbs, ValueWrapper<string> request) =>
            {
                await fbs.CreateFachbereichAsync(request.Value);
                return TypedResults.NoContent();
            });
        fachbereich.MapPut("/",
            async (ProfundumFachbereicheService fbs, DtoProfundumFachbereich request) =>
            {
                await fbs.UpdateFachbereichAsync(request.Id, request.Label);
                return TypedResults.NoContent();
            });
        fachbereich.MapDelete("/{id:guid}",
            async (ProfundumFachbereicheService fbs, Guid id) =>
            {
                await fbs.DeleteFachbereichAsync(id);
                return TypedResults.NoContent();
            });

        gp.MapPost("/matching", (Match svc) => svc.PerformMatching());
        gp.MapPost("/finalize", (Match svc) => svc.Finalize());
        gp.MapGet("/enrollments", (Match svc) => svc.GetAllEnrollmentsAsync());
        gp.MapPut("/enrollment/{personId:guid}", (Mgmt svc, Guid personId, List<DTOProfundumEnrollment> enrollments) => svc.UpdateEnrollmentsAsync(personId, enrollments));

        gp.MapGet("/matching.csv", async (Mgmt svc) => TypedResults.File(Encoding.UTF8.GetBytes(await svc.GetStudentMatchingCsv()), MediaTypeNames.Text.Csv));

        gp.MapGet("/feedback/belegung", GetAllQuartaleWithEnrollments)
            .RequireAuthorization(AuthorizationPolicies.TutorOnly);
    }

    // TODO This is slow and should be replaced by something more in line with the new matching interface.
    private static async Task<Ok<QuartalEnrollmentOverview[]>> GetAllQuartaleWithEnrollments(
        AfraAppContext dbContext,
        UserAccessor userAccessor)
    {
        var user = await userAccessor.GetUserAsync();
        IQueryable<ProfundumInstanz> profundaQuery = dbContext.ProfundaInstanzen
            .AsSplitQuery()
            .Include(e => e.Profundum)
            .Include(e => e.Einschreibungen.Where(enr => enr.IsFixed))
            .ThenInclude(e => e.BetroffenePerson)
            .Include(e => e.Slots);

        if (!user.GlobalPermissions.Contains(GlobalPermission.Profundumsverantwortlich))
            profundaQuery = profundaQuery.Where(p => p.Verantwortliche.Contains(user));

        var profunda = await profundaQuery
            .OrderBy(e => e.Profundum.Bezeichnung)
            .AsAsyncEnumerable()
            .SelectMany(e => e.Slots.Select(s => (slot: s, instanz: e)))
            .GroupBy(e => e.slot, a => a.instanz)
            .OrderByDescending(e => e.Key.Jahr)
            .ThenByDescending(e => e.Key.Quartal)
            .ThenBy(e => e.Key.Wochentag)
            .Select(e => new QuartalEnrollmentOverview(e.Key, e))
            .ToArrayAsync();

        return TypedResults.Ok(profunda);
    }
}
