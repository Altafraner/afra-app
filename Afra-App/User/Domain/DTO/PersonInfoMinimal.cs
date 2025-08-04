using System.Text.Json.Serialization;
using Afra_App.User.Domain.Models;

namespace Afra_App.User.Domain.DTO;

/// <summary>
///     A minimal representation of a person
/// </summary>
public record struct PersonInfoMinimal
{
    /// <summary>
    ///     Constructs a new minimal person representation from a person entity
    /// </summary>
    /// <param name="person">The persons DB entry</param>
    public PersonInfoMinimal(Models.Person person)
    {
        Vorname = person.Vorname;
        Nachname = person.Nachname;
        Id = person.Id;
        Rolle = person.Rolle;
    }

    /// <inheritdoc cref="User.Domain.DTO.Person.Id" />
    public Guid Id { get; set; }

    /// <inheritdoc cref="User.Domain.DTO.Person.Vorname" />
    public string Vorname { get; set; }

    /// <inheritdoc cref="User.Domain.DTO.Person.Nachname" />
    public string Nachname { get; set; }

    /// <inheritdoc cref="User.Domain.DTO.Person.Rolle" />
    [JsonConverter(typeof(JsonStringEnumConverter<Rolle>))]
    public Rolle Rolle { get; set; }
}
