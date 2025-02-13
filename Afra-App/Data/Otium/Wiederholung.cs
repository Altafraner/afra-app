using System.ComponentModel.DataAnnotations;
using Afra_App.Data.People;

namespace Afra_App.Data.Otium;

public class Wiederholung
{
    public Guid Id { get; init; }
    public required Otium Otium { get; set; }
    public required Person Tutor { get; set; }
    public required DayOfWeek Wochentag { get; set; }
    public required byte Block { get; set; }
    
    [MaxLength(10)]
    public required string Location { get; set; }
    
    public ICollection<Termin> Termine { get; init; } = new List<Termin>();

    public Termin ErstelleTermin(DateOnly date)
    {
        if (date.DayOfWeek != Wochentag)
            throw new ArgumentException("The given date does not match the regularity's day of the week.");

        var termin = new Termin
        {
            Otium = Otium,
            Tutor = Tutor,
            Datum = date,
            Block = Block,
            Ort = Location,
            Wiederholung = this
        };
        
        Termine.Add(termin);
        return termin;
    }
}