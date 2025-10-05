using System.ComponentModel.DataAnnotations;
using Altafraner.AfraApp.User.Domain.Models;

namespace Altafraner.AfraApp.User.Domain.DTO;

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
        Vorname = person.FirstName;
        Nachname = person.LastName;
        Email = person.Email;
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