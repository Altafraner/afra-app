namespace Altafraner.AfraApp.Profundum.Domain.DTO;

using Altafraner.AfraApp.Profundum.Domain.Models;

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
        minKlasse = dbProfundumDefinition.MinKlasse;
        maxKlasse = dbProfundumDefinition.MaxKlasse;
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
    public int? minKlasse { get; set; } = null;
    /// <inheritdoc cref="ProfundumDefinition.MaxKlasse"/>
    public int? maxKlasse { get; set; } = null;
}
