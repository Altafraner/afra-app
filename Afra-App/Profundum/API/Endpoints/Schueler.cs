using Afra_App.Backbone.Authentication;
using Afra_App.Profundum.Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


/// <summary>
///   Contains endpoints for managing Schueler profiles.
/// </summary>
public static class Schueler
{
    /// <summary>
    ///    Maps the Schueler endpoints to the given <see cref="IEndpointRouteBuilder" />.
    /// </summary>
    /// <param name="app"></param>
    public static void MapSchuelerEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/schueler")
            .RequireAuthorization(AuthorizationPolicies.TeacherOrAdmin);

        group.MapGet("/listprofunda", async (
            [FromServices] Afra_App.AfraAppContext dbContext) =>
        {
            var profiles = await dbContext.ProfundaEinschreibungen
                .Include(e => e.BetroffenePerson)
                .Include(e => e.ProfundumInstanz)
                .ToListAsync();
            return Results.Ok(profiles);
        });
    }
}