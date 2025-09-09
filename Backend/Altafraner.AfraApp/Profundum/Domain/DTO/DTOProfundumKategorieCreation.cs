using Altafraner.AfraApp.Profundum.Domain.Models;

namespace Altafraner.AfraApp.Profundum.Domain.DTO;

///
public record DTOProfundumKategorieCreation
{
    /// <inheritdoc cref="ProfundumKategorie.Bezeichnung"/>
    public required string Bezeichnung { get; set; }

    /// <inheritdoc cref="ProfundumKategorie.ProfilProfundum"/>
    public bool ProfilProfundum { get; set; }

    /// <inheritdoc cref="ProfundumKategorie.MaxProEinwahl"/>
    public int? MaxProEinwahl { get; set; }
}
