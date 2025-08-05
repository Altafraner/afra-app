namespace Afra_App.Profundum.Domain.DTO;

using Afra_App.Profundum.Domain.Models;

///
public record BlockKatalog
{
    /// <summary>
    ///     A label for the Slot
    /// </summary>
    public required string label { get; set; }

    /// <summary>
    ///     The canonical id of the slot as in <see cref="ProfundumSlot.ToString"/>
    /// </summary>
    public required string id { get; set; }

    /// <summary>
    ///     The available set of <see cref="ProfundumInstanz"/> for the slot
    /// </summary>
    public required BlockOption[] options { get; set; }
}
