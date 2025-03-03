using Afra_App.Data.Schuljahr;

namespace Afra_App.Data.Otium;

public class Wiederholung : OtiumsInstanz
{
    public Guid Id { get; init; }
    public required DayOfWeek Wochentag { get; set; }
    public required Wochentyp Wochentyp { get; set; }
    
    public ICollection<Termin> Termine { get; init; } = new List<Termin>();

    public Termin ErstelleTermin(Schultag schultag)
    {
        if (schultag.Datum.DayOfWeek != Wochentag || 
            !schultag.OtiumsBlock[Block] ||
            schultag.Wochentyp != Wochentyp)
            throw new ArgumentException("The given date does not match the regularity's day of the week.");

        var termin = new Termin
        {
            Otium = Otium,
            Tutor = Tutor,
            Schultag = schultag,
            Block = Block,
            Ort = Ort,
            Wiederholung = this
        };
        
        Termine.Add(termin);
        return termin;
    }
}