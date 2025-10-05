namespace Altafraner.AfraApp.Otium.Domain.DTO;

/// <summary>
///     A DTO for editing a Wiederholung
/// </summary>
public record ManagementWiederholungEdit
{
    /// <summary>
    ///     A maximum number of concurrent enrollments for the Oium. If null, no limit is set.
    /// </summary>
    public int? MaxEinschreibungen { get; set; }

    /// <summary>
    ///     The location for the Otium.
    /// </summary>
    public required string Ort { get; set; }
}
