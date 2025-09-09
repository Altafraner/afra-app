namespace Afra_App.Profundum.Domain.DTO;

using Afra_App.Profundum.Domain.Models;

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
