namespace Altafraner.AfraApp.Profundum.Domain.DTO;

/// <summary>
///     A slot a student is already fixed/finalized into, shown read-only alongside the rankable catalog.
/// </summary>
public record DTOFixierterSlot
{
    /// <summary>
    ///     The canonical id of the slot (see <see cref="Models.ProfundumSlot.ToString" />).
    /// </summary>
    public required string SlotId { get; set; }

    /// <summary>
    ///     A label for the slot.
    /// </summary>
    public required string SlotLabel { get; set; }

    /// <summary>
    ///     The name of the Profundum the student is fixed into, or "-" if fixed to "not enrolled".
    /// </summary>
    public required string Bezeichnung { get; set; }
}
