using System.ComponentModel.DataAnnotations;
using Altafraner.AfraApp.User.Domain.Models;

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

    /// <summary>
    ///     A description for the Profundum
    /// </summary>
    [MaxLength(2000)]
    public required string Beschreibung { get; set; }

    ///
    public required ProfundumKategorie Kategorie { get; set; }

    /// <summary>
    ///     The departements this profundum is part of.
    /// </summary>
    public List<ProfundumFachbereich> Fachbereiche { get; set; }

    ///
    public ICollection<ProfundumInstanz> Instanzen { get; set; } = [];

    ///
    public int? MinKlasse { get; set; } = null;
    ///
    public int? MaxKlasse { get; set; } = null;

    ///
    public ICollection<ProfundumDefinition> Dependencies { get; set; } = [];
    ///
    public ICollection<ProfundumDefinition> Dependants { get; set; } = [];
}
