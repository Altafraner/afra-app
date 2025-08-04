using Afra_App.User.Domain.DTO;

namespace Afra_App.Otium.Domain.DTO;

/// <summary>
/// A DTO that represents an overview of the teachers schedule and his mentees.
/// </summary>
/// <param name="Termine">The overview over the teachers schedule</param>
/// <param name="Mentees">The overview over the teachers mentee</param>
public record LehrerUebersicht(IEnumerable<LehrerTerminPreview> Termine, IEnumerable<MenteePreview> Mentees);

/// <summary>
/// Represents a preview of a <see cref="Models.Termin"/> the teacher is tutoring.
/// </summary>
/// <param name="Id">The id of the termin</param>
/// <param name="Otium">The name of the Otium the termin is for</param>
/// <param name="Ort">The location the termin is happening on</param>
/// <param name="Auslastung">The calculated load factor of the termin.</param>
/// <param name="Datum">The date the termin is happening on</param>
/// <param name="Block">The block the termin is happening in</param>
public record LehrerTerminPreview(Guid Id, string Otium, string Ort, int? Auslastung, DateOnly Datum, char Block);

/// <summary>
/// Represents the preview of a student as seen by his mentor.
/// </summary>
/// <param name="Mentee">Information on the mentee himself</param>
/// <param name="LetzteWoche">The status of the mentees enrollments in the past week</param>
/// <param name="DieseWoche">The status of the mentees enrollments in the current week</param>
/// <param name="NächsteWoche">The status of the mentees enrollments in the next week</param>
public record MenteePreview(
    PersonInfoMinimal Mentee,
    MenteePreviewStatus LetzteWoche,
    MenteePreviewStatus DieseWoche,
    MenteePreviewStatus NächsteWoche);

/// <summary>
/// An enum that represents the status of a student on any given day.
/// </summary>
public enum MenteePreviewStatus
{
    /// <summary>
    /// The students enrollments are okay and in case the day is in the past, the student was present.
    /// </summary>
    Okay,

    /// <summary>
    /// Either the student was absent or the enrollments are not okay.
    /// </summary>
    Auffaellig
}
