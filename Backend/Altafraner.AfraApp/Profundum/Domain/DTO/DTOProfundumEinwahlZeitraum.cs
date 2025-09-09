using Altafraner.AfraApp.Profundum.Domain.Models;

namespace Altafraner.AfraApp.Profundum.Domain.DTO;

///
public record DTOProfundumEinwahlZeitraum
{
    ///
    public DTOProfundumEinwahlZeitraum(ProfundumEinwahlZeitraum dbEinwahlZeitraum)
    {
        Id = dbEinwahlZeitraum.Id;
        EinwahlStart = dbEinwahlZeitraum.EinwahlStart.ToString();
        EinwahlStop = dbEinwahlZeitraum.EinwahlStop.ToString();
    }

    /// <inheritdoc cref="ProfundumEinwahlZeitraum.Id"/>
    public Guid? Id { get; set; }

    /// <inheritdoc cref="ProfundumEinwahlZeitraum.EinwahlStart"/>
    public string? EinwahlStart { get; set; }

    /// <inheritdoc cref="ProfundumEinwahlZeitraum.EinwahlStop"/>
    public string? EinwahlStop { get; set; }
}
