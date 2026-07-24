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
        group.MapPost("/wuensche", async (ProfundumEnrollmentService svc, UserAccessor userAccessor, List<Guid> wuensche) =>
            await svc.RegisterBelegWunschAsync(await userAccessor.GetUserAsync(), wuensche)
        );
        group.MapPost("/wuensche/entwurf", async (ProfundumEnrollmentService svc, UserAccessor userAccessor, List<Guid> wuensche) =>
            await svc.RegisterBelegWunschAsync(await userAccessor.GetUserAsync(), wuensche, istEntwurf: true)
        );
        group.MapGet("/wuensche", async (ProfundumEnrollmentService svc, UserAccessor userAccessor) => svc.GetKatalog(await userAccessor.GetUserAsync()));
        group.MapGet("/einschreibungen", GetEnrollmentsAsync);
        group.MapGet("/einwahl/aktiv", (AfraAppContext db) => {
            var now = DateTime.UtcNow;
            return db.ProfundumEinwahlZeitraeume.Any(ez => ez.EinwahlStart <= now && now < ez.EinwahlStop);
        });

        var partner = group.MapGroup("/partner");
        partner.MapGet("/", async (ProfundumPartnerService svc, UserAccessor userAccessor) =>
        {
            var (einladungen, wuensche) = await svc.GetForStudentAsync(await userAccessor.GetUserAsync());
            return Results.Ok(new { einladungen, wuensche });
        });
        partner.MapPost("/{definitionId:guid}", async (ProfundumPartnerService svc, UserAccessor userAccessor, Guid definitionId) =>
            await svc.CreateEinladungAsync(await userAccessor.GetUserAsync(), definitionId));
        partner.MapPost("/redeem/{definitionId:guid}/{token}", async (ProfundumPartnerService svc, UserAccessor userAccessor, Guid definitionId, string token) =>
            await svc.RedeemEinladungAsync(await userAccessor.GetUserAsync(), definitionId, token));
        partner.MapDelete("/einladung/{token}", async (ProfundumPartnerService svc, UserAccessor userAccessor, string token) =>
        {
            await svc.DeleteEinladungAsync(await userAccessor.GetUserAsync(), token);
            return Results.NoContent();
        });
        partner.MapDelete("/wunsch/{id:guid}", async (ProfundumPartnerService svc, UserAccessor userAccessor, Guid id) =>
        {
            await svc.DeleteWunschAsync(await userAccessor.GetUserAsync(), id);
            return Results.NoContent();
        });
    }

    ///
    private static async Task<IResult> GetEnrollmentsAsync(ProfundumEnrollmentService svc,
        UserAccessor userAccessor, AfraAppContext dbContext)
    {
        var user = await userAccessor.GetUserAsync();

        var now = DateTime.UtcNow;
        var einwahlZeitraum = dbContext.ProfundumEinwahlZeitraeume
            .Include(ez => ez.Slots)
            .First(ez => ez.EinwahlStart <= now && now < ez.EinwahlStop);
        var slots = einwahlZeitraum.Slots.Select(s => s.Id).ToArray();

        return Results.Ok(await svc.GetEnrollment(user, slots));
    }
}
