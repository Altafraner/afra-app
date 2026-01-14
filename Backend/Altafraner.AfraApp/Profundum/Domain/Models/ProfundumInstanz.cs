using System.ComponentModel.DataAnnotations;
using Altafraner.AfraApp.User.Domain.Models;
using Altafraner.Backbone.Utils;

namespace Altafraner.AfraApp.Profundum.Domain.Models;

/// <summary>
///     A db record representing a Profundum instance.
/// </summary>
public class ProfundumInstanz : IHasTimestamps, IHasUserTracking
{
    ///
    public Guid Id { get; set; }

    ///
    public required ProfundumDefinition Profundum { get; set; }

    ///
    public List<ProfundumSlot> Slots { get; set; } = null!;

    ///
    public int? MaxEinschreibungen { get; set; } = null;

    ///
    public List<ProfundumEinschreibung> Einschreibungen { get; set; } = null!;

    /// <summary>
    /// The persons responsible for the profundum
    /// </summary>
    public List<Person> Verantwortliche { get; set; } = null!;

    ///
    [MaxLength(20)]
    public required string Ort { get; set; }

    /// <inheritdoc/>
    public DateTime CreatedAt { get; set; }

    /// <inheritdoc/>
    public DateTime LastModified { get; set; }

    /// <inheritdoc/>
    public Guid? CreatedById { get; set; }

    /// <inheritdoc/>
    public Guid? LastModifiedById { get; set; }
}
