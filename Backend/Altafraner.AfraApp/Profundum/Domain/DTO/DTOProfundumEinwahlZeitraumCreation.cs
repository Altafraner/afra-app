using Altafraner.AfraApp.Profundum.Domain.Models;

namespace Altafraner.AfraApp.Profundum.Domain.DTO;

///
public record DTOProfundumEinwahlZeitraumCreation
{
    /// <inheritdoc cref="ProfundumEinwahlZeitraum.EinwahlStart"/>
    public string? EinwahlStart { get; set; }

    /// <inheritdoc cref="ProfundumEinwahlZeitraum.EinwahlStop"/>
    public string? EinwahlStop { get; set; }
}
