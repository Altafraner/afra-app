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
    [Obsolete("Use GetBlockStatus instead")]
    public bool IsBlockDoneOrRunning(Block block)
    {
        var now = DateTime.Now;
        var today = DateOnly.FromDateTime(now);
        var nowTime = TimeOnly.FromDateTime(now);

        return block.SchultagKey < today
               || (block.SchultagKey == today && Get(block.SchemaId)!.Interval.Start <= nowTime);
    }

    /// <summary>
    ///     Checks if a block is done
    /// </summary>
    /// <param name="block"></param>
    /// <returns></returns>
    public BlockStatus GetBlockStatus(Block block)
    {
        var now = DateTime.Now;
        var dateToday = DateOnly.FromDateTime(now);
        var nowTime = TimeOnly.FromDateTime(now);
        var schema = Get(block.SchemaId)!;

        var dayPending = block.SchultagKey > dateToday;
        var today = block.SchultagKey == dateToday;

        if (dayPending || (today && nowTime <= schema.Interval.Start)) return BlockStatus.Pending;
        if (today && nowTime <= schema.Interval.End) return BlockStatus.Running;
        return BlockStatus.Done;
    }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    /// <summary>
    ///     A blocks status
    /// </summary>
    public enum BlockStatus
    {
        Pending,
        Running,
        Done
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
