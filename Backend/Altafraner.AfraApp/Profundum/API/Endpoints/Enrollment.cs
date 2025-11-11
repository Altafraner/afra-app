using Altafraner.AfraApp.Backbone.Authorization;
using Altafraner.AfraApp.Profundum.Services;
using Altafraner.AfraApp.User.Services;
using Microsoft.EntityFrameworkCore;

namespace Altafraner.AfraApp.Profundum.API.Endpoints;

/// <summary>
///     Contains endpoints for managing Profunda Enrollments.
/// </summary>
public static class Enrollment
{
    /// <summary>
    ///     Maps the Profunda Enrollment endpoints to the given <see cref="IEndpointRouteBuilder" />.
    /// </summary>
    public static void MapEnrollmentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/sus")
            .RequireAuthorization(AuthorizationPolicies.MittelStufeStudentOnly);
        group.MapPost("/wuensche", AddBelegWuenscheAsync);
        group.MapGet("/wuensche", GetOptionsAsync);
        group.MapGet("/einschreibungen", GetEnrollmentsAsync);
    }

    ///
    private static async Task<IResult> GetOptionsAsync(ProfundumEnrollmentService enrollmentService,
        UserAccessor userAccessor, AfraAppContext dbContext, ILogger<ProfundumEnrollmentService> logger)
    {
        var user = await userAccessor.GetUserAsync();
        var katalog = enrollmentService.GetKatalog(user);
        return Results.Ok(katalog);
    }

    ///
    private static async Task<IResult> AddBelegWuenscheAsync(ProfundumEnrollmentService enrollmentService,
        UserAccessor userAccessor, AfraAppContext dbContext, ILogger<ProfundumEnrollmentService> logger,
        Dictionary<String, Guid[]> wuensche)
    {
        var user = await userAccessor.GetUserAsync();
        try
        {
            await enrollmentService.RegisterBelegWunschAsync(user, wuensche);
        }
        catch (ProfundumEinwahlWunschException e)
        {
            return Results.BadRequest(e.Message);
        }

        return Results.Ok("Einwahl gespeichert");
    }

    ///
    private static async Task<IResult> GetEnrollmentsAsync(ProfundumEnrollmentService enrollmentService,
        UserAccessor userAccessor, AfraAppContext dbContext, ILogger<ProfundumEnrollmentService> logger)
    {
        var user = await userAccessor.GetUserAsync();

        var now = DateTime.UtcNow;
        var einwahlZeitraum = dbContext.ProfundumEinwahlZeitraeume
            .Include(ez => ez.Slots)
            .First(ez => ez.EinwahlStart <= now && now < ez.EinwahlStop);
        var slots = einwahlZeitraum.Slots.Select(s => s.Id).ToArray();

        var result = await enrollmentService.GetEnrollment(user, slots);
        return Results.Ok(result);
    }
}
