using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Afra_App.Data.Otium;

public class Kategorie
{
    public Guid Id { get; init; }

    [MaxLength(50)] public required string Bezeichnung { get; set; }

    [MaxLength(20)] public string? Icon { get; set; }

    [MaxLength(20)] public string? CssColor { get; set; }

    public Kategorie? Parent { get; set; }
    public ICollection<Kategorie> Children { get; init; } = new List<Kategorie>();
    
    [JsonIgnore]
    public ICollection<Otium> Otia { get; init; } = new List<Otium>();
}