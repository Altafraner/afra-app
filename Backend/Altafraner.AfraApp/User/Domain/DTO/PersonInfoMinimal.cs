using System.Text.Json.Serialization;
using Altafraner.AfraApp.User.Domain.Models;

namespace Altafraner.AfraApp.User.Domain.DTO;

/// <summary>
///     A minimal representation of a person
/// </summary>
public record struct PersonInfoMinimal
{
    /// <summary>
    ///     Constructs a new minimal person representation from a person entity
    /// </summary>
    /// <param name="person">The persons DB entry</param>
    public PersonInfoMinimal(Person person)
    {
        Vorname = person.FirstName;
        Nachname = person.LastName;
        Id = person.Id;
        Rolle = person.Rolle;
        Gruppe = person.Gruppe;
        Email = person.Email;
    }

    /// <summary>
    ///     A unique identifier for the person
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    ///     The first name of the person
    /// </summary>
    public string Vorname { get; set; }

    /// <summary>
    ///     The last name of the person
    /// </summary>
    public string Nachname { get; set; }

    /// <summary>
    ///     The role of the person
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter<Rolle>))]
    public Rolle Rolle { get; set; }

    /// <inheritdoc cref="User.Domain.Models.Person.Gruppe" />
    public string? Gruppe { get; set; }

    /// <summary>
    ///     The email address of the person
    /// </summary>
    public string Email { get; set; }
}
