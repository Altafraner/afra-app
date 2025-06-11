using Afra_App.Backbone.Authentication;
using Afra_App.Otium.Configuration;
using Afra_App.Otium.Domain.Models.Schuljahr;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using DtoSchultag = Afra_App.Otium.Domain.DTO.Schultag;
using Schultag = Afra_App.Otium.Domain.Models.Schuljahr.Schultag;

namespace Afra_App.Otium.API.Endpoints;

/// <summary>
///     A class containing extension methods for the school year endpoint.
/// </summary>
public static class Schuljahr
{
    /// <summary>
    ///     Maps the school year endpoint to the given <see cref="IEndpointRouteBuilder" />.
    /// </summary>
    /// <param name="app"></param>
    public static void MapSchuljahr(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/schuljahr", GetSchuljahr)
            .RequireAuthorization();
        app.MapGet("/api/schuljahr/now", GetNow)
            .RequireAuthorization();

        var management = app.MapGroup("/api/management/schuljahr")
            .RequireAuthorization(AuthorizationPolicies.TutorOnly);
        management.MapPost("/", AddSchultage);
        management.MapDelete("/{datum}", DeleteSchultag);
    }

    private static async Task<IResult> GetSchuljahr(AfraAppContext dbContext)
    {
        var schultage = await dbContext.Schultage
            .Include(s => s.Blocks)
            .OrderBy(s => s.Datum)
            .Select(s => new DtoSchultag(s.Datum, s.Wochentyp, s.Blocks.Select(b => b.SchemaId)))
            .ToListAsync();
        var next = schultage.FirstOrDefault(s => s.Datum >= DateOnly.FromDateTime(DateTime.Now)) ??
                   schultage.LastOrDefault();

        return Results.Ok(new Domain.DTO.Schuljahr(next, schultage));
    }

    private static async Task<IResult> AddSchultage(AfraAppContext dbContext,
        IOptions<OtiumConfiguration> configuration,
        [FromBody] IEnumerable<DtoSchultag> schultageIn)
    {
        var blockKeys = configuration.Value.Blocks.Select(e => e.Id).Distinct();
        var schultage = schultageIn.Select(s => new Schultag
        {
            Datum = s.Datum,
            Wochentyp = s.Wochentyp,
            Blocks = s.Blocks.Select(b => new Block
            {
                SchemaId = b
            }).ToList()
        }).ToList();

        if (schultage.SelectMany(s => s.Blocks).Any(b => !blockKeys.Contains(b.SchemaId)))
            return Results.Problem(new ProblemDetails
            {
                Title = "Invalid Block",
                Status = StatusCodes.Status400BadRequest,
                Detail = "The block you provided is not valid. Valid blocks are: " + string.Join(", ", blockKeys),
                Type = nameof(DtoSchultag.Blocks)
            });


        foreach (var schultag in schultage.ToList())
        {
            var conflict = await dbContext.Schultage.Include(e => e.Blocks)
                .FirstOrDefaultAsync(s => s.Datum == schultag.Datum);
            if (conflict == null) continue;

            conflict.Wochentyp = schultag.Wochentyp;
            schultage.Remove(schultag);

            if (conflict.Blocks.All(b1 => schultag.Blocks.Any(b2 => b1.SchemaId == b2.SchemaId)) &&
                schultag.Blocks.All(b1 => conflict.Blocks.Any(b2 => b1.SchemaId == b2.SchemaId)))
                continue;

            foreach (var block in schultag.Blocks)
                if (conflict.Blocks.All(b => b.SchemaId != block.SchemaId))
                    conflict.Blocks.Add(block);

            foreach (var block in conflict.Blocks.ToList())
                if (schultag.Blocks.All(b => b.SchemaId != block.SchemaId))
                    conflict.Blocks.Remove(block);
        }

        await dbContext.Schultage.AddRangeAsync(schultage);
        await dbContext.SaveChangesAsync();

        return Results.Created(string.Empty,
            schultage.Select(s => new DtoSchultag(s.Datum, s.Wochentyp, s.Blocks.Select(b => b.SchemaId))));
    }

    private static async Task<IResult> DeleteSchultag(AfraAppContext dbContext, DateOnly datum)
    {
        var schultag = await dbContext.Schultage.FindAsync(datum);
        if (schultag == null) return Results.NotFound();

        dbContext.Schultage.Remove(schultag);
        await dbContext.SaveChangesAsync();

        return Results.NoContent();
    }

    private static async Task<IResult> GetNow(AfraAppContext dbContext, IOptions<OtiumConfiguration> config)
    {
        var now = DateTime.Now;
        var today = DateOnly.FromDateTime(now);

        // Give a 10 minute buffer. We have to use DateTime as we'd otherwise break on the edge of a day.
        var blockSchema =
            config.Value.Blocks.Where(b =>
                    today.ToDateTime(b.Interval.Start).AddMinutes(-10) <= now &&
                    today.ToDateTime(b.Interval.End).AddMinutes(10) >= now)
                .Select(b => b.Id)
                .ToList();
        if (blockSchema.Count == 0) return Results.NotFound();
        var block = await dbContext.Blocks
            .AsNoTracking()
            .Where(b => blockSchema.Contains(b.SchemaId))
            .Where(b => b.SchultagKey == today)
            .Select(b => new { b.Id, b.SchemaId })
            .FirstOrDefaultAsync();
        return block == null ? Results.NotFound() : Results.Ok(block);
    }
}
