using System.Diagnostics.CodeAnalysis;
using Altafraner.AfraApp.Domain.TimeInterval;
using Altafraner.AfraApp.Schuljahr.Domain.Models;

namespace Altafraner.AfraApp.Otium.Domain.DTO;

/// <summary>
///     Contains information about a block
/// </summary>
public record BlockInfo
{
    ///
    public BlockInfo()
    {
    }

    ///
    [SetsRequiredMembers]
    public BlockInfo(Block block, BlockMetadata metadata)
    {
        Id = block.Id;
        SchemaId = block.SchemaId;
        Name = metadata.Bezeichnung;
        Uhrzeit = metadata.Interval;
        Datum = block.SchultagKey;
    }

    /// <summary>
    ///     The unique identifier of the block
    /// </summary>
    public required Guid Id { get; set; }

    /// <summary>
    ///     The schema id of the block
    /// </summary>
    public required char SchemaId { get; set; }

    /// <summary>
    ///     The name of the block
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    ///     The time interval of the block
    /// </summary>
    public required TimeOnlyInterval Uhrzeit { get; set; }

    /// <summary>
    ///     The date of the block
    /// </summary>
    public required DateOnly Datum { get; set; }
}
