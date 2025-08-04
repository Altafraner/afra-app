namespace Afra_App.Profundum.Domain.DTO;

///
public record BlockKatalog
{
    ///
    public required string label { get; set; }
    ///
    public required string id { get; set; }
    ///
    public required BlockOption[] options { get; set; }
}
