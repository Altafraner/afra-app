namespace Altafraner.AfraApp.Profundum.Domain.DTO;

using Models;

///
public record DTOProfundumDefinition
{
    /// <inheritdoc cref="ProfundumDefinition.Id"/>
    public Guid Id { get; set; }

    /// <inheritdoc cref="ProfundumDefinition.Bezeichnung"/>
    public required string Bezeichnung { get; set; }

    /// <inheritdoc cref="ProfundumDefinition.Kategorie"/>
    public Guid KategorieId { get; set; }

    /// <inheritdoc cref="ProfundumDefinition.minKlasse"/>
    public int? minKlasse { get; set; } = null;
    /// <inheritdoc cref="ProfundumDefinition.maxKlasse"/>
    public int? maxKlasse { get; set; } = null;
}
