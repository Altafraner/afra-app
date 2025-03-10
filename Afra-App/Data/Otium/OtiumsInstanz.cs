using System.ComponentModel.DataAnnotations;
using Afra_App.Data.People;

namespace Afra_App.Data.Otium;

public abstract class OtiumsInstanz
{
    public required Otium Otium { get; set; }
    public required Person? Tutor { get; set; }
    public required byte Block { get; set; }
    [MaxLength(10)]
    public required string Ort { get; set; }
}