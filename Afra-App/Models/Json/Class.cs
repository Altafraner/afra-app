namespace Afra_App.Models.Json;

public record struct ClassJsonInfo : IJsonObject
{
    public Guid Id { get; init; }
    public int Level { get; init; }
    public string? Appendix { get; init; }
    public PersonJsonInfoMinimal? Tutor { get; init; }
    public IEnumerable<PersonJsonInfoMinimal> Students { get; init; }

    public ClassJsonInfo(Class classInstance)
    {
        Id = classInstance.Id;
        Level = classInstance.Level;
        Appendix = classInstance.Appendix;
        Tutor = classInstance.Tutor is null ? null : new PersonJsonInfoMinimal(classInstance.Tutor);
        Students = classInstance.Students.Select(student => new PersonJsonInfoMinimal(student));
    }
}

public record struct ClassJsonInfoMinimal : IJsonObject
{
    public string Name { get; set; }
    public Guid Id { get; set; }

    public ClassJsonInfoMinimal(Class classInstance)
    {
        Name = classInstance.ToString();
        Id = classInstance.Id;
    }
}