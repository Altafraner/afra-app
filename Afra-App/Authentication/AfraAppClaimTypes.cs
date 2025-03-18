using System.Security.Claims;

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
    public static string Id => "id";

    /// <summary>
    ///     The given, aka. first, name of the user
    /// </summary>
    public static string GivenName => "given_name";

    /// <summary>
    ///     The family, aka. last, name of the user
    /// </summary>
    public static string LastName => "last_name";
}