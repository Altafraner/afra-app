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
        group.MapPost(
            "/wuensche",
            async (
                ProfundumEnrollmentService svc,
                UserAccessor userAccessor,
                Dictionary<String, Guid[]> wuensche
            ) => await svc.RegisterBelegWunschAsync(await userAccessor.GetUserAsync(), wuensche)
        );
        group.MapGet(
            "/wuensche",
            async (ProfundumEnrollmentService svc, UserAccessor userAccessor) =>
                svc.GetKatalog(await userAccessor.GetUserAsync())
        );
        group.MapGet("/einschreibungen", GetEnrollmentsAsync);
    }

    ///
    private static async Task<IResult> GetEnrollmentsAsync(
        ProfundumEnrollmentService svc,
        UserAccessor userAccessor,
        AfraAppContext dbContext
    )
    {
        var user = await userAccessor.GetUserAsync();

        var now = DateTime.UtcNow;
        var einwahlZeitraum = dbContext
            .ProfundumEinwahlZeitraeume.Include(ez => ez.Slots)
            .First(ez => ez.EinwahlStart <= now && now < ez.EinwahlStop);
        var slots = einwahlZeitraum.Slots.Select(s => s.Id).ToArray();

        return Results.Ok(await svc.GetEnrollment(user, slots));
    }
}
