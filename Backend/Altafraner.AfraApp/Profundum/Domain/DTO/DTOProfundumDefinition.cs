namespace Altafraner.AfraApp.Profundum.Domain.DTO;

using Altafraner.AfraApp.Profundum.Domain.Models;
using Altafraner.AfraApp.User.Domain.DTO;

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
        VerantwortlicheIds = dbProfundumDefinition.Verantwortliche.Select(s => s.Id).ToArray();
        VerantwortlicheInfo = dbProfundumDefinition.Verantwortliche.Select(s => new PersonInfoMinimal(s)).ToArray();
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

    /// <inheritdoc cref="ProfundumDefinition.Verantwortliche"/>
    public ICollection<Guid> VerantwortlicheIds { get; set; } = [];

    /// <inheritdoc cref="ProfundumDefinition.Verantwortliche"/>
    public ICollection<PersonInfoMinimal> VerantwortlicheInfo { get; set; } = [];

    /// <inheritdoc cref="ProfundumDefinition.MinKlasse"/>
    public int? MinKlasse { get; set; } = null;
    /// <inheritdoc cref="ProfundumDefinition.MaxKlasse"/>
    public int? MaxKlasse { get; set; } = null;

    /// <inheritdoc cref="ProfundumDefinition.Dependencies"/>
    public ICollection<Guid> DependencyIds { get; set; } = [];
}
