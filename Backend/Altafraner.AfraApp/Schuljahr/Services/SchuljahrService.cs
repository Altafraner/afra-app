using Altafraner.AfraApp.Otium.Configuration;
using Altafraner.AfraApp.Otium.Services;
using Altafraner.AfraApp.Schuljahr.Domain.DTO;
using Altafraner.AfraApp.Schuljahr.Domain.Models;
using Altafraner.Backbone.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using DTO_Schultag = Altafraner.AfraApp.Schuljahr.Domain.DTO.Schultag;
using Models_Schultag = Altafraner.AfraApp.Schuljahr.Domain.Models.Schultag;

namespace Altafraner.AfraApp.Schuljahr.Services;

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
    public async Task<Domain.DTO.Schuljahr> GetSchuljahrAsync()
    {
        var schultage = await _dbContext.Schultage
            .Include(s => s.Blocks)
            .OrderBy(s => s.Datum)
            .Select(s => new DTO_Schultag(s.Datum, s.Wochentyp,
                s.Blocks.Select(b => new BlockSchema(b.SchemaId, _blockHelper.Get(b.SchemaId)!.Bezeichnung))))
            .ToListAsync();

        var next = schultage.FirstOrDefault(s => s.Datum >= DateOnly.FromDateTime(DateTime.Now)) ??
                   schultage.LastOrDefault();

        return new Domain.DTO.Schuljahr(next, schultage);
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

        if (schultag == null) return null;

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
    public async Task<List<Models_Schultag>> AddRangeAsync(IEnumerable<SchultagCreation> schultageIn)
    {
        var blockKeys = _configuration.Value.Blocks.Select(e => e.Id).Distinct();
        var schultage = schultageIn.Select(s => new Models_Schultag
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

            conflict.Blocks.AddRange(schultag.Blocks.Where(block =>
                conflict.Blocks.All(b => b.SchemaId != block.SchemaId)));
            conflict.Blocks.RemoveAll(block => schultag.Blocks.All(b => b.SchemaId != block.SchemaId));
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
    ///     Gets a schultag by its date.
    /// </summary>
    public async Task<Models_Schultag?> GetSchultagAsync(DateOnly datum)
    {
        var schultag = await _dbContext.Schultage
            .Include(s => s.Blocks)
            .FirstOrDefaultAsync(s => s.Datum == datum);
        return schultag;
    }

    /// <summary>
    ///     Gets the last day with blocks in the current week.
    /// </summary>
    /// <param name="datum">A date of a day in the week</param>
    /// <returns>The last day of the week that has any scheduled blocks. Null iff there are no scheduled blocks for the week</returns>
    public async Task<DateOnly?> GetLastDayWithBlocksAsync(DateOnly datum)
    {
        var monday = datum.GetStartOfWeek();
        var endOfWeek = monday.AddDays(7);

        var day = await _dbContext.Blocks
            .Where(b => b.SchultagKey >= monday && b.SchultagKey < endOfWeek)
            .OrderByDescending(b => b.SchultagKey)
            .FirstOrDefaultAsync();

        return day?.SchultagKey;
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
