using Microsoft.AspNetCore.Mvc.Diagnostics;

namespace Afra_App.Data.TimeInterval;

/// <summary>
/// A collection of DateTimeIntervals where removing an interval can split existing intervals.
/// </summary>
public class ReverseTimeline
{
    private readonly HashSet<DateTimeInterval> _intervals = [];

    /// <summary>
    /// Add a new interval to the timeline.
    /// </summary>
    /// <remarks>
    /// Be careful when adding intervals that overlap with or are adjacent to existing intervals. The timeline will not merge overlapping or adjacent intervals.
    /// </remarks>
    /// <param name="interval">The interval to add</param>
    public void Add(DateTimeInterval interval)
    {
        _intervals.Add(interval);
    }
    
    /// <summary>
    /// Remove an interval from the timeline. If the interval intersects with existing intervals, the existing intervals might be split.
    /// </summary>
    /// <param name="interval">The interval to remove</param>
    public void Remove(DateTimeInterval interval)
    {
        foreach (var listInterval in _intervals.Where(listInterval => listInterval.Intersects(interval)).ToList())
        {
            _intervals.Remove(listInterval);
            var (before, after) = listInterval.Difference(interval);
            if (before is not null) _intervals.Add((DateTimeInterval)before);
            if (after is not null) _intervals.Add((DateTimeInterval)after);
        }
    }

    /// <summary>
    /// Get a list of all intervals in the timeline.
    /// </summary>
    /// <returns>A list of all intervals in the timeline</returns>
    public List<DateTimeInterval> GetIntervals()
    {
        return _intervals.ToList();
    }
}