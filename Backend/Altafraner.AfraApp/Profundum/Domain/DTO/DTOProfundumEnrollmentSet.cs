using Altafraner.AfraApp.User.Domain.DTO;

namespace Altafraner.AfraApp.Profundum.Domain.DTO;

///
public record struct DTOProfundumEnrollmentSet
{
    ///
    public required PersonInfoMinimal Person { get; set; }

    ///
    public required IEnumerable<DTOProfundumEnrollment> Enrollments { get; set; }

    ///
    public required IEnumerable<DTOWunsch> Wuensche { get; set; }

    ///
    public required IEnumerable<string> Warnings { get; set; }

}

///
public record struct DTOWunsch(Guid Id, IEnumerable<Guid> SlotId, int Rang);
