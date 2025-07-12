using Afra_App.User.Domain.DTO;

namespace Afra_App.Otium.Domain.DTO;

/// <summary>
///     A interface representing the minimal basic structure of a DTO for a termin
/// </summary>
public interface ITermin
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
    ///     The if of the otium for which the termin is
    /// </summary>
    public Guid OtiumId { get; set; }

    /// <summary>
    ///     The location where the termin takes place
    /// </summary>
    public string Ort { get; set; }

    /// <summary>
    ///     The number of the block the termin is in
    /// </summary>
    public char Block { get; set; }

    /// <summary>
    ///     The tutor handling the termin (optional)
    /// </summary>
    public PersonInfoMinimal? Tutor { get; set; }
}
