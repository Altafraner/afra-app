using Altafraner.AfraApp.User.Domain.DTO;

namespace Altafraner.AfraApp.Otium.Domain.DTO;

/// <summary>
///     A interface representing the minimal basic structure of a DTO for a termin
/// </summary>
public interface ITermin
{
    /// <summary>
    ///     The unique identifier for the termin
    /// </summary>
    Guid Id { get; set; }

    /// <summary>
    ///     The designation of the otium for which the termin is
    /// </summary>
    string Otium { get; set; }

    /// <summary>
    ///     The if of the otium for which the termin is
    /// </summary>
    Guid OtiumId { get; set; }

    /// <summary>
    ///     The location where the termin takes place
    /// </summary>
    string Ort { get; set; }

    /// <summary>
    ///     The tutor handling the termin (optional)
    /// </summary>
    PersonInfoMinimal? Tutor { get; set; }
}
