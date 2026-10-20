using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using Altafraner.AfraApp.Otium.Configuration;
using Altafraner.AfraApp.Schuljahr.Domain.Models;
using Altafraner.AfraApp.User.Domain.Models;
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
        var blockStatus = GetBlockStatus(schema, nowTime);

        if (dayPending || (today && blockStatus == BlockStatus.Pending)) return BlockStatus.Pending;
        if (today && blockStatus == BlockStatus.Running) return BlockStatus.Running;
        return BlockStatus.Done;
    }

    /// <summary>
    ///     Gets the block status for a block metadata when the block is today
    /// </summary>
    public static BlockStatus GetBlockStatus(BlockMetadata schema, TimeOnly? now = null)
    {
        now ??= TimeOnly.FromDateTime(DateTime.Now);

        if (now < schema.Interval.Start) return BlockStatus.Pending;
        if (now < schema.Interval.End) return BlockStatus.Running;
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

    /// <summary>
    ///     Returns true if the user is required to attend a block of schema.
    /// </summary>
    public static bool IsRequiredToAttendBlock(BlockMetadata schema, Person user)
    {
        return IsRequiredToAttendBlock(schema.VerpflichteteGruppen, user.Gruppe);
    }

    /// <inheritdoc cref="IsRequiredToAttendBlock(BlockMetadata, Person)" />
    /// <param name="gruppen">The groups required to attend the block</param>
    /// <param name="gruppe">the group of the person attending</param>
    public static bool IsRequiredToAttendBlock(string[] gruppen, string? gruppe)
    {
        return gruppe is not null && gruppen.Contains(gruppe);
    }

    /// <summary>
    ///     Checks if a block has required attendees.
    /// </summary>
    public static bool HasRequiredAttendees(BlockMetadata schema)
    {
        return schema.VerpflichteteGruppen.Length != 0;
    }

    /// <summary>
    ///     Provides an expression that can be used in queries for detecting if a student must attend a block
    /// </summary>
    public static Expression<Func<Person, bool>> IsRequiredToAttendBlockExp(BlockMetadata schema)
    {
        return user => user.Gruppe != null && schema.VerpflichteteGruppen.Contains(user.Gruppe);
    }
}
