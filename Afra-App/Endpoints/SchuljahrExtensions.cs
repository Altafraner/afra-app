using Afra_App.Authentication;
using Afra_App.Data;
using Afra_App.Data.Configuration;
using Afra_App.Data.DTO;
using Afra_App.Data.Schuljahr;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using DtoSchultag = Afra_App.Data.DTO.Schultag;
using Schultag = Afra_App.Data.Schuljahr.Schultag;

namespace Afra_App.Endpoints;

/// <summary>
///     A class containing extension methods for the school year endpoint.
/// </summary>
public static class SchuljahrExtensions
{
    /// <summary>
    ///     Maps the school year endpoint to the given <see cref="IEndpointRouteBuilder" />.
    /// </summary>
    /// <param name="app"></param>
    public static void MapSchuljahr(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/schuljahr", GetSchuljahr)
            .RequireAuthorization();

        var management = app.MapGroup("/api/management/schuljahr")
            .RequireAuthorization(AuthorizationPolicies.TutorOnly);
        management.MapPost("/", AddSchultage);
        management.MapDelete("/{datum}", DeleteSchultag);
    }

    private static async Task<IResult> GetSchuljahr(AfraAppContext context)
    {
        var schultage = await context.Schultage
            .Include(s => s.Blocks)
            .OrderBy(s => s.Datum)
            .Select(s => new DtoSchultag(s.Datum, s.Wochentyp, s.Blocks.Select(b => b.SchemaId)))
            .ToListAsync();
        var next = schultage.FirstOrDefault(s => s.Datum >= DateOnly.FromDateTime(DateTime.Now)) ??
                   schultage.LastOrDefault();

        return Results.Ok(new Schuljahr(next, schultage));
    }

    private static async Task<IResult> AddSchultage(AfraAppContext context, IOptions<OtiumConfiguration> configuration,
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
            var conflict = await context.Schultage.Include(e => e.Blocks)
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

        await context.Schultage.AddRangeAsync(schultage);
        await context.SaveChangesAsync();

        return Results.Created(string.Empty,
            schultage.Select(s => new DtoSchultag(s.Datum, s.Wochentyp, s.Blocks.Select(b => b.SchemaId))));
    }

    private static async Task<IResult> DeleteSchultag(AfraAppContext context, DateOnly datum)
    {
        var schultag = await context.Schultage.FindAsync(datum);
        if (schultag == null) return Results.NotFound();

        context.Schultage.Remove(schultag);
        await context.SaveChangesAsync();

        return Results.NoContent();
    }
}
