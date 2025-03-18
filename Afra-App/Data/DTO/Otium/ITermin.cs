namespace Afra_App.Data.DTO.Otium;

/// <summary>
/// A interface representing the basic structure of a DTO for a termin
/// </summary>
public interface ITermin : IMinimalTermin
{
    /// <summary>
    /// The categories the termin is in
    /// </summary>
    public IAsyncEnumerable<Guid> Kategorien { get; set; }

    /// <summary>
    /// The maximum number of people that can be at the termin concurrently
    /// </summary>
    public int? MaxEinschreibungen { get; set; }

    /// <summary>
    /// Whether the termin is cancelled
    /// </summary>
    public bool IstAbgesagt { get; set; }
}