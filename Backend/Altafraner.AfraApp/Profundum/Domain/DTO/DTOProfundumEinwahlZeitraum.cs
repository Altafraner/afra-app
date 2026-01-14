using Altafraner.AfraApp.Profundum.Domain.Models;

namespace Altafraner.AfraApp.Profundum.Domain.DTO;

///
public record DTOProfundumEinwahlZeitraum
{
    ///
    public DTOProfundumEinwahlZeitraum(ProfundumEinwahlZeitraum dbEinwahlZeitraum)
    {
        Id = dbEinwahlZeitraum.Id;
        EinwahlStart = new DateTimeOffset(dbEinwahlZeitraum.EinwahlStart).ToLocalTime().ToString();
        EinwahlStop = new DateTimeOffset(dbEinwahlZeitraum.EinwahlStop).ToLocalTime().ToString();
    }

    /// <inheritdoc cref="ProfundumEinwahlZeitraum.Id"/>
    public Guid? Id { get; set; }

    /// <inheritdoc cref="ProfundumEinwahlZeitraum.EinwahlStart"/>
    public string? EinwahlStart { get; set; }

    /// <inheritdoc cref="ProfundumEinwahlZeitraum.EinwahlStop"/>
    public string? EinwahlStop { get; set; }
}
