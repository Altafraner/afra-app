using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

namespace Altafraner.AfraApp.Backbone.Auth;

internal class AuthenticationLifetimeService : IAuthenticationLifetimeService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthenticationLifetimeService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task SignInAsync(ClaimsPrincipal principal, bool isPersistent = false)
    {
        var context = _httpContextAccessor.HttpContext ??
                      throw new InvalidOperationException("There is no httpContext in the current scope");
        var props = new AuthenticationProperties
        {
            IsPersistent = isPersistent
        };
        await context.SignInAsync(principal, props);
    }

    public async Task SignOutAsync()
    {
        var context = _httpContextAccessor.HttpContext ??
                      throw new InvalidOperationException("There is no httpContext in the current scope");
        var props = new AuthenticationProperties { RedirectUri = "/" };
        var idToken = context.User.FindFirst(AfraAppClaimTypes.OidcIdTokenHint);
        if (idToken is not null)
        {
            await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            await context.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme, props);
            return;
        }

        await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme, props);
    }
}
