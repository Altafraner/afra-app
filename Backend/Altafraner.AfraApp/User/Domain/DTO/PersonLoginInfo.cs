using System.Text.Json.Serialization;
using Altafraner.AfraApp.User.Domain.Models;

namespace Altafraner.AfraApp.User.Domain.DTO;

/// <summary>
///     A dto for communicating information about the current user to clients
/// </summary>
public record PersonLoginInfo
{
    /// <summary>
    ///     A unique identifier for the person
    /// </summary>
    public required Guid Id { get; set; }

    /// <summary>
    ///     The first name of the person
    /// </summary>
    public required string Vorname { get; set; }

    /// <summary>
    ///     The last name of the person
    /// </summary>
    public required string Nachname { get; set; }

    /// <summary>
    ///     The role of the person
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter<Rolle>))]
    public required Rolle Rolle { get; set; }

    /// <summary>
    ///     A list of global permissions the user is assigned.
    /// </summary>
    public required GlobalPermission[] Berechtigungen { get; set; }

    /// <summary>
    ///     Whether the current session is an impersonation session.
    /// </summary>
    public string? ImpersonationId { get; set; }

    /// <summary>
    ///     The URL for managing the user's account.
    /// </summary>
    public required string AccountManagementUrl { get; set; }
}
