using System.Diagnostics.Contracts;
using Altafraner.AfraApp.Otium.Configuration;
using Altafraner.AfraApp.Schuljahr.Domain.Models;
using Microsoft.Extensions.Options;

namespace Altafraner.AfraApp.Otium.Services;

/// <summary>
///     A class that provides helper methods for working with block schemas.
/// </summary>
public class BlockHelper
{
    private readonly IOptions<OtiumConfiguration> _otiumConfiguration;

    /// <summary>
    ///     Constructor for the BlockHelper class.
    /// </summary>
    /// <param name="otiumConfiguration"></param>
    public BlockHelper(IOptions<OtiumConfiguration> otiumConfiguration)
    {
        _otiumConfiguration = otiumConfiguration;
    }

    /// <summary>
    ///     Gets the block metadata for a given schema ID.
    /// </summary>
    /// <param name="schemaId">The schemaId of the Block</param>
    /// <returns>The metadata for the block configuration with the given schema id if exists; Otherwise, null. </returns>
    [Pure]
    public BlockMetadata? Get(char schemaId)
    {
        return _otiumConfiguration.Value.Blocks.FirstOrDefault(b => b.Id == schemaId);
    }

    /// <summary>
    ///     Returns a list of all block metadata configurations.
    /// </summary>
    public List<BlockMetadata> GetAll()
    {
        return _otiumConfiguration.Value.Blocks.ToList();
    }

    /// <summary>
    ///     Checks if a block is done or currently running.
    /// </summary>
    public bool IsBlockDoneOrRunning(Block block)
    {
        var now = DateTime.Now;
        var today = DateOnly.FromDateTime(now);
        var nowTime = TimeOnly.FromDateTime(now);

        return block.SchultagKey < today
               || (block.SchultagKey == today && Get(block.SchemaId)!.Interval.Start <= nowTime);
    }
}
