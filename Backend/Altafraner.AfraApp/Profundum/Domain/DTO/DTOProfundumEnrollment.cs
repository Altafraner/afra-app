using System.Diagnostics.CodeAnalysis;
using Altafraner.AfraApp.Profundum.Domain.Models;

namespace Altafraner.AfraApp.Profundum.Domain.DTO;

/// <summary>
///     A dto containing information about a students enrollment to a profundum
/// </summary>
public record struct DTOProfundumEnrollment
{
    ///
    public DTOProfundumEnrollment()
    {
    }

    ///
    [SetsRequiredMembers]
    public DTOProfundumEnrollment(ProfundumEinschreibung dbEnrollment)
    {
        IsFixed = dbEnrollment.IsFixed;
        ProfundumSlotId = dbEnrollment.SlotId;
        ProfundumInstanzId = dbEnrollment.ProfundumInstanzId;
    }

    /// <summary>
    ///     If set, the students enrollment to this instance is fixed
    /// </summary>
    public bool IsFixed { get; set; }

    /// <summary>
    ///     The slot this enrollment is for
    /// </summary>
    public required Guid ProfundumSlotId { get; set; }

    /// <summary>
    ///     The profundum instance this enrollment is for
    /// </summary>
    public required Guid? ProfundumInstanzId { get; set; }
}
