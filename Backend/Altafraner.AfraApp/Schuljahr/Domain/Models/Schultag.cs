using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Altafraner.AfraApp.Schuljahr.Domain.Models;

/// <summary>
///     Represents a school day
/// </summary>
public class Schultag
{
    /// <summary>
    ///     The date of the school day
    /// </summary>
    [Key]
    public DateOnly Datum { get; set; }

    /// <summary>
    ///     The type of week the school day is in
    /// </summary>
    public Wochentyp Wochentyp { get; set; }

    /// <summary>
    ///     The blocks of the school day
    /// </summary>
    [JsonIgnore]
    public List<Block> Blocks { get; set; } = null!;
}
