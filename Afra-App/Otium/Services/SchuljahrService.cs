using Afra_App.Otium.Configuration;
using Afra_App.Otium.Domain.DTO;
using Afra_App.Otium.Domain.Models.Schuljahr;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using DtoSchultag = Afra_App.Otium.Domain.DTO.Schultag;
using Schultag = Afra_App.Otium.Domain.Models.Schuljahr.Schultag;

namespace Afra_App.Otium.Services;

/// <summary>
///     A service for managing school years and school days.
/// </summary>
public class SchuljahrService
{
    private readonly BlockHelper _blockHelper;
    private readonly IOptions<OtiumConfiguration> _configuration;
    private readonly AfraAppContext _dbContext;

    /// <summary>
    ///     Called from DI
    /// </summary>
    public SchuljahrService(AfraAppContext dbContext, IOptions<OtiumConfiguration> configuration,
        BlockHelper blockHelper)
    {
        _dbContext = dbContext;
        _configuration = configuration;
        _blockHelper = blockHelper;
    }

    /// <summary>
    ///     Gets the current school year, including all school days and the next day.
    /// </summary>
    /// <returns></returns>
    public async Task<Schuljahr> GetSchuljahrAsync()
    {
        var schultage = await _dbContext.Schultage
            .Include(s => s.Blocks)
            .OrderBy(s => s.Datum)
            .Select(s => new DtoSchultag(s.Datum, s.Wochentyp,
                s.Blocks.Select(b => new BlockSchema(b.SchemaId, _blockHelper.Get(b.SchemaId)!.Bezeichnung))))
            .ToListAsync();

        var next = schultage.FirstOrDefault(s => s.Datum >= DateOnly.FromDateTime(DateTime.Now)) ??
                   schultage.LastOrDefault();

        return new Schuljahr(next, schultage);
    }

    /// <summary>
    ///     Finds the currently active block for today.
    /// </summary>
    /// <returns>The currently active block, if any; Otherwise, null</returns>
    /// <exception cref="KeyNotFoundException">To</exception>
    public async Task<Block?> GetCurrentBlockAsync()
    {
        var now = DateTime.Now;

        var schultag = await _dbContext.Schultage.AsNoTracking()
            .Include(s => s.Blocks)
            .OrderBy(s => s.Datum)
            .FirstOrDefaultAsync(s => s.Datum == DateOnly.FromDateTime(now));

        if (schultag == null) throw new KeyNotFoundException("No schultag found for today");

        var time = TimeOnly.FromDateTime(DateTime.Now);
        var currentSchemas = GetCurrentSchemas(time);

        return schultag.Blocks.FirstOrDefault(b => currentSchemas.Contains(b.SchemaId));
    }

    /// <summary>
    ///     Deletes a schultag from the database.
    /// </summary>
    /// <param name="datum">The date of the schultag</param>
    /// <exception cref="KeyNotFoundException">There is no schoolday at the specified date</exception>
    public async Task DeleteSchultagAsync(DateOnly datum)
    {
        var schultag = await _dbContext.Schultage.FindAsync(datum);
        if (schultag == null) throw new KeyNotFoundException("Schultag not found");

        _dbContext.Schultage.Remove(schultag);
        await _dbContext.SaveChangesAsync();
    }

    /// <summary>
    ///     Adds a range of schultage to the database.
    /// </summary>
    /// <param name="schultageIn">The schooldays to add</param>
    /// <returns>A list of the newly created schooldays</returns>
    /// <exception cref="KeyNotFoundException">An invalid BlockId was provided</exception>
    public async Task<List<Schultag>> AddRangeAsync(IEnumerable<SchultagCreation> schultageIn)
    {
        var blockKeys = _configuration.Value.Blocks.Select(e => e.Id).Distinct();
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
            throw new KeyNotFoundException("Invalid block provided. Valid blocks are: " + string.Join(", ", blockKeys));

        foreach (var schultag in schultage.ToList())
        {
            var conflict = await _dbContext.Schultage.Include(e => e.Blocks)
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

        await _dbContext.Schultage.AddRangeAsync(schultage);
        await _dbContext.SaveChangesAsync();

        return schultage;
    }

    /// <summary>
    ///     Gets the blocks for a given date
    /// </summary>
    public async Task<List<Block>> GetBlocksAsync(DateOnly datum)
    {
        var blocks = await _dbContext.Blocks.Where(b => b.SchultagKey == datum).ToListAsync();
        return blocks;
    }

    /// <summary>
    ///     Gets all available block schemas.
    /// </summary>
    public IEnumerable<BlockSchema> GetAllSchemas()
    {
        return _blockHelper.GetAll().Select(bs => new BlockSchema(bs.Id, bs.Bezeichnung));
    }

    private List<char> GetCurrentSchemas(TimeOnly now)
    {
        return _configuration.Value.Blocks
            .Where(metadata => metadata.Interval.Contains(now))
            .Select(metadata => metadata.Id)
            .ToList();
    }
}
