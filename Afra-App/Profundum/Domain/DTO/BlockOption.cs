namespace Afra_App.Profundum.Domain.DTO;

///
public record BlockOption
{
    ///
    public required string label { get; set; }
    ///
    public required Guid value { get; set; }
    ///
    public string[]? alsoIncludes { get; set; }
}

