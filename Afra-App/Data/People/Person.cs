using System.Text.Json.Serialization;
using Afra_App.Data.Otium;

namespace Afra_App.Data.People;

public class Person
{
    public Guid Id { get; set; }
    public required string Vorname { get; set; }
    public required string Nachname { get; set; }
    public required string Email { get; set; }
    
    [JsonIgnore]
    public Person? Mentor { get; set; }

    [JsonIgnore]
    public ICollection<Person> Mentees { get; set; } = new List<Person>();
    
    public required Rolle Rolle { get; set; }
    [JsonIgnore]
    public ICollection<Otium.Otium> VerwalteteOtia { get; set; } = new List<Otium.Otium>();
    [JsonIgnore]
    public ICollection<Einschreibung> OtiaEinschreibungen { get; set; } = new List<Einschreibung>();

    public override string ToString() => $"{Vorname} {Nachname}";
}