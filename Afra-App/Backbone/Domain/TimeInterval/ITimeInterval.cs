namespace Afra_App.Backbone.Domain.TimeInterval;

/// <summary>
///     A time-interval with a start and end point.
/// </summary>
/// <typeparam name="T">The time-like parameter for the starting point</typeparam>
/// <remarks>
///     This is mostly here to enforce consistency between <see cref="DateTimeInterval" /> and
///     <see cref="TimeOnlyInterval" />
/// </remarks>
public interface ITimeInterval<T> where T : struct
{
    /// <summary>
    ///     Gets the starting point for a time-interval
    /// </summary>
    public T Start { get; }

    /// <summary>
    ///     Gets the duration of the time-interval
    /// </summary>
    public TimeSpan Duration { get; }

    /// <summary>
    ///     Gets the ending point for a time-interval.
    /// </summary>
    public T End { get; }

    /// <summary>
    ///     Determines whether this interval intersects with another interval.
    /// </summary>
    /// <param name="other">The other interval to check for intersection.</param>
    /// <returns>True if the intervals intersect; otherwise, false.</returns>
    public bool Intersects(ITimeInterval<T> other);

    /// <summary>
    ///     Determines whether this interval intersects with or is adjacent to another interval.
    /// </summary>
    /// <param name="other">The other DateTimeInterval to check for intersection or adjacency.</param>
    /// <returns>True if the intervals intersect or are adjacent; otherwise, false.</returns>
    public bool IntersectsOrIsAdjacent(ITimeInterval<T> other);

    /// <summary>
    ///     Gets the intersection of this interval with another interval.
    /// </summary>
    /// <param name="other">The other DateTimeInterval to intersect with.</param>
    /// <returns>A new DateTimeInterval representing the intersection, or null if there is no intersection.</returns>
    public ITimeInterval<T>? Intersection(ITimeInterval<T> other);

    /// <summary>
    ///     Determines whether this interval completely contains another interval.
    /// </summary>
    /// <param name="other">The other DateTimeInterval to check for containment.</param>
    /// <returns>True if this interval contains the other interval; otherwise, false.</returns>
    public bool Contains(ITimeInterval<T> other);

    /// <summary>
    ///     Determines whether this interval contains a specific DateTime.
    /// </summary>
    /// <param name="dateTime"> The DateTime to check for containment.</param>
    /// <returns>True if the DateTime is contained in the interval; otherwise, false.</returns>
    public bool Contains(T dateTime);

    /// <summary>
    ///     Gets the union of this interval with another interval.
    /// </summary>
    /// <param name="other">The other DateTimeInterval to union with.</param>
    /// <returns>A new DateTimeInterval representing the union of the two intervals.</returns>
    /// <exception cref="InvalidOperationException">
    ///     Thrown if the intervals do not intersect or are not intersecting or
    ///     adjacent.
    /// </exception>
    public ITimeInterval<T> Union(ITimeInterval<T> other);

    /// <summary>
    ///     Gets the difference between this interval and another interval.
    /// </summary>
    /// <param name="other">The other DateTimeInterval to subtract from this interval.</param>
    /// <returns>A tuple containing the intervals before and after the other interval.</returns>
    public (ITimeInterval<T>? Before, ITimeInterval<T>? After) Difference(ITimeInterval<T> other);
}