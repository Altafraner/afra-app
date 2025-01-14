using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace Afra_App.Models;

public class OtiumsKategory
{
    [Key]
    [MaxLength(50)]
    public required string Designation { get; set; }
    
    public ICollection<Otium> Otia { get; init; } = new List<Otium>();
}