using Afra_App.Data.TimeInterval;

namespace Afra_App.Data.DTO.Otium;

/// <summary>
///     A DTO for an enrollment
/// </summary>
public record Einschreibung : IMinimalTermin
{
    /// <summary>
    ///     Creates a new DTO from a einschreibung entry
    /// </summary>
    /// <param name="einschreibung"></param>
    public Einschreibung(Data.Otium.Einschreibung einschreibung)
    {
        Id = einschreibung.Id;
        TerminId = einschreibung.Termin.Id;
        Interval = einschreibung.Interval;
        Otium = einschreibung.Termin.Otium.Bezeichnung;
        Ort = einschreibung.Termin.Ort;
        Tutor = einschreibung.Termin.Tutor is not null ? new PersonInfoMinimal(einschreibung.Termin.Tutor) : null;
    }

    /// <summary>
    ///     The time interval for the enrollment
    /// </summary>
    public ITimeInterval<TimeOnly> Interval { get; set; }

    /// <summary>
    ///     The ID of the termin the enrollment is for
    /// </summary>
    public Guid TerminId { get; set; }

    /// <summary>
    ///     The ID of the enrollment
    /// </summary>
    public Guid Id { get; set; }

    /// <inheritdoc />
    public string Otium { get; set; }

    /// <inheritdoc />
    public string Ort { get; set; }

    /// <inheritdoc />
    public PersonInfoMinimal? Tutor { get; set; }
}