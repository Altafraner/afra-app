using Altafraner.AfraApp.Profundum.Domain.Models;

namespace Altafraner.AfraApp.Profundum.Domain.DTO;

///
public record DtoProfundumDefinition
{
    /// <inheritdoc cref="ProfundumDefinition.Id"/>
    public Guid Id { get; set; }

    /// <inheritdoc cref="ProfundumDefinition.Bezeichnung"/>
    public required string Bezeichnung { get; set; }

    /// <inheritdoc cref="ProfundumDefinition.Kategorie"/>
    public Guid KategorieId { get; set; }

    /// <inheritdoc cref="ProfundumDefinition.MinKlasse" />
    public int? MinKlasse { get; set; } = null;

    /// <inheritdoc cref="ProfundumDefinition.MaxKlasse" />
    public int? MaxKlasse { get; set; } = null;
}
