namespace Afra_App.Otium.Domain.DTO.Katalog;

/// <summary>
///     A interface representing the basic structure of a DTO for a termin
/// </summary>
public interface ITermin : IMinimalTermin
{
    /// <summary>
    ///     The maximum number of people that can be at the termin concurrently
    /// </summary>
    public int? MaxEinschreibungen { get; set; }

    /// <summary>
    ///     Whether the termin is cancelled
    /// </summary>
    public bool IstAbgesagt { get; set; }
}
