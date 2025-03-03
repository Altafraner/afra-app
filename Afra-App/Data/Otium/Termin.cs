using Afra_App.Data.People;
using Afra_App.Data.Schuljahr;

namespace Afra_App.Data.Otium;

public class Termin : OtiumsInstanz
{
    public Guid Id { get; set; }
    public Wiederholung? Wiederholung { get; set; }
    
    public required Schultag Schultag { get; set; }
    
    public ICollection<Einschreibung> Enrollments { get; set; } = new List<Einschreibung>();
    public bool IstAbgesagt { get; set; }
    public int? MaxEinschreibungen { get; set; }

    public Einschreibung Einschreiben(Person betroffenePerson, TimeOnly start, TimeOnly end)
    {
        if (IstAbgesagt)
            throw new InvalidOperationException("The installment has been canceled.");
        
        var einschreibung = new Einschreibung
        {
            Termin = this,
            BetroffenePerson = betroffenePerson,
            Start = start,
            Ende = end
        };
        
        Enrollments.Add(einschreibung);
        return einschreibung;
    }
}