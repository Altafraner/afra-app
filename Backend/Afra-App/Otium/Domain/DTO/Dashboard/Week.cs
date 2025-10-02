namespace Altafraner.AfraApp.Otium.Domain.DTO.Dashboard;

/// <summary>
///     A DTO for a day in the otium calendar
/// </summary>
/// <param name="Monday">The Date the DTO is for</param>
/// <param name="Message">A message containing complementary information on the users status for that day</param>
/// <param name="Einschreibungen">All enrollments for the day.</param>
public record Week(
    DateOnly Monday,
    string Message,
    IEnumerable<Einschreibung> Einschreibungen);
