using Altafraner.AfraApp.Otium.Configuration;
using Altafraner.AfraApp.Otium.Services;
using Altafraner.AfraApp.Schuljahr.Domain.DTO;
using Altafraner.AfraApp.Schuljahr.Domain.Models;
using Altafraner.AfraApp.User.Domain.Models;
using Altafraner.AfraApp.User.Services;
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
    private readonly UserService _userService;

    /// <summary>
    ///     Called from DI
    /// </summary>
    public SchuljahrService(AfraAppContext dbContext, IOptions<OtiumConfiguration> configuration,
        BlockHelper blockHelper,
        UserService userService)
    {
        _dbContext = dbContext;
        _configuration = configuration;
        _blockHelper = blockHelper;
        _userService = userService;
    }

    /// <summary>
    ///     Gets the current school year, including all school days and the next day.
    /// </summary>
    /// <returns></returns>
    public async Task<Domain.DTO.Schuljahr> GetSchuljahrAsync()
    {
        var blocks = _blockHelper.GetAll().ToDictionary(b => b.Id);
        var schultage = await _dbContext.Schultage
            .Include(s => s.Blocks)
            .OrderBy(s => s.Datum)
            .AsAsyncEnumerable()
            .Select(s => new DTO_Schultag(s.Datum, s.Wochentyp,
                s.Blocks.Select(b => new
                    {
                        Block = b,
                        Schema = blocks[b.SchemaId]
                    })
                    .OrderBy(b => b.Schema.Unterrichtsstunde)
                    .ThenBy(b => b.Block.SchemaId)
                    .Select(b => new BlockSchema(b.Block.SchemaId, b.Schema.Bezeichnung))))
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
        var schultagCreationRequests = schultageIn as SchultagCreation[] ?? schultageIn.ToArray();
        var supervisorIds = schultagCreationRequests.SelectMany(e => e.Blocks.SelectMany(e2 => e2.Supervisors));
        var supervisors = (await _userService.GetUsersByIdsAsync(supervisorIds)).ToDictionary(e => e.Id);
        var schultage = schultagCreationRequests.Select(s => new Models_Schultag
        {
            Datum = s.Datum,
            Wochentyp = s.Wochentyp,
            Blocks = s.Blocks.Select(b => new Block
            {
                SchemaId = b.SchemaId,
                Supervisors = b.Supervisors.Select(id => supervisors[id]).ToList()
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

            var oldBlocks = conflict.Blocks.ToList();
            var newBlocks = schultag.Blocks.ToList();

            foreach (var oldBlock in oldBlocks)
            {
                var correspondingBlock = newBlocks.FirstOrDefault(nb => nb.SchemaId == oldBlock.SchemaId);
                if (correspondingBlock is null)
                {
                    conflict.Blocks.Remove(oldBlock);
                    continue;
                }

                oldBlock.Supervisors = correspondingBlock.Supervisors;
                newBlocks.Remove(correspondingBlock);
            }

            conflict.Blocks.AddRange(newBlocks);
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
        var blocks = await _dbContext.Blocks
            .Include(b => b.Supervisors)
            .Where(b => b.SchultagKey == datum)
            .ToListAsync();
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
    ///     Adds a supervisor to a block
    /// </summary>
    /// <exception cref="KeyNotFoundException">Either the supervisor or user was not found</exception>
    /// <exception cref="InvalidOperationException">The user is a student and cannot be named supervisor</exception>
    public async Task AddSupervisor(Guid blockId, Guid supervisorId)
    {
        var block = await _dbContext.Blocks
            .Include(e => e.Supervisors)
            .FirstOrDefaultAsync(e => e.Id == blockId);
        if (block is null) throw new KeyNotFoundException();
        var supervisor = await _userService.GetUserByIdAsync(supervisorId);
        if (supervisor.Rolle is Rolle.Mittelstufe or Rolle.Oberstufe) throw new InvalidOperationException();

        if (block.Supervisors.Contains(supervisor)) return;
        block.Supervisors.Add(supervisor);
        await _dbContext.SaveChangesAsync();
    }

    /// <summary>
    ///     Removes a supervisor from a block
    /// </summary>
    /// <exception cref="KeyNotFoundException">Either the supervisor or user was not found</exception>
    public async Task DeleteSupervisor(Guid blockId, Guid supervisorId)
    {
        var block = await _dbContext.Blocks
            .Include(e => e.Supervisors)
            .FirstOrDefaultAsync(e => e.Id == blockId);
        if (block is null) throw new KeyNotFoundException();
        var supervisor = await _userService.GetUserByIdAsync(supervisorId);

        if (!block.Supervisors.Contains(supervisor)) return;
        block.Supervisors.Remove(supervisor);
        await _dbContext.SaveChangesAsync();
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

    /// <summary>
    ///     Adds a block to a schoolday
    /// </summary>
    /// <exception cref="KeyNotFoundException">Either the schoolday or schema do not exist</exception>
    public async Task AddBlockAsync(DateOnly datum, char schemaId)
    {
        var day = await _dbContext.Schultage
            .Include(s => s.Blocks)
            .FirstOrDefaultAsync(s => s.Datum == datum);
        var schema = _blockHelper.Get(schemaId);

        if (day is null || schema is null) throw new KeyNotFoundException();

        if (day.Blocks.Any(b => b.SchemaId == schemaId)) return;

        day.Blocks.Add(new Block
        {
            SchemaId = schemaId
        });
        await _dbContext.SaveChangesAsync();
    }
}
