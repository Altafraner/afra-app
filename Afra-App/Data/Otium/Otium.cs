using System.ComponentModel.DataAnnotations;
using Afra_App.Data.People;

namespace Afra_App.Data.Otium;

public class Otium
{
    public Guid Id { get; set; }
    [MaxLength(50)] public required string Bezeichnung { get; set; }
    [MaxLength(500)] public required string Beschreibung { get; set; }
    public required Kategorie Kategorie { get; set; }

    public ICollection<Person> Verantwortliche { get; set; } = new List<Person>();

    public ICollection<Wiederholung> Wiederholungen { get; set; } = new List<Wiederholung>();

    public ICollection<Termin> Termine { get; set; } = new List<Termin>();
}