namespace Afra_App.Profundum.Domain.DTO;

using Afra_App.Profundum.Domain.Models;

///
public record DTOProfundumEinwahlZeitraum
{
    /// <inheritdoc cref="ProfundumEinwahlZeitraum.Id"/>
    public Guid? Id { get; set; }

    /// <inheritdoc cref="ProfundumEinwahlZeitraum.EinwahlStart"/>
    public string EinwahlStart { get; set; }

    /// <inheritdoc cref="ProfundumEinwahlZeitraum.EinwahlStop"/>
    public string EinwahlStop { get; set; }
}
