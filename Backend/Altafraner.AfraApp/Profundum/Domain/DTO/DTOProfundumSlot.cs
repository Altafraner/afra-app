using Altafraner.AfraApp.Profundum.Domain.Models;

using System.Diagnostics.CodeAnalysis;

namespace Altafraner.AfraApp.Profundum.Domain.DTO;

///
public record DTOProfundumSlot
{
    ///
    [SetsRequiredMembers]
    public DTOProfundumSlot(ProfundumSlot dbSlot)
    {
        Id = dbSlot.Id;
        Jahr = dbSlot.Jahr;
        Quartal = dbSlot.Quartal;
        Wochentag = dbSlot.Wochentag;
        EinwahlZeitraumId = dbSlot.EinwahlZeitraum.Id;
    }

    /// <inheritdoc cref="ProfundumDefinition.Id"/>
    public Guid? Id { get; set; }

    /// <inheritdoc cref="ProfundumSlot.Jahr"/>
    public required int Jahr { get; set; }

    /// <inheritdoc cref="ProfundumSlot.Quartal"/>
    public required ProfundumQuartal Quartal { get; set; }

    /// <inheritdoc cref="ProfundumSlot.Wochentag"/>
    public required DayOfWeek Wochentag { get; set; }

    /// <inheritdoc cref="ProfundumSlot.EinwahlZeitraum"/>
    public required Guid EinwahlZeitraumId { get; set; }
}
