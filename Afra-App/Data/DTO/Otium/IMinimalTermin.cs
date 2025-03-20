namespace Afra_App.Data.DTO.Otium;

/// <summary>
///     A interface representing the minimal basic structure of a DTO for a termin
/// </summary>
public interface IMinimalTermin
{
    /// <summary>
    ///     The unique identifier for the termin
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    ///     The designation of the otium for which the termin is
    /// </summary>
    public string Otium { get; set; }

    /// <summary>
    ///     The location where the termin takes place
    /// </summary>
    public string Ort { get; set; }

    /// <summary>
    ///     The number of the block the termin is in
    /// </summary>
    public sbyte Block { get; set; }

    /// <summary>
    ///     The tutor handling the termin (optional)
    /// </summary>
    public PersonInfoMinimal? Tutor { get; set; }
}