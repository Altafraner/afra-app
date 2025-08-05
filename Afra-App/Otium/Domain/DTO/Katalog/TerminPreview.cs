using Afra_App.User.Domain.DTO;

namespace Afra_App.Otium.Domain.DTO.Katalog;

/// <summary>
///     A DTO for sending a preview of a termin to the client.
/// </summary>
public record TerminPreview : ITermin
{
    /// <summary>
    ///     Constructs a new TerminPreview DTO from a Termin entity
    /// </summary>
    /// <param name="termin">The termins DB entry</param>
    /// <param name="auslastung">The load of the termin</param>
    /// <param name="kategorien">A list of all categories the otium for the termin is in.</param>
    /// <param name="block">The block the termin is in</param>
    public TerminPreview(Models.OtiumTermin termin, int? auslastung, IAsyncEnumerable<Guid> kategorien, string block)
    {
        Id = termin.Id;
        Otium = termin.Otium.Bezeichnung;
        OtiumId = termin.Otium.Id;
        Ort = termin.Ort;
        BlockSchemaId = termin.Block.SchemaId;
        Kategorien = kategorien;
        IstAbgesagt = termin.IstAbgesagt;
        Tutor = termin.Tutor is null ? null : new PersonInfoMinimal(termin.Tutor);
        Auslastung = auslastung;
        MaxEinschreibungen = termin.MaxEinschreibungen;
        Block = block;
    }

    /// <summary>
    ///     If the termin has <see cref="MaxEinschreibungen" /> the current load-factor of the termin; otherwise null.
    /// </summary>
    /// <remarks>
    ///     Currently implemented by summing up the number of minutes in all einschreibungen and dividing by the total number
    ///     of minutes available for the termin.
    /// </remarks>
    public int? Auslastung { get; set; }

    /// <summary>
    ///     A list of all categories the termin is in
    /// </summary>
    public IAsyncEnumerable<Guid> Kategorien { get; set; }

    /// <summary>
    ///     A unique identifier for the termin
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    ///     The designation of the otium for which the termin is
    /// </summary>
    public string Otium { get; set; }

    /// <inheritdoc />
    public Guid OtiumId { get; set; }

    /// <summary>
    ///     The location where the termin takes place
    /// </summary>
    public string Ort { get; set; }

    /// <inheritdoc />
    public char BlockSchemaId { get; set; }

    /// <inheritdoc />
    public string Block { get; set; }

    /// <summary>
    ///     The tutor handling the termin (optional)
    /// </summary>
    public PersonInfoMinimal? Tutor { get; set; }

    /// <summary>
    ///     The maximum number of people that can be at the termin concurrently
    /// </summary>
    public int? MaxEinschreibungen { get; set; }

    /// <inheritdoc />
    public bool IstAbgesagt { get; set; }
}
