using Altafraner.AfraApp.Profundum.Domain.Models;

namespace Altafraner.AfraApp.Profundum.Domain.DTO;

///
public record DTOProfundumDefinitionCreation
{
    /// <inheritdoc cref="ProfundumDefinition.Id"/>
    public Guid Id { get; set; }

    /// <inheritdoc cref="ProfundumDefinition.Bezeichnung"/>
    public required string Bezeichnung { get; set; }

    /// <inheritdoc cref="ProfundumDefinition.Beschreibung"/>
    public required string Beschreibung { get; set; }

    /// <inheritdoc cref="ProfundumDefinition.Kategorie"/>
    public Guid KategorieId { get; set; }

    /// <inheritdoc cref="ProfundumDefinition.Verantwortliche"/>
    public ICollection<Guid> VerantwortlicheIds { get; set; } = [];

    /// <inheritdoc cref="ProfundumDefinition.MinKlasse"/>
    public int? minKlasse { get; set; } = null;
    /// <inheritdoc cref="ProfundumDefinition.MaxKlasse"/>
    public int? maxKlasse { get; set; } = null;
}
