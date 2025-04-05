namespace Afra_App.Data.DTO.Otium.Dashboard;

/// <summary>
///     A DTO for a day in the otium calendar
/// </summary>
/// <param name="Datum">The Date the DTO is for</param>
/// <param name="Vollstaendig">Whether the user has enrolled in Sub-Slots for the day</param>
/// <param name="KategorienErfuellt">Whether the user has enrolled in all required Kategories for a week</param>
/// <param name="Einschreibungen">All enrollments for the day.</param>
public record Tag(
    DateOnly Datum,
    bool Vollstaendig,
    bool KategorienErfuellt,
    IEnumerable<Einschreibung> Einschreibungen);