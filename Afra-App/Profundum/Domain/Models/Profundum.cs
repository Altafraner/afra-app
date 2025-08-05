using System.ComponentModel.DataAnnotations;

namespace Afra_App.Profundum.Domain.Models;

/// <summary>
///     A db record representing a Profundum.
/// </summary>
public class Profundum
{
    /// <summary>
    ///     A unique identifier for the Profundum
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    ///     A name for the Profundum
    /// </summary>
    [MaxLength(50)]
    public required string Bezeichnung { get; set; }

    ///
    public required ProfundumKategorie Kategorie { get; set; }

    ///
    public ICollection<ProfundumInstanz> Instanzen { get; set; } = [];

    ///
    public int? minKlasse { get; set; } = null;
    ///
    public int? maxKlasse { get; set; } = null;
}
