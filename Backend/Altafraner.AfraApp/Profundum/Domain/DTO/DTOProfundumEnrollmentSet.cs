namespace Altafraner.AfraApp.Profundum.Domain.DTO;

using Altafraner.AfraApp.User.Domain.DTO;

///
public record DTOProfundumEnrollmentSet
{
    ///
    public required PersonInfoMinimal Person { get; set; }
    ///
    public required IEnumerable<DTOProfundumEnrollment?> Enrollments { get; set; }

    ///
    public record DTOWunsch(Guid id, int rang);
    ///
    public required IEnumerable<DTOWunsch> Wuensche { get; set; }

    ///
    public required IEnumerable<string> Warnings { get; set; }
}
