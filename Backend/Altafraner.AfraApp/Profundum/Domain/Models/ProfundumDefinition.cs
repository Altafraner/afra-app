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

    /// <summary>
    ///     the profundums category
    /// </summary>
    /// <remarks>
    ///     the category is mostly used to choose which set of rules appies to the profundum. For information about it's
    ///     subject area, see <see cref="Fachbereiche" />.
    /// </remarks>
    public required ProfundumKategorie Kategorie { get; set; }

    /// <summary>
    ///     The departements this profundum is part of.
    /// </summary>
    public List<ProfundumFachbereich> Fachbereiche { get; set; } = null!;

    /// <summary>
    ///     all instances for this profundum
    /// </summary>
    public List<ProfundumInstanz> Instanzen { get; set; } = null!;

    /// <summary>
    ///     the minimum grade level needed to enroll to this profundum
    /// </summary>
    public int? MinKlasse { get; set; }

    /// <summary>
    ///     the maximum grade level allowed to enroll to this profundum
    /// </summary>
    public int? MaxKlasse { get; set; }

    /// <summary>
    ///     profunda that a student has to enroll in before enrolling to this profundum.
    /// </summary>
    public List<ProfundumDefinition> Dependencies { get; set; } = null!;

    /// <summary>
    ///     profunda that list this profundum as dependency.
    /// </summary>
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
