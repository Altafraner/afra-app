using Altafraner.AfraApp.User.Domain.DTO;

namespace Altafraner.AfraApp.Profundum.Domain.DTO;

/// <summary>
///     Contains the set of information to describe a students enrollment status and the data it was generated from.
/// </summary>
public record struct DTOProfundumEnrollmentSet
{
    /// <summary>
    ///     The person this information is about
    /// </summary>
    public required PersonInfoMinimal Person { get; set; }

    /// <summary>
    ///     The students enrollments
    /// </summary>
    public required IEnumerable<DTOProfundumEnrollment> Enrollments { get; set; }

    /// <summary>
    ///     The students enrollment preferences
    /// </summary>
    public required IEnumerable<DTOWunsch> Wuensche { get; set; }

    /// <summary>
    ///     Warnings about the students status of enrollment
    /// </summary>
    public required IEnumerable<MatchingWarning> Warnings { get; set; }
}
