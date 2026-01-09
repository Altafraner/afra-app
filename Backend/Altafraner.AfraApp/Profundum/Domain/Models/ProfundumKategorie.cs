using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Altafraner.AfraApp.Profundum.Domain.Models;

/// <summary>
///     A Kategorie for an Profundum.
/// </summary>
public class ProfundumKategorie
{
    /// <summary>
    ///     A unique identifier for the Kategorie.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    ///     A name for the Kategorie.
    /// </summary>
    [MaxLength(50)]
    public required string Bezeichnung { get; set; }

    /// <summary>
    /// A helper collection for navigating to all Profunda with this Kategorie.
    /// </summary>
    [JsonIgnore]
    public ICollection<ProfundumDefinition> Profunda { get; init; } = new List<ProfundumDefinition>();

    /// <summary>
    /// A boolean indicating that Profunda of this Kategorie correspond to Profilprofunda
    /// </summary>
    public required bool ProfilProfundum { get; set; }
}
