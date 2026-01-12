using System.ComponentModel.DataAnnotations;

namespace Altafraner.AfraApp.Profundum.Domain.Models;

/// <summary>
///     The departement this profundum is part of.
/// </summary>
public class ProfundumFachbereich
{
    /// <summary>
    ///     The unique id of the fachbereich
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    ///     The human-redable label of the fachbereich
    /// </summary>
    [MaxLength(100)]
    public required string Label { get; set; }

    /// <summary>
    ///     A list of all profunda that are part of this departement.
    /// </summary>
    public List<ProfundumDefinition> Profunda { get; set; } = [];
}
