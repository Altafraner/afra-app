using Altafraner.AfraApp.Profundum.Domain.Models;

namespace Altafraner.AfraApp.Profundum.Domain.DTO;

/// <summary>
///     A dto representing a grouping of slots so that their enrollment process is done together
/// </summary>
public record DTOProfundumEinwahlZeitraum
{
    ///
    public DTOProfundumEinwahlZeitraum(ProfundumEinwahlZeitraum dbEinwahlZeitraum)
    {
        Id = dbEinwahlZeitraum.Id;
        EinwahlStart = new DateTimeOffset(dbEinwahlZeitraum.EinwahlStart).ToLocalTime();
        EinwahlStop = new DateTimeOffset(dbEinwahlZeitraum.EinwahlStop).ToLocalTime();
        Bezeichnung = dbEinwahlZeitraum.Bezeichnung;
    }

    /// <inheritdoc cref="ProfundumEinwahlZeitraum.Id"/>
    public Guid? Id { get; set; }

    /// <inheritdoc cref="ProfundumEinwahlZeitraum.EinwahlStart"/>
    public DateTimeOffset EinwahlStart { get; set; }

    /// <inheritdoc cref="ProfundumEinwahlZeitraum.EinwahlStop"/>
    public DateTimeOffset EinwahlStop { get; set; }

    /// <inheritdoc cref="ProfundumEinwahlZeitraum.Bezeichnung"/>
    public string Bezeichnung { get; set; } = "";
}
