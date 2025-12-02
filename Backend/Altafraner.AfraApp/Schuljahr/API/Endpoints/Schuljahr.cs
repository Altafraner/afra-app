using Altafraner.AfraApp.Backbone.Authorization;
using Altafraner.AfraApp.Otium.Services;
using Altafraner.AfraApp.Schuljahr.Domain.DTO;
using Altafraner.AfraApp.Schuljahr.Services;
using Microsoft.AspNetCore.Mvc;

namespace Altafraner.AfraApp.Schuljahr.API.Endpoints;

/// <summary>
///     A class containing extension methods for the school year endpoint.
/// </summary>
public static class Schuljahr
{
    /// <summary>
    ///     Maps the school year endpoint to the given <see cref="IEndpointRouteBuilder" />.
    /// </summary>
    /// <param name="app"></param>
    public static void MapSchuljahrEndpoints(this IEndpointRouteBuilder app)
    {
        var general = app.MapGroup("/api/schuljahr")
            .RequireAuthorization();
        general.MapGet("/", GetSchuljahr);
        general.MapGet("/now", GetNow);
        general.MapGet("/{date}", GetBlocks);
        general.MapGet("/schemas", GetBlockSchemas);

        var management = app.MapGroup("/api/management/schuljahr")
            .RequireAuthorization(AuthorizationPolicies.Otiumsverantwortlich);
        management.MapPost("/", AddSchultage);
        management.MapDelete("/{datum}", DeleteSchultag);
    }

    private static async Task<IResult> GetSchuljahr(SchuljahrService schuljahrService)
    {
        return Results.Ok(await schuljahrService.GetSchuljahrAsync());
    }

    private static async Task<IResult> AddSchultage(SchuljahrService schuljahrService, BlockHelper blockHelper,
        [FromBody] IEnumerable<SchultagCreation> schultageIn)
    {
        try
        {
            var schultage = await schuljahrService.AddRangeAsync(schultageIn);

            return Results.Created(string.Empty,
                schultage.Select(s => new Schultag(s.Datum, s.Wochentyp,
                    s.Blocks.Select(b => new BlockSchema(b.SchemaId, blockHelper.Get(b.SchemaId)!.Bezeichnung)))));
        }
        catch (KeyNotFoundException e)
        {
            return Results.Problem(new ProblemDetails
            {
                Title = "Invalid Block",
                Status = StatusCodes.Status400BadRequest,
                Detail = e.Message,
                Type = nameof(Schultag.Blocks)
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

    private static async Task<IResult> GetBlocks(DateOnly date, SchuljahrService schuljahrService,
        BlockHelper blockHelper)
    {
        var blocks = await schuljahrService.GetBlocksAsync(date);
        var blocksMapped = blocks.Select(b => new
        {
            schemaId = b.SchemaId,
            name = blockHelper.Get(b.SchemaId)!.Bezeichnung,
            id = b.Id
        }).OrderBy(b => b.schemaId);

        return Results.Ok(blocksMapped);
    }

    private static IResult GetBlockSchemas(SchuljahrService schuljahrService)
    {
        return Results.Ok(schuljahrService.GetAllSchemas());
    }
}
