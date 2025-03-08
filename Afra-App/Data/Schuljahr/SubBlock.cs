using Afra_App.Data.TimeInterval;

namespace Afra_App.Data.Schuljahr;

internal record SubBlock(TimeOnlyInterval Interval, bool Optional)
{
    public SubBlock(TimeOnly start, TimeOnly end, bool Optional) : this(new TimeOnlyInterval(start, end), Optional)
    {
    }

    public SubBlock(TimeOnly start, int dauerMinuten, bool Optional) : this(start, start.AddMinutes(dauerMinuten),
        Optional)
    {
    }
}