namespace Afra_App.Backbone.Utilities;

/// <summary>
///     Contains extension methods for the <see cref="DateOnly" /> struct.
/// </summary>
public static class DateOnlyExtensions
{
    /// <summary>
    ///     Gets the start of the week (Monday) for the given date.
    /// </summary>
    public static DateOnly GetStartOfWeek(this DateOnly date)
    {
        /***
         * Sunday: 1 - 0 = 1 -> 1 - 7 = -6
         * Monday: 1 - 1 = 0
         * Saturday: 1 - 6 = -5
         */
        var diff = DayOfWeek.Monday - date.DayOfWeek;
        if (diff > 0)
            diff -= 7;
        return date.AddDays(diff);
    }
}
