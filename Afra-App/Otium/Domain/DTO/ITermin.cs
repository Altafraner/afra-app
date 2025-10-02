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
    ///     The schema id of the block the termin is in
    /// </summary>
    public char BlockSchemaId { get; set; }

    /// <summary>
    ///     The name of the block the termin is in
    /// </summary>
    public string Block { get; set; }

    /// <summary>
    ///     The tutor handling the termin (optional)
    /// </summary>
    public PersonInfoMinimal? Tutor { get; set; }
}
