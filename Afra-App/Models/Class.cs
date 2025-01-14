using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Afra_App.Models;

/// <summary>
/// A school class.
/// </summary>
/// <remarks>Be aware, that deleting a class from the DB will delete its associated students!</remarks>
public class Class
{
    public Guid Id { get; set; }
    public required int Level { get; set; }
    
    [MaxLength(10)]
    public required string? Appendix { get; set; }

    public Person? Tutor { get; set; }

    [JsonIgnore]
    public ICollection<Person> Students { get; init; } = new List<Person>();

    public override string ToString()
    {
        return $"{Level:00} {Appendix}" + (Tutor is null ? "" : $" ({Tutor})");
    }
}