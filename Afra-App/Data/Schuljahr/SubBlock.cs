using Afra_App.Data.TimeInterval;

namespace Afra_App.Data.Schuljahr;

public record SubBlock
{
    public SubBlock(TimeOnly start, TimeOnly end, bool Optional) : this(new TimeOnlyInterval(start, end), Optional)
    {
    }

    public SubBlock(TimeOnly start, int dauerMinuten, bool Optional) : this(start, start.AddMinutes(dauerMinuten),
        Optional)
    {
    }

    public SubBlock(TimeOnlyInterval Interval, bool Optional)
    {
        this.Interval = Interval;
        this.Optional = Optional;
    }

    public SubBlock()
    {
    }

    public TimeOnlyInterval Interval { get; init; }
    public bool Optional { get; init; }

    public void Deconstruct(out TimeOnlyInterval Interval, out bool Optional)
    {
        Interval = this.Interval;
        Optional = this.Optional;
    }
}