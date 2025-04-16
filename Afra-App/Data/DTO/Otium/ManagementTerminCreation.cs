namespace Afra_App.Data.DTO.Otium;

/// <summary>
///     A DTO for the creation of a Termin
/// </summary>
public record ManagementTerminCreation
{
    /// <summary>
    ///     The number of the Block the Termin is on.
    /// </summary>
    public required sbyte Block { get; set; }

    /// <summary>
    ///     The Id of the otium the termin belongs to
    /// </summary>
    public required Guid OtiumId { get; set; }

    /// <summary>
    ///     The date the Termin is on.
    /// </summary>
    public required DateOnly Datum { get; set; }

    /// <summary>
    ///     A maximum number of concurrent enrollments for the Termin. If null, no limit is set.
    /// </summary>
    public int? MaxEinschreibungen { get; set; }

    /// <summary>
    ///     The Id of the tutor of the Otium. Could be a student or a teacher.
    /// </summary>
    public required Guid? Tutor { get; set; }

    /// <summary>
    ///     The location for the Otium.
    /// </summary>
    public required string Ort { get; set; }
}
