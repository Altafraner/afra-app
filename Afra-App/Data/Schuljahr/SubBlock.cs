using Afra_App.Data.TimeInterval;

namespace Afra_App.Data.Schuljahr;

/// <summary>
///     A subdivision of a <see cref="Block" />
/// </summary>
public record SubBlock
{
    /// <summary>
    ///     Constructs a SubBlock with the given start and end times
    /// </summary>
    /// <param name="start">The inclusive starting time for the subblock</param>
    /// <param name="end">The exclusive ending time for the subblock</param>
    /// <param name="Mandatory">Whether the subblock is mandatory</param>
    public SubBlock(TimeOnly start, TimeOnly end, bool Mandatory = true) : this(new TimeOnlyInterval(start, end),
        Mandatory)
    {
    }

    /// <summary>
    ///     Constructs a SubBlock with the given start time and duration
    /// </summary>
    /// <param name="start">The inclusive starting time for the subblock</param>
    /// <param name="dauerMinuten">The duration for the subblock in minutes</param>
    /// <param name="Mandatory">Whether the subblock is mandatory</param>
    public SubBlock(TimeOnly start, int dauerMinuten, bool Mandatory = true) : this(start,
        start.AddMinutes(dauerMinuten),
        Mandatory)
    {
    }

    /// <summary>
    ///     Constructs a SubBlock with the given interval and mandatory flag
    /// </summary>
    /// <param name="Interval">The interval the subblock is in</param>
    /// <param name="Mandatory">Whether the subblock is mandatory</param>
    public SubBlock(TimeOnlyInterval Interval, bool Mandatory)
    {
        this.Interval = Interval;
        this.Mandatory = Mandatory;
    }

    /// <summary>
    ///     An empty constructor needed for deserialization. Do not delete.
    /// </summary>
    public SubBlock()
    {
    }

    /// <summary>
    ///     The interval the subblock is in
    /// </summary>
    public TimeOnlyInterval Interval { get; init; }

    /// <summary>
    ///     True, iff the subblock is mandatory for all students.
    /// </summary>
    public bool Mandatory { get; init; }
}