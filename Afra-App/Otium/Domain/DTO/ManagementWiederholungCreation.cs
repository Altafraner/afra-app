using Afra_App.Otium.Domain.Models.Schuljahr;

namespace Afra_App.Otium.Domain.DTO;

/// <summary>
///     A DTO for the creation of a Wiederholung
/// </summary>
public record ManagementWiederholungCreation
{
    /// <summary>
    ///     The Id of the otium the wiederholung belongs to
    /// </summary>
    public required Guid OtiumId { get; set; }

    /// <summary>
    ///     A maximum number of concurrent enrollments for the Oium. If null, no limit is set.
    /// </summary>
    public int? MaxEinschreibungen { get; set; }

    /// <summary>
    ///     The Id of the tutor of the Otium. Could be a student or a teacher.
    /// </summary>
    public Guid? Tutor { get; set; }

    /// <summary>
    ///     The location for the Otium.
    /// </summary>
    public required string Ort { get; set; }

    /// <summary>
    ///     The number of the Block the Wiederholung is on.
    /// </summary>
    public required char Block { get; set; }

    /// <summary>
    ///     The Day of the Week that Termine of the Wiederholung are scheduled
    /// </summary>
    public required DayOfWeek Wochentag { get; set; }

    /// <summary>
    ///     The Type of Week that Termine of the Wiederholung are scheduled
    /// </summary>
    public required Wochentyp Wochentyp { get; set; }

    /// <summary>
    ///     The First Date the recurrence rule is applied to
    /// </summary>
    public required DateOnly StartDate { get; set; }

    /// <summary>
    ///     The Last Date the recurrence rule is applied to
    /// </summary>
    public required DateOnly EndDate { get; set; }
}