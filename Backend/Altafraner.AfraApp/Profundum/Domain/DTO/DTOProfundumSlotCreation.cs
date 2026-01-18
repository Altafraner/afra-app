using Altafraner.AfraApp.Profundum.Domain.Models;

namespace Altafraner.AfraApp.Profundum.Domain.DTO;

/// <summary>
///     A request to change or create a profundum slot
/// </summary>
public record DTOProfundumSlotCreation
{
    /// <inheritdoc cref="ProfundumSlot.Jahr"/>
    public required int Jahr { get; set; }

    /// <inheritdoc cref="ProfundumSlot.Quartal"/>
    public required ProfundumQuartal Quartal { get; set; }

    /// <inheritdoc cref="ProfundumSlot.Wochentag"/>
    public required DayOfWeek Wochentag { get; set; }

    /// <inheritdoc cref="ProfundumSlot.EinwahlZeitraum"/>
    public required Guid EinwahlZeitraumId { get; set; }
}
