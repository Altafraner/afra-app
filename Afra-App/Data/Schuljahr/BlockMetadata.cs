using Afra_App.Data.TimeInterval;

namespace Afra_App.Data.Schuljahr;

/// <summary>
///     A subdivision of a <see cref="Block" />
/// </summary>
public record BlockMetadata
{
    /// <summary>
    ///     Constructs a Block with the given start and end times
    /// </summary>
    /// <param name="id">The schema-ID of the block</param>
    /// <param name="bezeichnung">The name of the block</param>
    /// <param name="start">The inclusive starting time for the block</param>
    /// <param name="end">The exclusive ending time for the block</param>
    /// <param name="Verpflichtend">Whether the block is mandatory</param>
    public BlockMetadata(char id, string bezeichnung, TimeOnly start, TimeOnly end, bool Verpflichtend = true) : this(
        id, bezeichnung, new TimeOnlyInterval(start, end),
        Verpflichtend)
    {
    }

    /// <summary>
    ///     Constructs a Block with the given start time and duration
    /// </summary>
    /// <param name="Id">The schema Id of the block</param>
    /// <param name="Bezeichnung">The name of the block</param>
    /// <param name="start">The inclusive starting time for the subblock</param>
    /// <param name="dauerMinuten">The duration for the subblock in minutes</param>
    /// <param name="Verpflichtend">Whether the subblock is mandatory</param>
    public BlockMetadata(char Id, string Bezeichnung, TimeOnly start, int dauerMinuten,
        bool Verpflichtend = true) : this(
        Id, Bezeichnung, start,
        start.AddMinutes(dauerMinuten),
        Verpflichtend)
    {
    }

    /// <summary>
    ///     Constructs a Block with the given interval and mandatory flag
    /// </summary>
    /// <param name="Id">The schema-ID of the block</param>
    /// <param name="Bezeichnung">The name of the block</param>
    /// <param name="Interval">The interval the block is in</param>
    /// <param name="Verpflichtend">Whether the block is mandatory</param>
    public BlockMetadata(char Id, string Bezeichnung, TimeOnlyInterval Interval, bool Verpflichtend)
    {
        this.Id = Id;
        this.Bezeichnung = Bezeichnung;
        this.Interval = Interval;
        this.Verpflichtend = Verpflichtend;
    }

    /// <summary>
    ///     An empty constructor needed for deserialization. Do not delete.
    /// </summary>
    public BlockMetadata()
    {
    }

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
    /// The schema id of the block
    /// </summary>
    public char Id { get; init; }
}
