namespace Afra_App.Data.Json;

public record Termin
{
    public string Otium { get; set; }
    public Guid[] Kategorien { get; set; }
    public PersonJsonInfoMinimal Tutor { get; set; }
    public double? Auslastung { get; set; }
    public int? MaxEinschreibungen { get; set; }
    
    public Termin(Afra_App.Data.Otium.Termin termin, double? auslastung)
    {
        Otium = termin.Otium.Bezeichnung;
        Kategorien = termin.Otium.Kategorien.Select(kategorie => kategorie.Id).ToArray();
        Tutor = new PersonJsonInfoMinimal(termin.Tutor);
        Auslastung = auslastung;
        MaxEinschreibungen = termin.MaxEinschreibungen;
    }
};