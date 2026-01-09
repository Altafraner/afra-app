using Altafraner.AfraApp.Profundum.Domain.Models;

namespace Altafraner.AfraApp.Profundum.Domain.DTO;

///
public record DTOProfundumKategorie
{
    ///
    public DTOProfundumKategorie(ProfundumKategorie dbProfundumKategorie)
    {
        Id = dbProfundumKategorie.Id;
        Bezeichnung = dbProfundumKategorie.Bezeichnung;
        ProfilProfundum = dbProfundumKategorie.ProfilProfundum;
    }

    /// <inheritdoc cref="ProfundumKategorie.Id"/>
    public Guid Id { get; set; }

    /// <inheritdoc cref="ProfundumKategorie.Bezeichnung"/>
    public string Bezeichnung { get; set; }

    /// <inheritdoc cref="ProfundumKategorie.ProfilProfundum"/>
    public bool ProfilProfundum { get; set; }
}
