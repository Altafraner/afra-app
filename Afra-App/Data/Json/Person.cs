using System.ComponentModel.DataAnnotations;
using Afra_App.Data.People;

namespace Afra_App.Data.Json;

public record struct PersonJsonInfo
{
    public Guid Id { get; init; }
    
    // The properties 'Vorname', 'Nachname' and 'Email' are a cache of the AD and should be updated regularly
    [MaxLength(20)]
    public string Vorname { get; init; }
    [MaxLength(20)]
    public string Nachname { get; init; }
    
    [EmailAddress]
    public string Email { get; init; }
    public Rolle Rolle { get; set; }
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
        Rolle = person.Rolle;
    }
}

public record struct PersonJsonInfoMinimal
{
    public string Vorname { get; set; }
    public string Nachname { get; set; }
    public Guid Id { get; set; }
    public Rolle Rolle { get; set; }

    public PersonJsonInfoMinimal(Person person)
    {
        Vorname = person.Vorname;
        Nachname = person.Nachname;
        Id = person.Id;
        Rolle = person.Rolle;
    }
}