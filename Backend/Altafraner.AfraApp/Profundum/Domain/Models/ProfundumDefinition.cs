using System.ComponentModel.DataAnnotations;

namespace Altafraner.AfraApp.Profundum.Domain.Models;

/// <summary>
///     A db record representing a Profundum.
/// </summary>
public class ProfundumDefinition
{
    /// <summary>
    ///     A unique identifier for the Profundum
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    ///     A name for the Profundum
    /// </summary>
    [MaxLength(100)]
    public required string Bezeichnung { get; set; }

    ///
    public required ProfundumKategorie Kategorie { get; set; }

    ///
    public ICollection<ProfundumInstanz> Instanzen { get; set; } = [];

    ///
    public int? MinKlasse { get; set; } = null;
    ///
    public int? MaxKlasse { get; set; } = null;
}
