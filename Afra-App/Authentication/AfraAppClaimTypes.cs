using System.Security.Claims;
using Afra_App.Data.People;

namespace Afra_App.Authentication;

/// <summary>
///     Specifies the claim types used in the <see cref="ClaimsPrincipal" /> for the Afra-App
/// </summary>
public static class AfraAppClaimTypes
{
    /// <summary>
    ///     The unique identifier of the user. Should be a <see cref="Guid" />, but must be a <see cref="string" /> for
    ///     compatibility.
    /// </summary>
    public static string Id => ClaimTypes.NameIdentifier;

    /// <summary>
    ///     The given, aka. first, name of the user
    /// </summary>
    public static string GivenName => ClaimTypes.GivenName;

    /// <summary>
    ///     The family, aka. last, name of the user
    /// </summary>
    public static string LastName => ClaimTypes.Name;

    /// <summary>
    ///     The role of the user
    /// </summary>
    /// <remarks>Should be one of <see cref="Rolle" /></remarks>
    public static string Role => ClaimTypes.Role;
}
