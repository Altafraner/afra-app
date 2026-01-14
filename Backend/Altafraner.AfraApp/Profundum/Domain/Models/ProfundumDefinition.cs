using System.ComponentModel.DataAnnotations;
using Altafraner.Backbone.Utils;

namespace Altafraner.AfraApp.Profundum.Domain.Models;

/// <summary>
///     A db record representing a Profundum.
/// </summary>
public class ProfundumDefinition : IHasTimestamps, IHasUserTracking
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
    public List<ProfundumFachbereich> Fachbereiche { get; set; } = null!;

    ///
    public List<ProfundumInstanz> Instanzen { get; set; } = null!;

    ///
    public int? MinKlasse { get; set; } = null;
    ///
    public int? MaxKlasse { get; set; } = null;

    ///
    public List<ProfundumDefinition> Dependencies { get; set; } = null!;
    ///
    public List<ProfundumDefinition> Dependants { get; set; } = null!;

    /// <inheritdoc/>
    public DateTime CreatedAt { get; set; }

    /// <inheritdoc/>
    public DateTime LastModified { get; set; }

    /// <inheritdoc/>
    public Guid? CreatedById { get; set; }

    /// <inheritdoc/>
    public Guid? LastModifiedById { get; set; }
}
