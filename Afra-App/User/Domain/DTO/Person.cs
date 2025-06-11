using System.ComponentModel.DataAnnotations;
using Afra_App.User.Domain.Models;

namespace Afra_App.User.Domain.DTO;

/// <summary>
///     Represents a person
/// </summary>
public record struct Person
{
    /// <summary>
    ///     Constructs a new Person DTO from a Person entity
    /// </summary>
    /// <param name="person">The persons DB entry</param>
    public Person(Models.Person person)
    {
        Id = person.Id;
        Vorname = person.Vorname;
        Nachname = person.Nachname;
        Email = person.Email;
        Mentor = person.Mentor is not null ? new PersonInfoMinimal(person.Mentor) : null;
        Mentees = person.Mentees.Select(mentee => new PersonInfoMinimal(mentee));
        Rolle = person.Rolle;
    }

    /// <summary>
    ///     A unique identifier for the person
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    ///     The first name of the person
    /// </summary>
    [MaxLength(20)]
    public string Vorname { get; init; }

    /// <summary>
    ///     The last name of the person
    /// </summary>
    [MaxLength(20)]
    public string Nachname { get; init; }

    /// <summary>
    ///     The email address of the person
    /// </summary>
    [EmailAddress]
    public string Email { get; init; }

    /// <summary>
    ///     The role of the person
    /// </summary>
    public Rolle Rolle { get; set; }

    /// <summary>
    ///     The mentor of the person
    /// </summary>
    public PersonInfoMinimal? Mentor { get; init; }

    /// <summary>
    ///     The mentees of the person
    /// </summary>
    public IEnumerable<PersonInfoMinimal> Mentees { get; init; }
}
