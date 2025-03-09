using Afra_App.Data.TimeInterval;

namespace Afra_App.Data.DTO.Otium;

/// <summary>
/// A DTO for an enrollment
/// </summary>
public record Einschreibung : IMinimalTermin
{
    /// <summary>
    /// The time interval for the enrollment
    /// </summary>
    public ITimeInterval<TimeOnly> Interval { get; set; }

    /// <inheritdoc />
    public Guid Id { get; set; }

    /// <inheritdoc />
    public string Otium { get; set; }

    /// <inheritdoc />
    public string Ort { get; set; }

    /// <inheritdoc />
    public PersonInfoMinimal Tutor { get; set; }

    /// <summary>
    /// Creates a new DTO from a einschreibung entry
    /// </summary>
    /// <param name="einschreibung"></param>
    public Einschreibung(Data.Otium.Einschreibung einschreibung)
    {
        Id = einschreibung.Id;
        Interval = einschreibung.Interval;
        Otium = einschreibung.Termin.Otium.Bezeichnung;
        Ort = einschreibung.Termin.Ort;
        Tutor = new PersonInfoMinimal(einschreibung.Termin.Tutor);
    }
}