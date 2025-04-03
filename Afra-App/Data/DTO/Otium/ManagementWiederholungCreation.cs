using Afra_App.Data.Schuljahr;

namespace Afra_App.Data.DTO.Otium;

/// <summary>
///     A DTO for the creation of a Wiederholung
/// </summary>
public record ManagementWiederholungCreation
{
    /// <summary>
    ///     The Id of the otium the wiederholung belongs to
    /// </summary>
    public required Guid otiumId { get; set; }

    /// <summary>
    ///     A maximum number of concurrent enrollments for the Oium. If null, no limit is set.
    /// </summary>
    public int? MaxEinschreibungen { get; set; }

    /// <summary>
    ///     The Id of the tutor of the Otium. Could be a student or a teacher.
    /// </summary>
    public required Guid Tutor { get; set; }

    /// <summary>
    ///     The location for the Otium.
    /// </summary>
    public required string Ort { get; set; }

    /// <summary>
    ///     The number of the Block the Wiederholung is on.
    /// </summary>
    public required sbyte Block { get; set; }

    /// <summary>
    ///     The Day of the Week that Termine of the Wiederholung are scheduled
    /// </summary>
    public required DayOfWeek Wochentag { get; set; }

    /// <summary>
    ///     The Type of Week that Termine of the Wiederholung are scheduled
    /// </summary>
    public required Wochentyp Wochentyp { get; set; }

    /// <summary>
    ///     The First Date the recurrency rule is applied to
    /// </summary>
    public required DateOnly startDate { get; set; }

    /// <summary>
    ///     The Last Date the recurrency rule is applied to
    /// </summary>
    public required DateOnly endDate { get; set; }
}