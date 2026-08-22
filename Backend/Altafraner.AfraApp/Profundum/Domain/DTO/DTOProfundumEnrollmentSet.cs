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
    public required Dictionary<string, List<DtoProfundumWunsch>> Wuensche { get; set; }

    /// <summary>
    ///     Warnings about the students status of enrollment
    /// </summary>
    public required IEnumerable<MatchingWarning> Warnings { get; set; }

    /// <summary>
    ///     The students confirmed team-partner pairings, if any - surfaced here so staff manually overriding an
    ///     enrollment (see <c>Matching.vue</c>) can see and coordinate the partner's placement too. A pairing is a
    ///     hard solver constraint during automatic matching, but a manual override bypasses the solver entirely, so
    ///     it's easy to silently desync one without the other.
    /// </summary>
    public required IEnumerable<DTOProfundumPartnerWunschStaff> Partnerschaften { get; set; }

    /// <summary>
    ///     Free-text supplementary information (Auslandsaufenthalt, Lernvertrag) the student provided for the
    ///     current Einwahlzeitraum, opaque to the backend. Empty if the student hasn't answered or had nothing to
    ///     report.
    /// </summary>
    public required string ZusatzInformation { get; set; }
}
