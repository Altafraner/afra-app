namespace Afra_App.Data.Otium;

public class Wiederholung : OtiumsInstanz
{
    public Guid Id { get; init; }
    public required DayOfWeek Wochentag { get; set; }
    
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
            Ort = Ort,
            Wiederholung = this
        };
        
        Termine.Add(termin);
        return termin;
    }
}