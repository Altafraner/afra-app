using Afra_App.Backbone.Authentication;
using Afra_App.User.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

namespace Afra_App.Profundum.API.Endpoints;

/// <summary>
///   The Bewertung endpoints for Profundum management.
/// </summary>
public static class Bewertung
{
    /// <summary>
    /// Maps the Bewertung endpoints
    /// </summary>
    /// <param name="app"></param>
    public static void MapBewertungEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/bewertung");

        group.MapGet("/kriterien", GetKriterienAsync);
        group.MapPost("/kriterium/create", CreateKriteriumAsync)
            .RequireAuthorization(AuthorizationPolicies.AdminOnly);
        group.MapPost("/kriterien/rename/{id}", RenameKriteriumAsync)
            .RequireAuthorization(AuthorizationPolicies.AdminOnly);
        group.MapDelete("/kriterien/delete/{id}", DeleteKriteriumAsync)
            .RequireAuthorization(AuthorizationPolicies.AdminOnly);

        group.MapGet("/me", GetMyBewertungenAsync)
            .RequireAuthorization(AuthorizationPolicies.MittelStufeStudentOnly);

        group.MapGet("/{personId:guid}", GetBewertungenForPersonAsync)
            .RequireAuthorization(AuthorizationPolicies.TutorOnly);

        group.MapGet("/{personId:guid}/profunda", GetProfundaForPersonAsync);

        group.MapGet("/anker", GetAnkerAsync);

        group.MapGet("/{personId:guid}/{instanzId:guid}", GetBewertungenForProfundumAsync);

        group.MapGet("/{personId:guid}/{instanzId:guid}/ankerbewertungen",
            GetAllAnkerBewertungenAsync);

        group.MapPost("/{personId:guid}/{instanzId:guid}/{ankerId:guid}/{kriteriumId:guid}/{grad:int}/{wirdbewertet:bool}", AddBewertungAsync)
            .RequireAuthorization(AuthorizationPolicies.TutorOnly);

    }

    private static async Task<IResult> GetKriterienAsync(AfraAppContext db)
    {
        var data = await db.ProfundumsBewertungKriterien.ToListAsync();
        return Results.Ok(data);
    }

    private static async Task<IResult> CreateKriteriumAsync(
        AfraAppContext db, [FromBody] string bezeichnung)
    {
        if (string.IsNullOrWhiteSpace(bezeichnung))
            return Results.BadRequest("Bezeichnung darf nicht leer sein");

        var k = new ProfundumsBewertungKriterium
        {
            Id = Guid.NewGuid(),
            Bezeichnung = bezeichnung
        };

        db.ProfundumsBewertungKriterien.Add(k);
        await db.SaveChangesAsync();

        return Results.Created($"/bewertung/kriterien/{k.Id}", k);
    }

    private static async Task<IResult> RenameKriteriumAsync(
        Guid id, AfraAppContext db, [FromBody] string newBezeichnung)
    {
        var k = await db.ProfundumsBewertungKriterien.FindAsync(id);
        if (k == null) return Results.NotFound("Kriterium nicht gefunden");

        if (string.IsNullOrWhiteSpace(newBezeichnung))
            return Results.BadRequest("Bezeichnung darf nicht leer sein");

        k.Bezeichnung = newBezeichnung;
        await db.SaveChangesAsync();

        return Results.Ok(k);
    }

    private static async Task<IResult> DeleteKriteriumAsync(Guid id, AfraAppContext db)
    {
        var k = await db.ProfundumsBewertungKriterien.FindAsync(id);
        if (k == null) return Results.NotFound("Kriterium nicht gefunden");

        db.ProfundumsBewertungKriterien.Remove(k);
        await db.SaveChangesAsync();

        return Results.Ok("Kriterium gelöscht");
    }

    private static async Task<IResult> GetMyBewertungenAsync(
        ProfundumsBewertungService service, UserAccessor users)
    {
        var user = await users.GetUserAsync();
        var result = await service.GetBewertungenAsync(user);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetBewertungenForPersonAsync(
        Guid personId, ProfundumsBewertungService service, AfraAppContext db)
    {
        var person = await db.Personen.FindAsync(personId);
        if (person == null) return Results.NotFound("Person nicht gefunden");

        var result = await service.GetBewertungenAsync(person);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetProfundaForPersonAsync(AfraAppContext db, Guid personId)
    {
        var instanzen = await db.ProfundaEinschreibungen
            .Include(e => e.ProfundumInstanz)
            .ThenInclude(i => i.Profundum)
            .Where(e => e.BetroffenePersonId == personId)
            .Select(e => new
            {
                instanzId = e.ProfundumInstanz.Id,
                profundumName = e.ProfundumInstanz.Profundum.Bezeichnung,
                teachername = e.ProfundumInstanz.Tutor != null ? e.ProfundumInstanz.Tutor.Vorname + " " + e.ProfundumInstanz.Tutor.Nachname : "Unbekannt"
            })
            .ToListAsync();

        return Results.Ok(instanzen);
    }

    private static async Task<IResult> GetAnkerAsync(AfraAppContext db)
    {
        var data = await db.ProfundumAnker
            .Select(a => new { id = a.Id, bezeichnung = a.Bezeichnung })
            .ToListAsync();
        return Results.Ok(data);
    }

    private static async Task<IResult> GetBewertungenForProfundumAsync(
        Guid personId, Guid instanzId, AfraAppContext db)
    {
        var result = await db.ProfundumBewertungen
            .Where(b => b.BetroffenePerson.Id == personId && b.Instanz.Id == instanzId)
            .Select(b => new { kriteriumId = b.Kriterium.Id, grad = b.Grad })
            .ToListAsync();

        return Results.Ok(result);
    }

    private static async Task<IResult> GetAllAnkerBewertungenAsync(
        Guid personId, Guid instanzId, AfraAppContext db)
    {
        var result = await db.ProfundumAnkerBewertungen
            .Where(b => b.BewertetePerson.Id == personId && b.Profundum.Id == instanzId)
            .Select(b => new
            {
                ankerId = b.Anker.Id,
                ProfundumId = b.Profundum.Id,
                kriteriumId = b.Kriterium.Id,
                grad = b.Bewertung
            })
            .ToListAsync();

        return Results.Ok(result);
    }

    private static async Task<IResult> AddBewertungAsync(
        Guid personId, Guid instanzId, Guid ankerId, Guid kriteriumId,
        int grad, bool wirdbewertet,ProfundumsBewertungService service)
    {
        try
        {
            var bew = await service.AddBewertungAnkerAsync(personId, instanzId, ankerId, kriteriumId, grad, wirdbewertet);
            if (bew == null)
            {
                return Results.NoContent();
            }

            return Results.Created($"/bewertung/{bew.Id}", bew);
        }
        catch (ProfundumsBewertungException e)
        {
            return Results.BadRequest(e.Message);
        }
    }
}
