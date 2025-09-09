namespace Afra_App.Profundum.Domain.DTO;

using Afra_App.Profundum.Domain.Models;

///
public record DTOProfundumKategorie
{
    ///
    public DTOProfundumKategorie(ProfundumKategorie dbProfundumKategorie)
    {
        Id = dbProfundumKategorie.Id;
        Bezeichnung = dbProfundumKategorie.Bezeichnung;
        ProfilProfundum = dbProfundumKategorie.ProfilProfundum;
        MaxProEinwahl = dbProfundumKategorie.MaxProEinwahl;
    }

    /// <inheritdoc cref="ProfundumKategorie.Id"/>
    public Guid Id { get; set; }

    /// <inheritdoc cref="ProfundumKategorie.Bezeichnung"/>
    public string Bezeichnung { get; set; }

    /// <inheritdoc cref="ProfundumKategorie.ProfilProfundum"/>
    public bool ProfilProfundum { get; set; }

    /// <inheritdoc cref="ProfundumKategorie.MaxProEinwahl"/>
    public int? MaxProEinwahl { get; set; }
}
