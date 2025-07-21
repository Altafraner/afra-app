using Afra_App.Backbone.Authentication;
using Afra_App.Otium.Services;
using Microsoft.AspNetCore.Mvc;
using DtoSchultag = Afra_App.Otium.Domain.DTO.Schultag;

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
            .RequireAuthorization(AuthorizationPolicies.Otiumsverantwortlich);
        management.MapPost("/", AddSchultage);
        management.MapDelete("/{datum}", DeleteSchultag);
    }

    private static async Task<IResult> GetSchuljahr(SchuljahrService schuljahrService)
    {
        return Results.Ok(await schuljahrService.GetSchuljahrAsync());
    }

    private static async Task<IResult> AddSchultage(SchuljahrService schuljahrService,
        [FromBody] IEnumerable<DtoSchultag> schultageIn)
    {
        try
        {
            var schultage = await schuljahrService.AddRangeAsync(schultageIn);

            return Results.Created(string.Empty,
                schultage.Select(s => new DtoSchultag(s.Datum, s.Wochentyp, s.Blocks.Select(b => b.SchemaId))));
        }
        catch (KeyNotFoundException e)
        {
            return Results.Problem(new ProblemDetails
            {
                Title = "Invalid Block",
                Status = StatusCodes.Status400BadRequest,
                Detail = e.Message,
                Type = nameof(DtoSchultag.Blocks)
            });
        }
    }

    private static async Task<IResult> DeleteSchultag(SchuljahrService schuljahrService, DateOnly datum)
    {
        try
        {
            await schuljahrService.DeleteSchultagAsync(datum);
            return Results.NoContent();
        }
        catch (KeyNotFoundException e)
        {
            return Results.NotFound(e.Message);
        }
    }

    private static async Task<IResult> GetNow(SchuljahrService schuljahrService)
    {
        var block = await schuljahrService.GetCurrentBlockAsync();
        return block == null ? Results.NotFound() : Results.Ok(new { block.Id, block.SchemaId });
    }
}
