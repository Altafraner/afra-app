using System.Diagnostics.CodeAnalysis;
using Altafraner.AfraApp.Profundum.Domain.Models;

namespace Altafraner.AfraApp.Profundum.Domain.DTO;

///
public record DtoProfundumKategorie
{
    /// <summary>
    ///     Initializes a dto from the db model
    /// </summary>
    /// <param name="kategorie"></param>
    [SetsRequiredMembers]
    public DtoProfundumKategorie(ProfundumKategorie kategorie)
    {
        Id = kategorie.Id;
        Bezeichnung = kategorie.Bezeichnung;
        ProfilProfundum = kategorie.ProfilProfundum;
        MaxProEinwahl = kategorie.MaxProEinwahl;
    }

    ///
    public DtoProfundumKategorie()
    {
    }

    /// <inheritdoc cref="ProfundumKategorie.Id" />
    public Guid Id { get; set; }

    /// <inheritdoc cref="ProfundumKategorie.Bezeichnung" />
    public required string Bezeichnung { get; set; }

    /// <inheritdoc cref="ProfundumKategorie.ProfilProfundum" />
    public bool ProfilProfundum { get; set; }

    /// <inheritdoc cref="ProfundumKategorie.MaxProEinwahl" />
    public int? MaxProEinwahl { get; set; }
}
