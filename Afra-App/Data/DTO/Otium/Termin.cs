using Afra_App.Data.TimeInterval;

namespace Afra_App.Data.DTO.Otium;

/// <summary>
/// A DTO for the detailed view of a termin
/// </summary>
public record Termin : ITermin
{
    /// <summary>
    /// The unique ID for the Termin
    /// </summary>
    public Guid Id { get; set; }

    /// <inheritdoc />
    public string Otium { get; set; }

    /// <inheritdoc />
    public string Ort { get; set; }

    /// <inheritdoc />
    public IAsyncEnumerable<Guid> Kategorien { get; set; }
    
    /// <inheritdoc />
    public PersonInfoMinimal Tutor { get; set; }

    /// <inheritdoc />
    public int? MaxEinschreibungen { get; set; }
    
    /// <summary>
    /// A list of all available timeslots for the termin
    /// </summary>
    public IAsyncEnumerable<EinschreibungsPreview> Einschreibungen { get; set; }

    /// <summary>
    /// Constructs a new DTO from a termin entry
    /// </summary>
    /// <param name="termin">The termins DB entry</param>
    /// <param name="einschreibungen">A list of all available timeslots for the termin</param>
    /// <param name="kategorien">All categories the Otium is in</param>
    public Termin(Data.Otium.Termin termin, IAsyncEnumerable<EinschreibungsPreview> einschreibungen, IAsyncEnumerable<Guid> kategorien)
    {
        Id = termin.Id;
        Otium = termin.Otium.Bezeichnung;
        Ort = termin.Ort;
        Kategorien = kategorien;
        Tutor = new PersonInfoMinimal(termin.Tutor);
        MaxEinschreibungen = termin.MaxEinschreibungen;
        Einschreibungen = einschreibungen;
    }

}

/// <summary>
/// A DTO for previewing a time-slot
/// </summary>
/// <param name="Anzahl">the amount of einschreibungen for this sub-slot</param>
/// <param name="KannBearbeiten">whether the user may edit his enrollment status</param>
/// <param name="Eingeschrieben">whether the user is enrolled for this sub-slot</param>
/// <param name="Interval">the timeslot for this otium</param>
public record EinschreibungsPreview(int Anzahl, bool KannBearbeiten, bool Eingeschrieben, TimeOnlyInterval Interval);