namespace Altafraner.AfraApp.Profundum.Domain.Models;

///
public class ProfundumTermin
{
    ///
    public required ProfundumSlot Slot { get; set; }

    ///
    public required DateOnly Day { get; set; }

    ///
    public required TimeOnly StartTime { get; set; }

    ///
    public required TimeOnly EndTime { get; set; }
}
