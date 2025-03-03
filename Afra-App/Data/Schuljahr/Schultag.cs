using System.ComponentModel.DataAnnotations;

namespace Afra_App.Data.Schuljahr;

public class Schultag
{
    [Key]
    public DateOnly Datum { get; set; }
    public Wochentyp Wochentyp { get; set; }
    public bool[] OtiumsBlock { get; set; } = new bool[2];
}