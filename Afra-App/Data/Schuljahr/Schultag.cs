using System.ComponentModel.DataAnnotations;

namespace Afra_App.Data.Schuljahr;

/// <summary>
/// Represents a school day
/// </summary>
public class Schultag
{
    /// <summary>
    /// The date of the school day
    /// </summary>
    [Key]
    public DateOnly Datum { get; set; }

    /// <summary>
    /// The type of week the school day is in
    /// </summary>
    public Wochentyp Wochentyp { get; set; }

    /// <summary>
    /// A boolean array representing the Otia-blocks of the day. True, if the block is planned; Otherwise, false.
    /// </summary>
    /// <remarks>
    /// There would be more elegant ways to represent this, i just haven't invested the time to implement them jet.
    /// </remarks>
    public bool[] OtiumsBlock { get; set; } = new bool[2];
}