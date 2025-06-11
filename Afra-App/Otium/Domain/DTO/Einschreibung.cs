using Afra_App.Backbone.Domain.TimeInterval;
using Afra_App.Otium.Domain.DTO.Katalog;
using Afra_App.User.Domain.DTO;

namespace Afra_App.Otium.Domain.DTO;

/// <summary>
///     A DTO for an enrollment
/// </summary>
public record Einschreibung : IMinimalTermin
{
    /// <summary>
    ///     Creates a new DTO from a einschreibung entry
    /// </summary>
    /// <param name="einschreibung"></param>
    public Einschreibung(Models.Einschreibung einschreibung)
    {
        Id = einschreibung.Id;
        TerminId = einschreibung.Termin.Id;
        Otium = einschreibung.Termin.Otium.Bezeichnung;
        OtiumId = einschreibung.Termin.Otium.Id;
        KategorieId = einschreibung.Termin.Otium.Kategorie.Id;
        Ort = einschreibung.Termin.Ort;
        Block = einschreibung.Termin.Block.SchemaId;
        Tutor = einschreibung.Termin.Tutor is not null ? new PersonInfoMinimal(einschreibung.Termin.Tutor) : null;
        Interval = einschreibung.Interval;
        Datum = einschreibung.Termin.Block.SchultagKey;
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
    ///     The date the enrollment is for
    /// </summary>
    public DateOnly Datum { get; set; }

    /// <summary>
    /// The ID of the category the enrollment is for
    /// </summary>
    public Guid KategorieId { get; set; }

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
