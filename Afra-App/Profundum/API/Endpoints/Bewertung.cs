using Afra_App;
using Afra_App.Backbone.Authentication;
using Afra_App.User.Services;
using Afra_App.Profundum.Domain.DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

/// <summary>
///    Contains endpoints for managing Profunda Bewertungen.
/// </summary>
public static class Bewertung
{
    /// <summary>
    ///     Maps the Profundum Bewertung endpoints to the given <see cref="IEndpointRouteBuilder" />.
    /// </summary>
    public static void MapBewertungEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/bewertung");

        group.MapGet("/kriterien", async (
            [FromServices] AfraAppContext dbContext) =>
        {
            var kriterien = await dbContext.ProfundumsBewertungKriterien.ToListAsync();
            return Results.Ok(kriterien);
        });

        group.MapPost("/create-kriterium", async (
            [FromServices] AfraAppContext dbContext,
            [FromBody] string bezeichnung) =>
        {
            if (string.IsNullOrWhiteSpace(bezeichnung))
                return Results.BadRequest("Bezeichnung darf nicht leer sein");

            var kriterium = new ProfundumsBewertungKriterium
            {
                Id = Guid.NewGuid(),
                Bezeichnung = bezeichnung
            };

            dbContext.ProfundumsBewertungKriterien.Add(kriterium);
            await dbContext.SaveChangesAsync();

            return Results.Created($"/bewertung/kriterien/{kriterium.Id}", kriterium);
        })
        .RequireAuthorization(AuthorizationPolicies.AdminOnly);

        group.MapDelete("/kriterien/{id}", async (Guid id, [FromServices] AfraAppContext dbContext) =>
        {
            var kriterium = await dbContext.ProfundumsBewertungKriterien.FindAsync(id);
            if (kriterium == null) return Results.NotFound("Kriterium nicht gefunden");

            dbContext.ProfundumsBewertungKriterien.Remove(kriterium);
            await dbContext.SaveChangesAsync();

            return Results.Ok("Kriterium gelöscht");
        }).RequireAuthorization(AuthorizationPolicies.AdminOnly);

        group.MapGet("/me", async (
            [FromServices] ProfundumsBewertungService bewertungService,
            [FromServices] UserAccessor userAccessor) =>
        {
            var user = await userAccessor.GetUserAsync();
            var bewertungen = await bewertungService.GetBewertungenAsync(user);
            return Results.Ok(bewertungen);
        })
        .RequireAuthorization(AuthorizationPolicies.MittelStufeStudentOnly);

        group.MapGet("/{personId}", async (
            Guid personId,
            [FromServices] ProfundumsBewertungService bewertungService,
            [FromServices] AfraAppContext dbContext) =>
        {
            var person = await dbContext.Personen.FindAsync(personId);
            if (person == null)
                return Results.NotFound("Person nicht gefunden");

            var bewertungen = await bewertungService.GetBewertungenAsync(person);
            return Results.Ok(bewertungen);
        })
        .RequireAuthorization(AuthorizationPolicies.TutorOnly);

        group.MapPost("/", async (
            [FromServices] ProfundumsBewertungService bewertungService,
            [FromServices] AfraAppContext dbContext,
            List<DTOProfundumBewertung> bewertungenDto) =>
        {
            var bewertungen = new List<ProfundumBewertung>();

            foreach (var dto in bewertungenDto)
            {
                var kriterium = await dbContext.ProfundumsBewertungKriterien.FindAsync(dto.KriteriumId);
                var instanz = await dbContext.ProfundaInstanzen.FindAsync(dto.InstanzId);
                var student = await dbContext.Personen.FindAsync(dto.InstanzId);

                if (kriterium == null || instanz == null || student == null)
                    return Results.BadRequest("Kriterium, Instanz oder betroffene Person nicht gefunden");

                bewertungen.Add(new ProfundumBewertung
                {
                    Id = Guid.NewGuid(),
                    Kriterium = kriterium,
                    Instanz = instanz,
                    BetroffenePerson = student,
                    Grad = dto.Grad
                });
            }

            try
            {
                await bewertungService.AddBewertungenAsync(null!, bewertungen);
            }
            catch (ProfundumsBewertungException e)
            {
                return Results.BadRequest(e.Message);
            }

            return Results.Ok("Feedback gespeichert");
        })
        .RequireAuthorization(AuthorizationPolicies.TutorOnly);
    }
}
