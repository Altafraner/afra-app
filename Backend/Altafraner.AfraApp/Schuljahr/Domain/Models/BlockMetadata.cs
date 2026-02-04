using Altafraner.AfraApp.Otium.Domain.Models.TimeInterval;

namespace Altafraner.AfraApp.Schuljahr.Domain.Models;

/// <summary>
///     A subdivision of a <see cref="Block" />
/// </summary>
public record BlockMetadata
{
    /// <summary>
    ///     An empty constructor needed for deserialization. Do not delete.
    /// </summary>
    public BlockMetadata() { }

    /// <summary>
    ///     The interval the block is in
    /// </summary>
    public TimeOnlyInterval Interval { get; init; }

    /// <summary>
    ///     True, iff the block is mandatory for all students.
    /// </summary>
    public bool Verpflichtend { get; init; }

    /// <summary>
    ///     The name of the block
    /// </summary>
    public required string Bezeichnung { get; init; }

    /// <summary>
    ///     The schema id of the block
    /// </summary>
    public char Id { get; init; }

    /// <summary>
    ///     The latest time a student can enroll in this block.
    /// </summary>
    public TimeOnly EinschreibenBis { get; init; }
}
