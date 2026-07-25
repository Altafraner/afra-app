using Altafraner.AfraApp.Backbone.Authorization;
using Altafraner.AfraApp.Otium.API;
using Altafraner.AfraApp.Otium.Services;
using Altafraner.AfraApp.Schuljahr.Domain.DTO;
using Altafraner.AfraApp.Schuljahr.Services;
using Altafraner.AfraApp.User.Domain.DTO;
using Microsoft.AspNetCore.Http.HttpResults;
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
        management.MapPost("/{datum}/block", AddBlock);
        management.MapPost("/block/{blockId:guid}/supervisors", AddSupervisor);
        management.MapDelete("/block/{blockId:guid}", RemoveBlock);
        management.MapDelete("/block/{blockId:guid}/supervisors/{userId:guid}", DeleteSupervisor);
    }

    private static async Task<NoContent> AddBlock(DateOnly datum,
        ValueWrapper<char> value,
        SchuljahrService schuljahrService)
    {
        await schuljahrService.AddBlockAsync(datum, value.Value);
        return TypedResults.NoContent();
    }

    private static async Task<Results<NoContent, NotFound>> RemoveBlock(Guid blockId, AfraAppContext dbContext)
    {
        var block = await dbContext.Blocks.FindAsync(blockId);
        if (block is null) return TypedResults.NotFound();
        dbContext.Blocks.Remove(block);
        await dbContext.SaveChangesAsync();
        return TypedResults.NoContent();
    }

    private static async Task<Results<NoContent, NotFound>> DeleteSupervisor(Guid blockId,
        Guid userId,
        SchuljahrService schuljahrService)
    {
        try
        {
            await schuljahrService.DeleteSupervisor(blockId, userId);
        }
        catch (KeyNotFoundException)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.NoContent();
    }

    private static async Task<Results<NoContent, NotFound, ValidationProblem>> AddSupervisor(Guid blockId,
        ValueWrapper<Guid> userWrapper,
        SchuljahrService schuljahrService)
    {
        try
        {
            await schuljahrService.AddSupervisor(blockId, userWrapper.Value);
        }
        catch (KeyNotFoundException)
        {
            return TypedResults.NotFound();
        }
        catch (InvalidOperationException)
        {
            return TypedResults.ValidationProblem(new Dictionary<string, string[]>
            {
                ["value"] = ["The user a student and cannot be a supervisor"]
            });
        }

        return TypedResults.NoContent();
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
        var blocksMapped = blocks
            .Select(b => (Block: b, Schema: blockHelper.Get(b.SchemaId)!))
            .OrderBy(b => b.Schema.Unterrichtsstunde)
            .ThenBy(b => b.Schema.Id)
            .Select(b => new
            {
                schemaId = b.Schema.Id,
                name = b.Schema.Bezeichnung,
                id = b.Block.Id,
                supervisors = b.Block.Supervisors.Select(e => new PersonInfoMinimal(e))
            });

        return Results.Ok(blocksMapped);
    }

    private static IResult GetBlockSchemas(SchuljahrService schuljahrService)
    {
        return Results.Ok(schuljahrService.GetAllSchemas());
    }
}
