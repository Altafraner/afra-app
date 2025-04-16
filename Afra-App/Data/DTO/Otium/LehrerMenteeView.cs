using Afra_App.Data.DTO.Otium.Dashboard;

namespace Afra_App.Data.DTO.Otium;

/// <summary>
/// A DTO that represents a information on a student for a teacher
/// </summary>
/// <param name="Termine">The <see cref="Tag">Schultage</see> including enrollments the student should enroll for
/// </param>
/// <param name="Mentee">Information on the student himself.</param>
public record LehrerMenteeView(IAsyncEnumerable<Tag> Termine, PersonInfoMinimal Mentee);
