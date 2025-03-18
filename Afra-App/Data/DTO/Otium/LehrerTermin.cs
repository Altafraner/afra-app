using Afra_App.Data.TimeInterval;

namespace Afra_App.Data.DTO.Otium;

public record LehrerTermin : IMinimalTermin
{
    public Guid Id { get; set; }
    public string Otium { get; set; }
    public string Ort { get; set; }
    public PersonInfoMinimal? Tutor { get; set; }
    public IEnumerable<LehrerEinschreibung> Einschreibungen { get; set; }
    public DateOnly Datum { get; set; }
    public byte Block { get; set; }
}

public record LehrerEinschreibung(
    PersonInfoMinimal? Student,
    TimeOnlyInterval Interval,
    AnwesenheitsStatus Anwesenheit);