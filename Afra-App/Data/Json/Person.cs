using Afra_App.Data.People;

namespace Afra_App.Data.Json;

public record struct PersonJsonInfo
{
    public Guid Id { get; init; }
    public string Vorname { get; init; }
    public string Nachname { get; init; }
    public string Email { get; init; }
    public PersonJsonInfoMinimal? Mentor { get; init; }
    public IEnumerable<PersonJsonInfoMinimal> Mentees { get; init; }

    public PersonJsonInfo(Person person)
    {
        Id = person.Id;
        Vorname = person.Vorname;
        Nachname = person.Nachname;
        Email = person.Email;
        Mentor = person.Mentor is not null ? new PersonJsonInfoMinimal(person.Mentor) : null;
        Mentees = person.Mentees.Select(mentee => new PersonJsonInfoMinimal(mentee));
    }
}

public record struct PersonJsonInfoMinimal
{
    public string Name { get; set; }
    public Guid Id { get; set; }

    public PersonJsonInfoMinimal(Person person)
    {
        Name = $"{person.Vorname} {person.Nachname}";
        Id = person.Id;
    }
}