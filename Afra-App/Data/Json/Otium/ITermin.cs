namespace Afra_App.Data.Json.Otium;

public interface ITermin
{
    public Guid Id { get; set; }
    public string Otium { get; set; }
    public string Ort { get; set; }
    public IAsyncEnumerable<Guid> Kategorien { get; set; }
    public PersonJsonInfoMinimal Tutor { get; set; }
    public int? MaxEinschreibungen { get; set; }
}