namespace Afra_App.Data.Json.Otium;

public record TerminPreview : ITermin
{
    public Guid Id { get; set; }
    public string Otium { get; set; }
    public string Ort { get; set; }
    public IAsyncEnumerable<Guid> Kategorien { get; set; }
    public PersonJsonInfoMinimal Tutor { get; set; }
    public double? Auslastung { get; set; }
    public int? MaxEinschreibungen { get; set; }
    
    public TerminPreview(Afra_App.Data.Otium.Termin termin, double? auslastung, IAsyncEnumerable<Guid> kategorien)
    {
        Id = termin.Id;
        Otium = termin.Otium.Bezeichnung;
        Ort = termin.Ort;
        Kategorien = kategorien;
        Tutor = new PersonJsonInfoMinimal(termin.Tutor);
        Auslastung = auslastung;
        MaxEinschreibungen = termin.MaxEinschreibungen;
    }
};