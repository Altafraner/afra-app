using System.ComponentModel.DataAnnotations;

namespace Altafraner.AfraApp.Domain.Configuration;

/// <summary>
///     Contains OIDC Configuration
/// </summary>
public class OidcConfiguration
{
    /// <summary>
    ///     Whether OIDC is used
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    ///     The OIDC Authority
    /// </summary>
    /// <example>For keycloak, use <c>https://[your-keycloak-url]/realms/[your-realm]</c></example>
    public string? Authority { get; set; }

    /// <summary>
    ///     The name of the claim that stores the ldap objectGuid
    /// </summary>
    public string? IdClaim { get; set; }

    /// <summary>
    ///     The OIDC Client ID
    /// </summary>
    public string? ClientId { get; set; }

    /// <summary>
    ///     The OIDC Client Secret
    /// </summary>
    public string? ClientSecret { get; set; }

    internal static bool Validate(OidcConfiguration configuration)
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
