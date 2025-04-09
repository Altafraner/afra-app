using Afra_App.Data;
using Afra_App.Data.DTO;
using Microsoft.EntityFrameworkCore;

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
    }

    private static async Task<IResult> GetSchuljahr(AfraAppContext context)
    {
        var schultage = await context.Schultage
            .Include(s => s.Blocks)
            .OrderBy(s => s.Datum)
            .Select(s => new Schultag(s.Datum, s.Wochentyp, s.Blocks.Select(b => b.Nummer)))
            .ToListAsync();
        var next = schultage.FirstOrDefault(s => s.Datum >= DateOnly.FromDateTime(DateTime.Now)) ?? schultage.Last();

        return Results.Ok(new Schuljahr(next, schultage));
    }
}