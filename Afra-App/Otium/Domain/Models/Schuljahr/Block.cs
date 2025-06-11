namespace Afra_App.Otium.Domain.Models.Schuljahr;

/// <summary>
///     A Block on a <see cref="Schultag" />
/// </summary>
public class Block
{
    /// <summary>
    ///     The unique identifier of the Block
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    ///     The <see cref="Schultag" /> the Block is on
    /// </summary>
    public Schultag Schultag { get; set; } = null!;

    /// <summary>
    ///     The foreign key of the <see cref="Schultag" /> the Block is on
    /// </summary>
    /// <remarks>Do not use directly!</remarks>
    internal DateOnly SchultagKey { get; set; }

    /// <summary>
    ///     The number of the Block
    /// </summary>
    public required char SchemaId { get; set; } = '\0';

    /// <summary>
    ///     True, iff the supervisor has checked on missing students for this Block.
    /// </summary>
    public bool SindAnwesenheitenFehlernderKontrolliert { get; set; } = false;
}