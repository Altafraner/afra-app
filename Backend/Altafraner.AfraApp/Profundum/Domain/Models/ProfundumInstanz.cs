using System.ComponentModel.DataAnnotations;
using Altafraner.AfraApp.User.Domain.Models;
using Altafraner.Backbone.Utils;

namespace Altafraner.AfraApp.Profundum.Domain.Models;

/// <summary>
///     A db record representing a Profundum instance.
/// </summary>
public class ProfundumInstanz : IHasTimestamps, IHasUserTracking
{
    /// <summary>
    ///     The unique identifier of the instance
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    ///     the profundum this instance is part of
    /// </summary>
    public required ProfundumDefinition Profundum { get; set; }

    /// <summary>
    ///     the slots this instance happens in
    /// </summary>
    public List<ProfundumSlot> Slots { get; set; } = null!;

    /// <summary>
    ///     the max amount of enrollments for this profundum
    /// </summary>
    public int? MaxEinschreibungen { get; set; } = null;

    /// <summary>
    ///     all enrollments into this instance
    /// </summary>
    /// <remarks>Enrollments might be partial, be careful</remarks>
    public List<ProfundumEinschreibung> Einschreibungen { get; set; } = null!;

    /// <summary>
    /// The persons responsible for the profundum
    /// </summary>
    public List<Person> Verantwortliche { get; set; } = null!;

    /// <summary>
    ///     the physical location this profundum happens at
    /// </summary>
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
