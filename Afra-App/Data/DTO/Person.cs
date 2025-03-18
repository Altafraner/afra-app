using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Afra_App.Data.People;

namespace Afra_App.Data.DTO;

/// <summary>
///     Represents a person
/// </summary>
public record struct Person
{
    /// <summary>
    ///     Constructs a new Person DTO from a Person entity
    /// </summary>
    /// <param name="person">The persons DB entry</param>
    public Person(People.Person person)
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

/// <summary>
///     A minimal representation of a person
/// </summary>
public record struct PersonInfoMinimal
{
    /// <summary>
    ///     Constructs a new minimal person representation from a person entity
    /// </summary>
    /// <param name="person">The persons DB entry</param>
    public PersonInfoMinimal(People.Person person)
    {
        Vorname = person.Vorname;
        Nachname = person.Nachname;
        Id = person.Id;
        Rolle = person.Rolle;
    }

    /// <inheritdoc cref="Person.Id" />
    public Guid Id { get; set; }

    /// <inheritdoc cref="Person.Vorname" />
    public string Vorname { get; set; }

    /// <inheritdoc cref="Person.Nachname" />
    public string Nachname { get; set; }

    /// <inheritdoc cref="Person.Rolle" />
    [JsonConverter(typeof(JsonStringEnumConverter<Rolle>))]
    public Rolle Rolle { get; set; }
}