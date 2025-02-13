using System.ComponentModel.DataAnnotations;

namespace Afra_App.Data.Otium;

public class Kategorie
{
    [Key]
    [MaxLength(50)]
    public required string Bezeichnung { get; set; }
    
    public ICollection<Otium> Otia { get; init; } = new List<Otium>();
}