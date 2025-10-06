using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

namespace Altafraner.Backbone.CookieAuthentication.Services;

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
        await context.SignOutAsync();
    }
}