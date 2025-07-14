using Afra_App.User.Domain.DTO;

namespace Afra_App.Otium.Domain.DTO.Katalog;

/// <summary>
///     A DTO for the detailed view of a termin
/// </summary>
public record Termin : ITermin
{
    /// <summary>
    ///     Constructs a new DTO from a termin entry
    /// </summary>
    /// <param name="termin">The termins DB entry</param>
    /// <param name="einschreibung">Information on whether and how to enroll</param>
    /// <param name="kategorie">The categorie the Otium is in</param>
    /// <param name="startTime">The time the termin starts at</param>
    public Termin(Models.Termin termin, EinschreibungsPreview einschreibung,
        Guid kategorie, TimeOnly startTime)
    {
        Id = termin.Id;
        Otium = termin.Otium.Bezeichnung;
        OtiumId = termin.Otium.Id;
        Ort = termin.Ort;
        Kategorie = kategorie;
        IstAbgesagt = termin.IstAbgesagt;
        Tutor = termin.Tutor is not null ? new PersonInfoMinimal(termin.Tutor) : null;
        MaxEinschreibungen = termin.MaxEinschreibungen;
        Einschreibung = einschreibung;
        Block = termin.Block.SchemaId;
        Datum = termin.Block.Schultag.Datum.ToDateTime(startTime);
        Beschreibung = termin.Otium.Beschreibung;
    }

    /// <summary>
    ///     The start date and time for the termin
    /// </summary>
    public DateTime Datum { get; set; }

    /// <summary>
    ///     A list of all available timeslots for the termin
    /// </summary>
    public EinschreibungsPreview Einschreibung { get; set; }

    /// <summary>
    /// The description of the termin
    /// </summary>
    public string Beschreibung { get; set; }

    /// <summary>
    ///    The unique ID of the category the Otium is in
    /// </summary>
    public Guid Kategorie { get; set; }

    /// <summary>
    ///     The unique ID for the Termin
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

    /// <inheritdoc />
    public int? MaxEinschreibungen { get; set; }

    /// <inheritdoc />
    public bool IstAbgesagt { get; set; }
}
