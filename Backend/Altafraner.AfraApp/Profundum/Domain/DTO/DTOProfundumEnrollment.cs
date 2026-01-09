using System.Diagnostics.CodeAnalysis;
using Altafraner.AfraApp.Profundum.Domain.Models;

namespace Altafraner.AfraApp.Profundum.Domain.DTO;

///
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

    ///
    public bool IsFixed { get; set; }

    ///
    public required Guid ProfundumSlotId { get; set; }

    ///
    public required Guid? ProfundumInstanzId { get; set; }
}
