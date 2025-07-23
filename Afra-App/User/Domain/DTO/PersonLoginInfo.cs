using System.Text.Json.Serialization;
using Afra_App.User.Domain.Models;

namespace Afra_App.User.Domain.DTO;

/// <summary>
///     A dto for communicating information about the current user to clients
/// </summary>
public record PersonLoginInfo
{
    /// <inheritdoc cref="User.Domain.DTO.Person.Id" />
    public required Guid Id { get; set; }

    /// <inheritdoc cref="User.Domain.DTO.Person.Vorname" />
    public required string Vorname { get; set; }

    /// <inheritdoc cref="User.Domain.DTO.Person.Nachname" />
    public required string Nachname { get; set; }

    /// <inheritdoc cref="User.Domain.DTO.Person.Rolle" />
    [JsonConverter(typeof(JsonStringEnumConverter<Rolle>))]
    public required Rolle Rolle { get; set; }

    /// <summary>
    ///     A list of global permissions the user is assigned.
    /// </summary>
    public required GlobalPermission[] Berechtigungen { get; set; }
}