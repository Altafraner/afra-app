using Afra_App.Data;
using Microsoft.EntityFrameworkCore;

namespace Afra_App.Endpoints;

/// <summary>
/// A class containing extension methods for the school year endpoint.
/// </summary>
public static class SchuljahrExtensions
{
    /// <summary>
    /// Maps the school year endpoint to the given <see cref="IEndpointRouteBuilder" />.
    /// </summary>
    /// <param name="app"></param>
    public static void MapSchuljahr(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/schuljahr", GetSchuljahr)
            .RequireAuthorization();
    }

    private static async Task<IResult> GetSchuljahr(AfraAppContext context)
    {
        var schultage = await context.Schultage.OrderBy(s => s.Datum).ToListAsync();
        var next = schultage.FirstOrDefault(s => s.Datum >= DateOnly.FromDateTime(DateTime.Now)) ?? schultage.Last();

        return Results.Ok(new Data.DTO.Schuljahr(next, schultage));
    }
}