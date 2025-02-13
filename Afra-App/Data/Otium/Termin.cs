using System.ComponentModel.DataAnnotations;
using Afra_App.Data.People;

namespace Afra_App.Data.Otium;

public class Termin
{
    public Guid Id { get; set; }
    public required Otium Otium { get; set; }
    public required Person Tutor { get; set; }
    public required DateOnly Datum { get; set; }
    public required byte Block { get; set; }
    
    [MaxLength(10)]
    public required string Ort { get; set; }
    public Wiederholung? Wiederholung { get; set; }
    
    public ICollection<Einschreibung> Enrollments { get; set; } = new List<Einschreibung>();
    public bool IstAbgesagt { get; set; }

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