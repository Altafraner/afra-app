using Afra_App.Backbone.Authentication;
using Afra_App.Profundum.Domain.DTO;
using Afra_App.Profundum.Domain.Models;
using Afra_App.Profundum.Services;
using Afra_App.User.Services;
using Microsoft.EntityFrameworkCore;

namespace Afra_App.Profundum.API.Endpoints;

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
            .RequireAuthorization(AuthorizationPolicies.TeacherOrAdmin);

        group.MapPut("/einwahlzeitraum", AddEinwahlZeitraumAsync);
        group.MapPut("/slot", AddSlotAsync);
        group.MapPut("/kategorie", AddKategorieAsync);
        group.MapPut("/profundum", AddProfundumAsync);
        group.MapPut("/instanz", AddInstanzAsync);

        group.MapGet("/missing", GetUnenrolledAsync);
        group.MapGet("/matching", MatchingAsync);
        group.MapGet("/matching.csv", MatchingAsyncCSV);
    }

    ///
    private static async Task<IResult> AddEinwahlZeitraumAsync(ProfundumManagementService managementService,
           UserAccessor userAccessor, AfraAppContext dbContext, ILogger<ProfundumEnrollmentService> logger, DTOProfundumEinwahlZeitraum zeitraum)
    {
        var res = await managementService.CreateEinwahlZeitraumAsync(zeitraum);
        if (res is null)
        {
            return Results.BadRequest("Could not create einwahlZeitraum");
        }
        return Results.Ok(res.Id);
    }

    ///
    private static async Task<IResult> AddSlotAsync(ProfundumManagementService managementService,
           UserAccessor userAccessor, AfraAppContext dbContext, ILogger<ProfundumEnrollmentService> logger, DTOProfundumSlot slot)
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
           UserAccessor userAccessor, AfraAppContext dbContext, ILogger<ProfundumEnrollmentService> logger, DTOProfundumKategorie kategorie)
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
           UserAccessor userAccessor, AfraAppContext dbContext, ILogger<ProfundumEnrollmentService> logger, DTOProfundumDefinition definition)
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
           UserAccessor userAccessor, AfraAppContext dbContext, ILogger<ProfundumEnrollmentService> logger, DTOProfundumInstanz instanz)
    {
        var res = await managementService.CreateInstanzAsync(instanz);
        if (res is null)
        {
            return Results.BadRequest("Could not create instanz");
        }
        return Results.Ok(res.Id);
    }


    ///
    private static async Task<IResult> MatchingAsync(ProfundumEnrollmentService enrollmentService,
           UserAccessor userAccessor, AfraAppContext dbContext, ILogger<ProfundumEnrollmentService> logger)
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
    private static async Task<IResult> MatchingAsyncCSV(ProfundumEnrollmentService enrollmentService,
           UserAccessor userAccessor, AfraAppContext dbContext, ILogger<ProfundumEnrollmentService> logger)
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

        await enrollmentService.PerformMatching(einwahlZeitraum);
        var csv = await enrollmentService.GetStudentMatchingCSV(einwahlZeitraum);
        return Results.File(System.Text.Encoding.UTF8.GetBytes(csv), "text/csv");
    }

    ///
    private static async Task<IResult> GetUnenrolledAsync(ProfundumEnrollmentService enrollmentService,
           UserAccessor userAccessor, AfraAppContext dbContext, ILogger<ProfundumEnrollmentService> logger)
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

        var result = await enrollmentService.GetMissingStudents(slots);
        return Results.Ok(result);
    }
}
