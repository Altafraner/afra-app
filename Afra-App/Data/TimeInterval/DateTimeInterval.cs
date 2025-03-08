namespace Afra_App.Data.TimeInterval;

/// <summary>
///     Represents a time interval with a start <see cref="DateTime" /> and a duration.
/// </summary>
/// <param name="Start">The starting Date and Time for the DateTimeInterval</param>
/// <param name="Duration">The duration of the interval</param>
public record struct DateTimeInterval(DateTime Start, TimeSpan Duration) : ITimeInterval<DateTime>
{
    /// <summary>
    ///     Represents a time interval with a start <see cref="DateTime" /> and a duration.
    /// </summary>
    /// <param name="start">The starting Date and Time for the DateTimeInterval</param>
    /// <param name="end">The ending Date and Time for the DateTimeInterval</param>
    public DateTimeInterval(DateTime start, DateTime end) : this(start, end - start)
    {
    }

    /// <summary>
    ///     Gets the ending Date and Time for the DateTimeInterval.
    /// </summary>
    public DateTime End => Start + Duration;

    /// <summary>
    ///     Determines whether this interval intersects with another interval.
    /// </summary>
    /// <param name="other">The other DateTimeInterval to check for intersection.</param>
    /// <returns>True if the intervals intersect; otherwise, false.</returns>
    public bool Intersects(ITimeInterval<DateTime> other)
    {
        return Start < other.End && End > other.Start;
    }

    /// <summary>
    ///     Determines whether this interval intersects with or is adjacent to another interval.
    /// </summary>
    /// <param name="other">The other DateTimeInterval to check for intersection or adjacency.</param>
    /// <returns>True if the intervals intersect or are adjacent; otherwise, false.</returns>
    public bool IntersectsOrIsAdjacent(ITimeInterval<DateTime> other)
    {
        return Start <= other.End && End >= other.Start;
    }

    /// <summary>
    ///     Gets the intersection of this interval with another interval.
    /// </summary>
    /// <param name="other">The other DateTimeInterval to intersect with.</param>
    /// <returns>A new DateTimeInterval representing the intersection, or null if there is no intersection.</returns>
    public ITimeInterval<DateTime>? Intersection(ITimeInterval<DateTime> other)
    {
        if (!Intersects(other)) return null;

        var start = Start > other.Start ? Start : other.Start;
        var end = End < other.End ? End : other.End;
        return new DateTimeInterval(start, end);
    }

    /// <summary>
    ///     Determines whether this interval completely contains another interval.
    /// </summary>
    /// <param name="other">The other DateTimeInterval to check for containment.</param>
    /// <returns>True if this interval contains the other interval; otherwise, false.</returns>
    public bool Contains(ITimeInterval<DateTime> other)
    {
        return Start <= other.Start && End >= other.End;
    }
    
    /// <summary>
    ///     Determines whether this interval contains a specific DateTime.
    /// </summary>
    /// <param name="dateTime"> The DateTime to check for containment.</param>
    /// <returns>True if the DateTime is contained in the interval; otherwise, false.</returns>
    public bool Contains(DateTime dateTime)
    {
        return Start <= dateTime && End > dateTime;
    }

    /// <summary>
    ///     Gets the union of this interval with another interval.
    /// </summary>
    /// <param name="other">The other DateTimeInterval to union with.</param>
    /// <returns>A new DateTimeInterval representing the union of the two intervals.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the intervals do not intersect or are not adjacent.</exception>
    public ITimeInterval<DateTime> Union(ITimeInterval<DateTime> other)
    {
        if (!IntersectsOrIsAdjacent(other))
            throw new InvalidOperationException("The sets are not connected.");

        var start = Start < other.Start ? Start : other.Start;
        var end = End > other.End ? End : other.End;
        return new DateTimeInterval(start, end);
    }

    /// <summary>
    ///     Gets the difference between this interval and another interval.
    /// </summary>
    /// <param name="other">The other DateTimeInterval to subtract from this interval.</param>
    /// <returns>A tuple containing the intervals before and after the other interval.</returns>
    public (ITimeInterval<DateTime>? Before, ITimeInterval<DateTime>? After) Difference(ITimeInterval<DateTime> other)
    {
        if (!Intersects(other)) return other.Start > Start ? (null, this) : (this, null);

        DateTimeInterval? before = null;
        DateTimeInterval? after = null;
        if (other.Start >= Start) before = new DateTimeInterval(Start, other.Start);
        if (other.End <= End) after = new DateTimeInterval(other.End, End);

        return (before, after);
    }

    /// <summary>
    /// Converts the DateTimeInterval to a TimeOnlyInterval losing the date information.
    /// </summary>
    public TimeOnlyInterval ToTimeOnlyInterval() => new TimeOnlyInterval(TimeOnly.FromDateTime(Start), Duration);
}