using System.Diagnostics.CodeAnalysis;
using Altafraner.AfraApp.Profundum.Domain.Models;

namespace Altafraner.AfraApp.Profundum.Domain.DTO;

/// <summary>
///     A profundums category
/// </summary>
/// <remarks>
///     This is usualy used to encode which special rules apply to the profundum. For information about its contents
///     <see cref="ProfundumFachbereich" />.
/// </remarks>
public record DTOProfundumKategorie
{
    ///
    [SetsRequiredMembers]
    public DTOProfundumKategorie(ProfundumKategorie dbProfundumKategorie)
    {
        Id = dbProfundumKategorie.Id;
        Bezeichnung = dbProfundumKategorie.Bezeichnung;
        ProfilProfundum = dbProfundumKategorie.ProfilProfundum;
    }

    ///
    public DTOProfundumKategorie()
    {
    }

    /// <inheritdoc cref="ProfundumKategorie.Id"/>
    public Guid Id { get; set; }

    /// <inheritdoc cref="ProfundumKategorie.Bezeichnung" />
    public required string Bezeichnung { get; set; }

    /// <inheritdoc cref="ProfundumKategorie.ProfilProfundum" />
    public required bool ProfilProfundum { get; set; }
}
