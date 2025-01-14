namespace Afra_App.Models.Json;

public record struct PersonJsonInfo : IJsonObject
{
    public Guid Id { get; init; }
    public string FirstName { get; init; }
    public string LastName { get; init; }
    public string Email { get; init; }
    public PersonJsonInfoMinimal? Mentor { get; init; }
    public ClassJsonInfoMinimal? Class { get; init; }
    public IEnumerable<PersonJsonInfoMinimal> Mentees { get; init; }
    public IEnumerable<ClassJsonInfoMinimal> TutoredClasses { get; init; }

    public PersonJsonInfo(Person person)
    {
        Id = person.Id;
        FirstName = person.FirstName;
        LastName = person.LastName;
        Email = person.Email;
        Mentor = person.Mentor is not null ? new PersonJsonInfoMinimal(person.Mentor) : null;
        Class = person.Class is not null ? new ClassJsonInfoMinimal(person.Class) : null;
        Mentees = person.Mentees.Select(mentee => new PersonJsonInfoMinimal(mentee));
        TutoredClasses = person.TutoredClasses.Select(classInstance => new ClassJsonInfoMinimal(classInstance));
    }
}

public record struct PersonJsonInfoMinimal : IJsonObject
{
    public string Name { get; set; }
    public Guid Id { get; set; }

    public PersonJsonInfoMinimal(Person person)
    {
        Name = $"{person.FirstName} {person.LastName}";
        Id = person.Id;
    }
}