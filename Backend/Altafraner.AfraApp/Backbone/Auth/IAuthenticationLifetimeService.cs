using System.Security.Claims;

namespace Altafraner.AfraApp.Backbone.Auth;

/// <summary>
///     A service handling signing in and out users.
/// </summary>
public interface IAuthenticationLifetimeService
{
    /// <summary>
    ///     Signs in a user
    /// </summary>
    Task SignInAsync(ClaimsPrincipal principal, bool isPersistent = false);

    /// <summary>
    ///     Signs out a user
    /// </summary>
    /// <returns></returns>
    Task SignOutAsync();
}
