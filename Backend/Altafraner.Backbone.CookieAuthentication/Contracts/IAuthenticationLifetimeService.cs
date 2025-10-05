using System.Security.Claims;

namespace Altafraner.Backbone.CookieAuthentication.Contracts;

public interface IAuthenticationLifetimeService
{
    public Task SignInAsync(ClaimsPrincipal principal, bool isPersistent = false);
    public Task SignOutAsync();
}
