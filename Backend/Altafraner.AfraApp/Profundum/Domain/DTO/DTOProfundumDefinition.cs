namespace Altafraner.AfraApp.Profundum.Domain.DTO;

using Models;

///
public record DTOProfundumDefinition
{
    ///
    public DTOProfundumDefinition(ProfundumDefinition dbProfundumDefinition)
    {
        Id = dbProfundumDefinition.Id;
        Bezeichnung = dbProfundumDefinition.Bezeichnung;
        Beschreibung = dbProfundumDefinition.Beschreibung;
        KategorieId = dbProfundumDefinition.Kategorie.Id;
        MinKlasse = dbProfundumDefinition.MinKlasse;
        MaxKlasse = dbProfundumDefinition.MaxKlasse;
        DependencyIds = dbProfundumDefinition.Dependencies.Select(d => d.Id).ToArray();
    }

    /// <inheritdoc cref="ProfundumDefinition.Id"/>
    public Guid Id { get; set; }

    /// <inheritdoc cref="ProfundumDefinition.Bezeichnung"/>
    public string Bezeichnung { get; set; }

    /// <inheritdoc cref="ProfundumDefinition.Beschreibung"/>
    public string Beschreibung { get; set; }

    /// <inheritdoc cref="ProfundumDefinition.Kategorie"/>
    public Guid KategorieId { get; set; }

    /// <inheritdoc cref="ProfundumDefinition.MinKlasse"/>
    public int? MinKlasse { get; set; }

    /// <inheritdoc cref="ProfundumDefinition.MaxKlasse"/>
    public int? MaxKlasse { get; set; }

    /// <inheritdoc cref="ProfundumDefinition.Dependencies"/>
    public ICollection<Guid> DependencyIds { get; set; } = [];
}
