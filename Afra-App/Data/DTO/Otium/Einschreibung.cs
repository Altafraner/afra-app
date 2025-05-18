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
        Otium = einschreibung.Termin.Otium.Bezeichnung;
        OtiumId = einschreibung.Termin.Otium.Id;
        Ort = einschreibung.Termin.Ort;
        Block = einschreibung.Termin.Block.SchemaId;
        Tutor = einschreibung.Termin.Tutor is not null ? new PersonInfoMinimal(einschreibung.Termin.Tutor) : null;
        Interval = einschreibung.Interval;
    }

    /// <summary>
    ///     The ID of the termin the enrollment is for
    /// </summary>
    public Guid TerminId { get; set; }

    /// <summary>
    ///     The time the enrollment is in. Usually, this is the full duration of the <see cref="Termin" />.
    /// </summary>
    public TimeOnlyInterval Interval { get; set; }

    /// <summary>
    ///     The ID of the enrollment
    /// </summary>
    public Guid Id { get; set; }

    /// <inheritdoc />
    public string Otium { get; set; }

    /// <inheritdoc />
    public Guid OtiumId { get; set; }

    /// <inheritdoc />
    public string Ort { get; set; }

    /// <inheritdoc />
    public char Block { get; set; }

    /// <inheritdoc />
    public PersonInfoMinimal? Tutor { get; set; }
}
