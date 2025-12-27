using System.Text;
using Altafraner.AfraApp.Backbone.Authorization;
using Altafraner.AfraApp.Profundum.Domain.DTO;
using Altafraner.AfraApp.Profundum.Domain.Models;
using Altafraner.AfraApp.Profundum.Services;
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
        var group = app.MapGroup("/management")
            .RequireAuthorization(AuthorizationPolicies.AdminOnly);

        group.MapPut("/einwahlzeitraum", AddEinwahlZeitraumAsync);
        group.MapPut("/slot", AddSlotAsync);
        group.MapPut("/kategorie", AddKategorieAsync);
        group.MapPut("/profundum", AddProfundumAsync);
        group.MapPut("/instanz", AddInstanzAsync);

        group.MapGet("/missing", GetUnenrolledAsync);
        group.MapGet("/missing/emails", GetUnenrolledEmailsAsync);
        group.MapPost("/matching", DoMatchingAsync);
        group.MapPost("/matching/final", DoFinalMatchingAsync);
        group.MapGet("/matching.csv", MatchingAsyncCsv);

        group.MapGet("/kategorien", GetKategorien);
        app.MapGet("/management/belegung", GetAllQuartaleWithEnrollments)
            .RequireAuthorization(AuthorizationPolicies.TutorOnly);
    }

    ///
    private static async Task<IResult> AddEinwahlZeitraumAsync(ProfundumManagementService managementService,
        UserAccessor userAccessor,
        AfraAppContext dbContext,
        ILogger<ProfundumEnrollmentService> logger,
        DtoProfundumEinwahlZeitraum zeitraum)
    {
        var res = await managementService.CreateEinwahlZeitraumAsync(zeitraum);
        return Results.Ok(res.Id);
    }

    ///
    private static async Task<IResult> AddSlotAsync(ProfundumManagementService managementService,
        UserAccessor userAccessor,
        AfraAppContext dbContext,
        ILogger<ProfundumEnrollmentService> logger,
        DtoProfundumSlot slot)
    {
        var res = await managementService.CreateSlotAsync(slot);
        if (res is null)
        {
            return Results.BadRequest("Could not create slot");
        }

        return Results.Ok(res.Id);
    }

    ///
    private static async Task<IResult> AddKategorieAsync(ProfundumManagementService managementService,
        UserAccessor userAccessor,
        AfraAppContext dbContext,
        ILogger<ProfundumEnrollmentService> logger,
        DtoProfundumKategorie kategorie)
    {
        var res = await managementService.CreateKategorieAsync(kategorie);
        if (res is null)
        {
            return Results.BadRequest("Could not create kategorie");
        }

        return Results.Ok(res.Id);
    }

    ///
    private static async Task<IResult> AddProfundumAsync(ProfundumManagementService managementService,
        UserAccessor userAccessor,
        AfraAppContext dbContext,
        ILogger<ProfundumEnrollmentService> logger,
        DtoProfundumDefinition definition)
    {
        var res = await managementService.CreateProfundumAsync(definition);
        if (res is null)
        {
            return Results.BadRequest("Could not create profundum");
        }

        return Results.Ok(res.Id);
    }

    ///
    private static async Task<IResult> AddInstanzAsync(ProfundumManagementService managementService,
        UserAccessor userAccessor,
        AfraAppContext dbContext,
        ILogger<ProfundumEnrollmentService> logger,
        DtoProfundumInstanz instanz)
    {
        var res = await managementService.CreateInstanzAsync(instanz);
        if (res is null)
        {
            return Results.BadRequest("Could not create instanz");
        }

        return Results.Ok(res.Id);
    }


    ///
    private static async Task<IResult> DoMatchingAsync(ProfundumEnrollmentService enrollmentService,
        UserAccessor userAccessor,
        AfraAppContext dbContext,
        ILogger<ProfundumEnrollmentService> logger)
    {
        var now = DateTime.UtcNow;
        var einwahlZeitraum = (await dbContext.ProfundumEinwahlZeitraeume
                .Include(ez => ez.Slots)
                .Where(ez => ez.EinwahlStart <= now && now < ez.EinwahlStop)
                .ToArrayAsync())
            .FirstOrDefault((ProfundumEinwahlZeitraum?)null);
        if (einwahlZeitraum is null)
        {
            return Results.NotFound("Kein offener Einwahlzeitraum");
        }

        var result = await enrollmentService.PerformMatching(einwahlZeitraum);
        return Results.Ok(result);
    }

    ///
    private static async Task<IResult> DoFinalMatchingAsync(ProfundumEnrollmentService enrollmentService,
        UserAccessor userAccessor,
        AfraAppContext dbContext,
        ILogger<ProfundumEnrollmentService> logger)
    {
        var now = DateTime.UtcNow;
        var einwahlZeitraum = (await dbContext.ProfundumEinwahlZeitraeume
                .Include(ez => ez.Slots)
                .Where(ez => ez.EinwahlStart <= now && now < ez.EinwahlStop)
                .ToArrayAsync())
            .FirstOrDefault((ProfundumEinwahlZeitraum?)null);
        if (einwahlZeitraum is null)
        {
            return Results.NotFound("Kein offener Einwahlzeitraum");
        }

        var result = await enrollmentService.PerformMatching(einwahlZeitraum, writeBackOnSuccess: true);
        return Results.Ok(result);
    }

    ///
    private static async Task<IResult> MatchingAsyncCsv(ProfundumEnrollmentService enrollmentService,
        UserAccessor userAccessor,
        AfraAppContext dbContext,
        ILogger<ProfundumEnrollmentService> logger)
    {
        var now = DateTime.UtcNow;
        var einwahlZeitraum = (await dbContext.ProfundumEinwahlZeitraeume
                .Include(ez => ez.Slots)
                .Where(ez => ez.EinwahlStart <= now && now < ez.EinwahlStop)
                .ToArrayAsync())
            .FirstOrDefault((ProfundumEinwahlZeitraum?)null);
        if (einwahlZeitraum is null)
        {
            return Results.NotFound("Kein offener Einwahlzeitraum");
        }

        var csv = await enrollmentService.GetStudentMatchingCsv(einwahlZeitraum);
        return Results.File(Encoding.UTF8.GetBytes(csv), "text/csv");
    }

    ///
    private static async Task<IResult> GetUnenrolledAsync(ProfundumEnrollmentService enrollmentService,
        UserAccessor userAccessor,
        AfraAppContext dbContext,
        ILogger<ProfundumEnrollmentService> logger)
    {
        var now = DateTime.UtcNow;
        var einwahlZeitraum = (await dbContext.ProfundumEinwahlZeitraeume
                .Include(ez => ez.Slots)
                .Where(ez => ez.EinwahlStart <= now && now < ez.EinwahlStop)
                .ToArrayAsync())
            .FirstOrDefault((ProfundumEinwahlZeitraum?)null);
        if (einwahlZeitraum is null)
        {
            return Results.NotFound("Kein offener Einwahlzeitraum");
        }

        var slots = einwahlZeitraum.Slots.Select(s => s.Id).ToArray();

        var result = await enrollmentService.GetMissingStudentsAsync(slots);
        return Results.Ok(result);
    }

    ///
    private static async Task<IResult> GetUnenrolledEmailsAsync(ProfundumEnrollmentService enrollmentService,
        UserAccessor userAccessor,
        AfraAppContext dbContext,
        ILogger<ProfundumEnrollmentService> logger)
    {
        var now = DateTime.UtcNow;
        var einwahlZeitraum = (await dbContext.ProfundumEinwahlZeitraeume
                .Include(ez => ez.Slots)
                .Where(ez => ez.EinwahlStart <= now && now < ez.EinwahlStop)
                .ToArrayAsync())
            .FirstOrDefault((ProfundumEinwahlZeitraum?)null);
        if (einwahlZeitraum is null)
        {
            return Results.NotFound("Kein offener Einwahlzeitraum");
        }

        var slots = einwahlZeitraum.Slots.Select(s => s.Id).ToArray();

        var result = await enrollmentService.GetMissingStudentsEmailsAsync(slots);
        return Results.Ok(result);
    }

    // TODO This is slow and a security risk. Replace with multiple endpoints
    private static async Task<Ok<QuartalEnrollmentOverview[]>> GetAllQuartaleWithEnrollments(
        AfraAppContext dbContext,
        UserAccessor userAccessor)
    {
        var user = await userAccessor.GetUserAsync();
        IQueryable<ProfundumInstanz> profundaQuery = dbContext.ProfundaInstanzen
            .AsSplitQuery()
            .Include(e => e.Profundum)
            .Include(e => e.Einschreibungen)
            .ThenInclude(e => e.BetroffenePerson)
            .Include(e => e.Slots);

        if (!user.GlobalPermissions.Contains(GlobalPermission.Profundumsverantwortlich))
            profundaQuery = profundaQuery.Where(p => p.Tutor == user);

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

    // TODO Replace this
    private static IAsyncEnumerable<DtoProfundumKategorie> GetKategorien(AfraAppContext dbContext)
    {
        return dbContext.ProfundaKategorien.AsAsyncEnumerable().Select(k => new DtoProfundumKategorie(k));
    }
}
