namespace Altafraner.AfraApp.Profundum.Domain.DTO;

using System.Diagnostics.CodeAnalysis;
using Altafraner.AfraApp.Profundum.Domain.Models;

///
public record DTOProfundumEnrollment
{
    ///
    [SetsRequiredMembers]
    public DTOProfundumEnrollment(ProfundumEinschreibung dbEnrollment)
    {
        IsFixed = dbEnrollment.IsFixed;
        BetroffenePersonId = dbEnrollment.BetroffenePersonId;
        ProfundumInstanzId = dbEnrollment.ProfundumInstanzId;
    }

    ///
    public bool IsFixed { get; set; }

    ///
    public required Guid BetroffenePersonId { get; set; }

    ///
    public required Guid ProfundumInstanzId { get; set; }
}
