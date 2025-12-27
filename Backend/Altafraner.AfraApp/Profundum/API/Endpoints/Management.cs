using Altafraner.AfraApp.Backbone.Authorization;
using Altafraner.AfraApp.Profundum.Domain.DTO;
using Altafraner.AfraApp.Profundum.Domain.Models;
using Altafraner.AfraApp.Profundum.Services;
using Altafraner.AfraApp.User.Services;
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

        group.MapGet("/einwahlzeitraum", GetEinwahlZeiträumeAsync);
        group.MapPost("/einwahlzeitraum", AddEinwahlZeitraumAsync);

        group.MapGet("/slot", GetSlotsAsync);
        group.MapPost("/slot", AddSlotAsync);

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

    private static async Task<IResult> AddEinwahlZeitraumAsync(ProfundumManagementService managementService,
        UserAccessor userAccessor, AfraAppContext dbContext, ILogger<ProfundumEnrollmentService> logger,
        DTOProfundumEinwahlZeitraum zeitraum)
    {
        var res = await managementService.CreateEinwahlZeitraumAsync(zeitraum);
        return Results.Ok(res.Id);
    }

    private static async Task<IResult> GetEinwahlZeiträumeAsync(ProfundumManagementService managementService,
           UserAccessor userAccessor, AfraAppContext dbContext, ILogger<ProfundumEnrollmentService> logger)
    {
        return Results.Ok(await managementService.GetEinwahlZeiträumeAsync());
    }

    private static async Task<IResult> GetSlotsAsync(ProfundumManagementService managementService,
           UserAccessor userAccessor, AfraAppContext dbContext, ILogger<ProfundumEnrollmentService> logger)
    {
        return Results.Ok(await managementService.GetSlotsAsync());
    }

    private static async Task<IResult> AddSlotAsync(ProfundumManagementService managementService,
        UserAccessor userAccessor, AfraAppContext dbContext, ILogger<ProfundumEnrollmentService> logger,
        DTOProfundumSlot slot)
    {
        var res = await managementService.CreateSlotAsync(slot);
        if (res is null)
        {
            return Results.BadRequest("Could not create slot");
        }

        return Results.Ok(res.Id);
    }

    private static async Task<IResult> GetKategorienAsync(ProfundumManagementService managementService,
           UserAccessor userAccessor, AfraAppContext dbContext, ILogger<ProfundumEnrollmentService> logger)
    {
        return Results.Ok(await managementService.GetKategorienAsync());
    }

    private static async Task<IResult> AddKategorieAsync(ProfundumManagementService managementService,
           UserAccessor userAccessor, AfraAppContext dbContext, ILogger<ProfundumEnrollmentService> logger, DTOProfundumKategorieCreation kategorie)
    {
        var res = await managementService.CreateKategorieAsync(kategorie);
        if (res is null)
        {
            return Results.BadRequest("Could not create kategorie");
        }

        return Results.Ok(res.Id);
    }

    private static async Task<IResult> UpdateKategorieAsync(ProfundumManagementService managementService,
           UserAccessor userAccessor, AfraAppContext dbContext, ILogger<ProfundumEnrollmentService> logger,
           Guid kategorieId, DTOProfundumKategorieCreation kategorie)
    {
        await managementService.UpdateKategorieAsync(kategorieId, kategorie);
        return Results.Ok();
    }

    private static async Task<IResult> DeleteKategorieAsync(ProfundumManagementService managementService,
           UserAccessor userAccessor, AfraAppContext dbContext, ILogger<ProfundumEnrollmentService> logger,
           Guid kategorieId)
    {
        await managementService.DeleteKategorieAsync(kategorieId);
        return Results.Ok();
    }

    private static async Task<IResult> AddProfundumAsync(ProfundumManagementService managementService,
           UserAccessor userAccessor, AfraAppContext dbContext, ILogger<ProfundumEnrollmentService> logger,
           DTOProfundumDefinitionCreation definition)
    {
        var res = await managementService.CreateProfundumAsync(definition);
        if (res is null)
        {
            return Results.BadRequest("Could not create profundum");
        }

        return Results.Ok(res.Id);
    }

    private static async Task<IResult> UpdateProfundumAsync(ProfundumManagementService managementService,
           UserAccessor userAccessor, AfraAppContext dbContext, ILogger<ProfundumEnrollmentService> logger,
          Guid profundumId, DTOProfundumDefinitionCreation definition)
    {
        var res = await managementService.UpdateProfundumAsync(profundumId, definition);
        if (res is null)
        {
            return Results.BadRequest("Could not update profundum");
        }
        return Results.Ok(res.Id);
    }

    private static async Task<IResult> DeleteProfundumAsync(ProfundumManagementService managementService,
           UserAccessor userAccessor, AfraAppContext dbContext, ILogger<ProfundumEnrollmentService> logger,
          Guid profundumId)
    {
        await managementService.DeleteProfundumAsync(profundumId);
        return Results.Ok();
    }

    private static async Task<IResult> GetProfundumAsync(ProfundumManagementService managementService,
           UserAccessor userAccessor, AfraAppContext dbContext, ILogger<ProfundumEnrollmentService> logger, Guid profundumId)
    {
        Console.WriteLine($"check for id {profundumId}");
        return Results.Ok(await managementService.GetProfundumAsync(profundumId));
    }

    private static async Task<IResult> GetProfundaAsync(ProfundumManagementService managementService,
           UserAccessor userAccessor, AfraAppContext dbContext, ILogger<ProfundumEnrollmentService> logger)
    {
        return Results.Ok(await managementService.GetProfundaAsync());
    }

    private static async Task<IResult> AddInstanzAsync(ProfundumManagementService managementService,
        UserAccessor userAccessor, AfraAppContext dbContext, ILogger<ProfundumEnrollmentService> logger,
        DTOProfundumInstanzCreation instanz)
    {
        var res = await managementService.CreateInstanzAsync(instanz);
        if (res is null)
        {
            return Results.BadRequest("Could not create instanz");
        }

        return Results.Ok(res.Id);
    }
    private static async Task<IResult> GetInstanzenAsync(ProfundumManagementService svc) =>
        Results.Ok(await svc.GetInstanzenAsync());

    private static async Task<IResult> GetInstanzAsync(ProfundumManagementService svc, Guid instanzId) =>
        Results.Ok(await svc.GetInstanzAsync(instanzId));

    private static async Task<IResult> GetInstanzPdfAsync(ProfundumManagementService svc, Guid instanzId) =>
        Results.File(await svc.GetInstanzPdfAsync(instanzId), "application/pdf");

    private static async Task<IResult> UpdateInstanzAsync(ProfundumManagementService svc, Guid instanzId, DTOProfundumInstanzCreation instanz)
    {
        var res = await svc.UpdateInstanzAsync(instanzId, instanz);
        return res is null ? Results.NotFound() : Results.Ok(res.Id);
    }

    private static async Task<IResult> DeleteInstanzAsync(ProfundumManagementService svc, Guid instanzId)
    {
        await svc.DeleteInstanzAsync(instanzId);
        return Results.Ok();
    }


    ///
    private static async Task<IResult> DoMatchingAsync(ProfundumEnrollmentService enrollmentService,
        UserAccessor userAccessor, AfraAppContext dbContext, ILogger<ProfundumEnrollmentService> logger)
    {
        var now = DateTime.UtcNow;
        var einwahlZeitraum = (await dbContext.ProfundumEinwahlZeitraeume
            .Include(ez => ez.Slots)
            .ToArrayAsync())
            .FirstOrDefault((ProfundumEinwahlZeitraum?)null);
        if (einwahlZeitraum is null)
        {
            return Results.NotFound("Kein offener Einwahlzeitraum");
        }

        var result = await enrollmentService.PerformMatching(einwahlZeitraum, true);
        return Results.Ok(result);
    }

    ///
    private static async Task<IResult> GetAllEnrollmentsAsync(ProfundumManagementService managementService,
           UserAccessor userAccessor, AfraAppContext dbContext, ILogger<ProfundumManagementService> logger)
    {
        var result = await managementService.GetAllEnrollmentsAsync();
        return Results.Ok(result);
    }
}
