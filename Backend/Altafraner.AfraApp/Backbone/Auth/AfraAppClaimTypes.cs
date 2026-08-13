using System.Security.Claims;
using Altafraner.AfraApp.User.Domain.Models;

namespace Altafraner.AfraApp.Backbone.Auth;

/// <summary>
///     Specifies the claim types used in the <see cref="ClaimsPrincipal" /> for the Afra-App
/// </summary>
public static class AfraAppClaimTypes
{
    /// <summary>
    ///     The unique identifier of the user. Should be a <see cref="Guid" />, but must be a <see cref="string" /> for
    ///     compatibility.
    /// </summary>
    public const string Id = "http://altafraner.de/claims/id";

    /// <summary>
    ///     The given, aka. first, name of the user
    /// </summary>
    public const string GivenName = ClaimTypes.GivenName;

    /// <summary>
    ///     The family, aka. last, name of the user
    /// </summary>
    public const string LastName = ClaimTypes.Name;

    /// <summary>
    ///     The role of the user
    /// </summary>
    /// <remarks>Should be one of <see cref="Rolle" /></remarks>
    public const string Role = ClaimTypes.Role;

    /// <summary>
    ///     A global permission of the user
    /// </summary>
    /// <remarks>Should be one of <see cref="GlobalPermission" /></remarks>
    public const string GlobalPermission = "GlobalPermission";

    /// <summary>
    ///     Indicates that the current session is an impersonation session.
    /// </summary>
    public const string ImpersonatingUserId = "ImpersonatingUserId";

    /// <summary>
    ///     Used to save the id_token from the oidc protocoll message
    /// </summary>
    public const string OidcIdTokenHint = "OidcIdTokenHint";
}
