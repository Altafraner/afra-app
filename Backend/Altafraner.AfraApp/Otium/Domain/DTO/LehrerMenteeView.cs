using Altafraner.AfraApp.Otium.Domain.DTO.Dashboard;
using Altafraner.AfraApp.User.Domain.DTO;

namespace Altafraner.AfraApp.Otium.Domain.DTO;

/// <summary>
/// A DTO that represents a information on a student for a teacher
/// </summary>
/// <param name="Termine">The <see cref="Week">Schultage</see> including enrollments the student should enroll for
/// </param>
/// <param name="Mentee">Information on the student himself.</param>
public record LehrerMenteeView(IAsyncEnumerable<Week> Termine, PersonInfoMinimal Mentee);
