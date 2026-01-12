using Altafraner.AfraApp.User.Domain.Models;
using Altafraner.Backbone.Utils;

namespace Altafraner.AfraApp.Profundum.Domain.Models;

/// <summary>
///     A record representing an enrollment for a <see cref="ProfundumDefinition" />.
/// </summary>
public class ProfundumEinschreibung : IHasTimestamps
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

    ///
    protected internal Guid BetroffenePersonId { get; set; }

    ///
    public required ProfundumInstanz? ProfundumInstanz { get; set; }

    ///
    protected internal Guid? ProfundumInstanzId { get; set; }

    ///
    public required ProfundumSlot Slot { get; set; }

    ///
    protected internal Guid SlotId { get; set; }

    /// <inheritdoc/>
    public DateTime CreatedAt { get; set; }

    /// <inheritdoc/>
    public DateTime LastModified { get; set; }
}
