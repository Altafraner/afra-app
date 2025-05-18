namespace Afra_App.Data.Schuljahr;

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
    public Schultag Schultag { get; set; }

    /// <summary>
    ///     The foreign key of the <see cref="Schultag" /> the Block is on
    /// </summary>
    /// <remarks>Do not use directly!</remarks>
    internal DateOnly SchultagKey { get; set; }

    /// <summary>
    ///     The number of the Block
    /// </summary>
    public required char SchemaId { get; set; } = '\0';
}
