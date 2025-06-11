using Afra_App.User.Domain.DTO;

namespace Afra_App.Otium.Domain.DTO;

/// <summary>
///     A DTO for the detailed view of a termin
/// </summary>
public record Termin : ITermin
{
    /// <summary>
    ///     Constructs a new DTO from a termin entry
    /// </summary>
    /// <param name="termin">The termins DB entry</param>
    /// <param name="einschreibungen">A list of all available timeslots for the termin</param>
    /// <param name="kategorien">All categories the Otium is in</param>
    /// <param name="startTime">The time the termin starts at</param>
    public Termin(Models.Termin termin, IAsyncEnumerable<EinschreibungsPreview> einschreibungen,
        IAsyncEnumerable<Guid> kategorien, TimeOnly startTime)
    {
        Id = termin.Id;
        Otium = termin.Otium.Bezeichnung;
        OtiumId = termin.Otium.Id;
        Ort = termin.Ort;
        Kategorien = kategorien;
        IstAbgesagt = termin.IstAbgesagt;
        Tutor = termin.Tutor is not null ? new PersonInfoMinimal(termin.Tutor) : null;
        MaxEinschreibungen = termin.MaxEinschreibungen;
        Einschreibungen = einschreibungen;
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
    public IAsyncEnumerable<EinschreibungsPreview> Einschreibungen { get; set; }

    /// <summary>
    /// The description of the termin
    /// </summary>
    public string Beschreibung { get; set; }

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
    public IAsyncEnumerable<Guid> Kategorien { get; set; }

    /// <inheritdoc />
    public PersonInfoMinimal? Tutor { get; set; }

    /// <inheritdoc />
    public int? MaxEinschreibungen { get; set; }

    /// <inheritdoc />
    public bool IstAbgesagt { get; set; }
}