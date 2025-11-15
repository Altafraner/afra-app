using Afra_App;
using Afra_App.Backbone.Authentication;
using Afra_App.User.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Afra_App.Profundum.Domain.Models;
using Afra_App.User.Domain.Models;

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

        group.MapPost("/kriterium/create", async (
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

        group.MapPost("/kriterien/rename/{id}", async (
            Guid id,
            [FromServices] AfraAppContext dbContext,
            [FromBody] string newBezeichnung) =>
        {
            var kriterium = await dbContext.ProfundumsBewertungKriterien.FindAsync(id);
            if (kriterium == null) return Results.NotFound("Kriterium nicht gefunden");

            if (string.IsNullOrWhiteSpace(newBezeichnung))
                return Results.BadRequest("Bezeichnung darf nicht leer sein");

            kriterium.Bezeichnung = newBezeichnung;
            await dbContext.SaveChangesAsync();

            return Results.Ok(kriterium);
        }).RequireAuthorization(AuthorizationPolicies.AdminOnly);

        group.MapDelete("/kriterien/delete/{id}", async (
            Guid id,
            [FromServices] AfraAppContext dbContext) =>
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
            Person? studentReference = null;

            foreach (var dto in bewertungenDto)
            {
                var kriterium = await dbContext.ProfundumsBewertungKriterien.FindAsync(dto.KriteriumId);
                var instanz = await dbContext.ProfundaEinschreibungen
                    .Include(i => i.BetroffenePerson)
                    .Include(i => i.ProfundumInstanz)
                    .FirstOrDefaultAsync(i => i.ProfundumInstanzId == dto.InstanzId);

                if (kriterium == null || instanz == null)
                    return Results.BadRequest("Kriterium oder Instanz nicht gefunden");

                var student = instanz.BetroffenePerson;
                if (student == null)
                    return Results.BadRequest("Betroffene Person fehlt für Instanz");

                studentReference = (Person?)student;

                bewertungen.Add(new ProfundumBewertung
                {
                    Id = Guid.NewGuid(),
                    Kriterium = kriterium,
                    Instanz = instanz.ProfundumInstanz,
                    BetroffenePerson = student,
                    Grad = dto.Grad
                });
            }

            if (studentReference == null)
            {
                return Results.BadRequest("Keine gültige betroffene Person gefunden");
            }
            
            try
            {
                await bewertungService.AddBewertungenAsync(studentReference, bewertungen);
            }
            catch (ProfundumsBewertungException e)
            {
                return Results.BadRequest(e.Message);
            }

            return Results.Ok("Feedback gespeichert");
        })
        .RequireAuthorization(AuthorizationPolicies.TutorOnly);


        group.MapGet("/{id:guid}/profunda", async (
        [FromServices] AfraAppContext dbContext,
        Guid id) =>
        {
            var instanzen = await dbContext.ProfundaEinschreibungen
                .Include(e => e.ProfundumInstanz)
                .ThenInclude(i => i.Profundum)
                .Where(e => e.BetroffenePersonId == id)
                .Select(e => new
                {
                    instanzId = e.ProfundumInstanz.Id,
                    profundumName = e.ProfundumInstanz.Profundum.Bezeichnung
                })
                .ToListAsync();

            return Results.Ok(instanzen);
        });
        
        group.MapGet("/{personId:guid}/{instanzId:guid}", async (
        Guid personId,
        Guid instanzId,
        [FromServices] AfraAppContext dbContext) =>
        {
            var bewertungen = await dbContext.ProfundumBewertungen
                .Where(b => b.BetroffenePerson.Id == personId && b.Kriterium.Id == instanzId)
                .Select(b => new
                {
                    kriteriumId = b.Kriterium.Id,
                    grad = b.Grad
                })
                .ToListAsync();

            return Results.Ok(bewertungen);
        });

    }
}
