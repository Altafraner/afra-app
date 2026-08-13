using System.ComponentModel.DataAnnotations;
using Altafraner.Backbone.Utils;

namespace Altafraner.AfraApp.Domain.Configuration;

/// <summary>
///     Contains OIDC Configuration
/// </summary>
public class OidcConfiguration : IValidatable<OidcConfiguration>
{
    /// <summary>
    ///     Whether OIDC is used
    /// </summary>
    public bool Enabled { get; init; }

    /// <summary>
    ///     The OIDC Authority
    /// </summary>
    /// <example>For keycloak, use <c>https://[your-keycloak-url]/realms/[your-realm]</c></example>
    public string? Authority { get; init; }

    /// <summary>
    ///     The name of the claim that stores the ldap objectGuid
    /// </summary>
    public string? IdClaim { get; init; }

    /// <summary>
    ///     The OIDC Client ID
    /// </summary>
    public string? ClientId { get; init; }

    /// <summary>
    ///     The OIDC Client Secret
    /// </summary>
    public string? ClientSecret { get; init; }

    /// <inheritdoc />
    public static bool Validate(OidcConfiguration configuration)
    {
        if (!configuration.Enabled) return true;
        if (!string.IsNullOrWhiteSpace(configuration.Authority)
            && !string.IsNullOrWhiteSpace(configuration.ClientId)
            && !string.IsNullOrWhiteSpace(configuration.ClientSecret)
            && !string.IsNullOrWhiteSpace(configuration.IdClaim))
            return true;
        throw new ValidationException(
            $"{nameof(Authority)}, {nameof(ClientId)}, {nameof(ClientSecret)} and {nameof(IdClaim)} must be set when oidc is enabled");
    }
}
