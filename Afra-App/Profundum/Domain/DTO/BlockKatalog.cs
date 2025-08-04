namespace Afra_App.Profundum.Domain.DTO;

///
public record BlockKatalog
{
    /// <summary>
    ///     A label for the Slot
    /// </summary>
    public required string label { get; set; }

    /// <summary>
    ///     The canonical id of the slot as in <see cref="Profundum.Domain.Models.ProfundumSlot.ToString"/>
    /// </summary>
    public required string id { get; set; }

    /// <summary>
    ///     The available set of <see cref="Profundum.Domain.Models.ProfundumInstanz"/> for the slot
    /// </summary>
    public required BlockOption[] options { get; set; }
}
