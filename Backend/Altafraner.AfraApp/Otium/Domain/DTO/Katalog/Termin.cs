using Altafraner.AfraApp.Otium.Domain.Models;
using Altafraner.AfraApp.Schuljahr.Domain.Models;
using Altafraner.AfraApp.User.Domain.DTO;

namespace Altafraner.AfraApp.Otium.Domain.DTO.Katalog;

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
    /// <param name="kategorie">The category the Otium is in</param>
    /// <param name="schema">The schema of the block the termin is in</param>
    public Termin(
        OtiumTermin termin,
        EinschreibungsPreview einschreibung,
        Guid kategorie,
        BlockMetadata schema
    )
    {
        Id = termin.Id;
        Otium = termin.Bezeichnung;
        Beschreibung = termin.Beschreibung;
        OtiumId = termin.Otium.Id;
        Ort = termin.Ort;
        Kategorie = kategorie;
        IstAbgesagt = termin.IstAbgesagt;
        Tutor = termin.Tutor is not null ? new PersonInfoMinimal(termin.Tutor) : null;
        MaxEinschreibungen = termin.MaxEinschreibungen;
        Einschreibung = einschreibung;
        Wiederholungen =
            termin
                .Wiederholung?.Termine.Select(t => t.Block.SchultagKey)
                .Distinct()
                .Order()
                .SkipWhile(d => d <= termin.Block.SchultagKey)
            ?? [];
        Block = new BlockInfo
        {
            Datum = termin.Block.Schultag.Datum,
            Id = termin.Block.Id,
            Name = schema.Bezeichnung,
            SchemaId = schema.Id,
            Uhrzeit = schema.Interval,
        };
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
    ///     The description of the termin
    /// </summary>
    public string Beschreibung { get; set; }

    /// <summary>
    ///     The unique ID of the category the Otium is in
    /// </summary>
    public Guid Kategorie { get; set; }

    /// <summary>
    ///     A list of all dates on which the termin is repeated
    /// </summary>
    public IEnumerable<DateOnly> Wiederholungen { get; set; } = [];

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

    /// <summary>
    ///     The block the termin is in.
    /// </summary>
    public BlockInfo Block { get; set; }

    /// <inheritdoc />
    public PersonInfoMinimal? Tutor { get; set; }

    /// <inheritdoc />
    public int? MaxEinschreibungen { get; set; }

    /// <inheritdoc />
    public bool IstAbgesagt { get; set; }

    /// <summary>
    ///     A one time override name for the Otium Termin
    /// </summary>
    public string? Bezeichnung { get; set; }
}
