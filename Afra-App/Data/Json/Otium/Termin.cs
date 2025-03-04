using Afra_App.Data.TimeInterval;

namespace Afra_App.Data.Json.Otium;

public record Termin : ITermin
{
    public Guid Id { get; set; }
    public string Otium { get; set; }
    public string Ort { get; set; }
    public IAsyncEnumerable<Guid> Kategorien { get; set; }
    public PersonJsonInfoMinimal Tutor { get; set; }
    public int? MaxEinschreibungen { get; set; }
    public IAsyncEnumerable<EinschreibungsPreview> Einschreibungen { get; set; }

    public Termin(Data.Otium.Termin termin, IAsyncEnumerable<EinschreibungsPreview> einschreibungen, IAsyncEnumerable<Guid> kategorien)
    {
        Id = termin.Id;
        Otium = termin.Otium.Bezeichnung;
        Ort = termin.Ort;
        Kategorien = kategorien;
        Tutor = new PersonJsonInfoMinimal(termin.Tutor);
        MaxEinschreibungen = termin.MaxEinschreibungen;
        Einschreibungen = einschreibungen;
    }

};

public record EinschreibungsPreview(int Anzahl, bool KannBearbeiten, bool Eingeschrieben, TimeOnlyInterval Interval);