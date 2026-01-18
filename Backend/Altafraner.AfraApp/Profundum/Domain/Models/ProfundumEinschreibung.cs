using Altafraner.AfraApp.User.Domain.Models;
using Altafraner.Backbone.Utils;

namespace Altafraner.AfraApp.Profundum.Domain.Models;

/// <summary>
///     A record representing an enrollment for a <see cref="ProfundumDefinition" />.
/// </summary>
public class ProfundumEinschreibung : IHasTimestamps, IHasUserTracking
{
    /// <summary>
    ///     False iff the Enrollment is preliminary and can be safely overwritten by matching functions
    ///     True iff the Enrollment is part of a manual constraint or has been finalized
    /// </summary>
    public bool IsFixed { get; set; }

    /// <summary>
    ///     A reference to the person affected by the enrollment.
    /// </summary>
    public required Person BetroffenePerson { get; set; }

    /// <summary>
    ///     the id of the person affected by this enrollment
    /// </summary>
    protected internal Guid BetroffenePersonId { get; set; }

    /// <summary>
    ///     the profundum instance the student is enrolled in
    /// </summary>
    public required ProfundumInstanz? ProfundumInstanz { get; set; }

    /// <summary>
    ///     the id of <see cref="ProfundumInstanz" />.
    /// </summary>
    protected internal Guid? ProfundumInstanzId { get; set; }

    /// <summary>
    ///     The slot this enrollment is for
    /// </summary>
    /// <remarks>be careful as a student might be only partially enrolled to a multi slot profundum instance</remarks>
    public required ProfundumSlot Slot { get; set; }

    /// <summary>
    ///     the id of <see cref="Slot" />.
    /// </summary>
    protected internal Guid SlotId { get; set; }

    /// <inheritdoc/>
    public DateTime CreatedAt { get; set; }

    /// <inheritdoc/>
    public DateTime LastModified { get; set; }

    /// <inheritdoc/>
    public Guid? CreatedById { get; set; }

    /// <inheritdoc/>
    public Guid? LastModifiedById { get; set; }
}
