namespace Afra_App.Data.TimeInterval;

/// <summary>
/// A collection of DateTimeIntervals where removing an interval can split existing intervals.
/// </summary>
/// <remarks>With some work, this could implement <see cref="ICollection{T}"/> or <see cref="IList{T}"/> mit <c>T</c> als <see cref="ITimeInterval{T}"/></remarks>
public class Timeline<T> where T : struct
{
    private readonly ICollection<ITimeInterval<T>> _intervals = [];

    /// <summary>
    /// Creates a new empty timeline.
    /// </summary>
    public Timeline() { }

    /// <summary>
    /// Creates a new timeline containing the given intervals.
    /// </summary>
    /// <param name="intervals">The intervals to prefill with</param>
    /// <remarks>
    /// Be careful when adding intervals that overlap with or are adjacent with another. The timeline will not merge overlapping or adjacent intervals in the constructor.
    /// </remarks>
    public Timeline(IEnumerable<ITimeInterval<T>> intervals)
    {
        _intervals = new List<ITimeInterval<T>>(intervals);
    }

    /// <summary>
    /// Add a new interval to the timeline.
    /// </summary>
    /// <param name="interval">The interval to add</param>
    public void Add(ITimeInterval<T> interval)
    {
        var intersection = _intervals.Where(interval.IntersectsOrIsAdjacent);

        var current = interval;
        foreach (var item in intersection.ToList())
        {
            current = current.Union(item);
            _intervals.Remove(item);
        }

        _intervals.Add(current);
    }

    /// <summary>
    /// Remove an interval from the timeline. If the interval intersects with existing intervals, the existing intervals might be split.
    /// </summary>
    /// <param name="interval">The interval to remove</param>
    public void Remove(ITimeInterval<T> interval)
    {
        foreach (var listInterval in _intervals.Where(listInterval => listInterval.Intersects(interval)).ToList())
        {
            _intervals.Remove(listInterval);
            var (before, after) = listInterval.Difference(interval);
            if (before is not null) _intervals.Add(before);
            if (after is not null) _intervals.Add(after);
        }
    }

    /// <summary>
    /// Get a list of all intervals in the timeline.
    /// </summary>
    /// <returns>A list of all intervals in the timeline</returns>
    public List<ITimeInterval<T>> GetIntervals()
    {
        return _intervals.ToList();
    }
}