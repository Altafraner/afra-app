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
        app.MapPost("api/schuljahr/schultage", AddSchultage)
            .RequireAuthorization();
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

        await context.Schultage.AddRangeAsync(schultage);
        await context.SaveChangesAsync();

        return Results.Created(string.Empty,
            schultage.Select(s => new DtoSchultag(s.Datum, s.Wochentyp, s.Blocks.Select(b => b.SchemaId))));
    }
}
