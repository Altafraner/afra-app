namespace Afra_App.Data.TimeInterval;

/// <summary>
///     Represents a time interval with a start <see cref="TimeOnly" /> and a duration.
/// </summary>
public record struct TimeOnlyInterval : ITimeInterval<TimeOnly>
{
    /// <summary>
    ///     Represents a time interval with a start <see cref="TimeOnly" /> and a duration.
    /// </summary>
    /// <param name="start">The starting Date and Time for the TimeOnlyInterval</param>
    /// <param name="end">The ending Date and Time for the TimeOnlyInterval</param>
    public TimeOnlyInterval(TimeOnly start, TimeOnly end) : this(start, end - start)
    {
    }

    /// <summary>
    ///     Represents a time interval with a start <see cref="TimeOnly" /> and a duration.
    /// </summary>
    /// <param name="Start">The starting Date and Time for the TimeOnlyInterval</param>
    /// <param name="Duration">The duration of the interval</param>
    public TimeOnlyInterval(TimeOnly Start, TimeSpan Duration)
    {
        this.Start = Start;
        this.Duration = Duration;
    }

    /// <summary>
    ///     A default constructor for the configuration binding
    /// </summary>
    public TimeOnlyInterval()
    {
    }

    /// <summary>
    ///     Gets the ending Date and Time for the TimeOnlyInterval.
    /// </summary>
    public TimeOnly End => Start.Add(Duration);

    /// <summary>The starting Date and Time for the TimeOnlyInterval</summary>
    public TimeOnly Start { get; set; }

    /// <summary>The duration of the interval</summary>
    public TimeSpan Duration { get; set; }

    /// <summary>
    ///     Determines whether this interval intersects with another interval.
    /// </summary>
    /// <param name="other">The other TimeOnlyInterval to check for intersection.</param>
    /// <returns>True if the intervals intersect; otherwise, false.</returns>
    /// TODO: Create EF Core mapping to improve query performance
    public bool Intersects(ITimeInterval<TimeOnly> other)
    {
        return Start < other.End && End > other.Start;
    }

    /// <summary>
    ///     Determines whether this interval intersects with or is adjacent to another interval.
    /// </summary>
    /// <param name="other">The other TimeOnlyInterval to check for intersection or adjacency.</param>
    /// <returns>True if the intervals intersect or are adjacent; otherwise, false.</returns>
    public bool IntersectsOrIsAdjacent(ITimeInterval<TimeOnly> other)
    {
        return Start <= other.End && End >= other.Start;
    }

    /// <summary>
    ///     Gets the intersection of this interval with another interval.
    /// </summary>
    /// <param name="other">The other TimeOnlyInterval to intersect with.</param>
    /// <returns>A new TimeOnlyInterval representing the intersection, or null if there is no intersection.</returns>
    public ITimeInterval<TimeOnly>? Intersection(ITimeInterval<TimeOnly> other)
    {
        if (!Intersects(other)) return null;

        var start = Start > other.Start ? Start : other.Start;
        var end = End < other.End ? End : other.End;
        return new TimeOnlyInterval(start, end);
    }

    /// <summary>
    ///     Determines whether this interval completely contains another interval.
    /// </summary>
    /// <param name="other">The other TimeOnlyInterval to check for containment.</param>
    /// <returns>True if this interval contains the other interval; otherwise, false.</returns>
    public bool Contains(ITimeInterval<TimeOnly> other)
    {
        return Start <= other.Start && End >= other.End;
    }

    /// <summary>
    ///     Determines whether this interval contains a specific TimeOnly.
    /// </summary>
    /// <param name="time"> The Time to check for containment.</param>
    /// <returns>True if the Time is contained in the interval; otherwise, false.</returns>
    public bool Contains(TimeOnly time)
    {
        return Start <= time && End > time;
    }

    /// <summary>
    ///     Gets the union of this interval with another interval.
    /// </summary>
    /// <param name="other">The other TimeOnlyInterval to union with.</param>
    /// <returns>A new TimeOnlyInterval representing the union of the two intervals.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the intervals do not intersect or are not adjacent.</exception>
    public ITimeInterval<TimeOnly> Union(ITimeInterval<TimeOnly> other)
    {
        if (!IntersectsOrIsAdjacent(other))
            throw new InvalidOperationException("The sets are not connected.");

        var start = Start < other.Start ? Start : other.Start;
        var end = End > other.End ? End : other.End;
        return new TimeOnlyInterval(start, end);
    }

    /// <summary>
    ///     Gets the difference between this interval and another interval.
    /// </summary>
    /// <param name="other">The other TimeOnlyInterval to subtract from this interval.</param>
    /// <returns>A tuple containing the intervals before and after the other interval.</returns>
    public (ITimeInterval<TimeOnly>? Before, ITimeInterval<TimeOnly>? After) Difference(ITimeInterval<TimeOnly> other)
    {
        if (!Intersects(other)) return other.Start > Start ? (null, this) : (this, null);

        TimeOnlyInterval? before = null;
        TimeOnlyInterval? after = null;
        if (other.Start > Start) before = new TimeOnlyInterval(Start, other.Start);
        if (other.End < End) after = new TimeOnlyInterval(other.End, End);

        return (before, after);
    }

    /// <summary>
    ///     Determines whether this interval is adjacent to another interval.
    /// </summary>
    /// <param name="other">The other TimeOnlyInterval to check for  adjacency</param>
    /// <returns>True if the intervals are adjacent; otherwise, false.</returns>
    public bool IsAdjacent(ITimeInterval<TimeOnly> other)
    {
        return Start == other.End || End == other.Start;
    }

    /// <summary>
    ///     Converts the TimeOnlyInterval to a <see cref="DateTimeInterval" /> using the specified <see cref="DateOnly" /> as
    ///     Date component while maintaining Time and Duratation.
    /// </summary>
    public DateTimeInterval ToDateTimeInterval(DateOnly date)
    {
        return new DateTimeInterval(new DateTime(date, Start), Duration);
    }

    public readonly void Deconstruct(out TimeOnly Start, out TimeSpan Duration)
    {
        Start = this.Start;
        Duration = this.Duration;
    }
}