using Altafraner.AfraApp.Profundum.Domain.Models;

namespace Altafraner.AfraApp.Profundum.Domain.DTO;

///
public record BlockKatalog
{
    /// <summary>
    ///     A label for the Slot
    /// </summary>
    public required string Label { get; set; }

    /// <summary>
    ///     The canonical id of the slot as in <see cref="ProfundumSlot.ToString"/>
    /// </summary>
    public required string Id { get; set; }

    /// <summary>
    ///     The available set of <see cref="ProfundumInstanz"/> for the slot
    /// </summary>
    public required BlockOption[] Options { get; set; }

    ///
    public required BlockOption? Fixed { get; set; }
}
