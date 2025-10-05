using System.Security.Claims;
using Altafraner.AfraApp.Backbone.Authentication;
using Altafraner.AfraApp.User.Domain.Models;
using Altafraner.Backbone.CookieAuthentication.Contracts;

namespace Altafraner.AfraApp.User.Services;

/// <summary>
///     A service for accessing the currently logged-in user.
/// </summary>
public class UserAccessor
{
    private const string UserPersonObjectCacheKey = "UserPersonObject";
    private readonly AfraAppContext _dbContext;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    /// <summary>
    ///     Constructs a new instance of the <see cref="UserAccessor" /> class.
    /// </summary>
    public UserAccessor(IHttpContextAccessor httpContextAccessor, IServiceScopeFactory serviceScopeFactory,
        AfraAppContext dbContext)
    {
        _httpContextAccessor = httpContextAccessor;
        _serviceScopeFactory = serviceScopeFactory;
        _dbContext = dbContext;
    }

    /// <summary>
    ///     Fetches the currently logged-in user from the database.
    /// </summary>
    /// <returns>The currently logged-in user, if any; Otherwise null</returns>
    /// <exception cref="InvalidOperationException">There is no <see cref="HttpContext" /> or the user is not signed in.</exception>
    /// <exception cref="KeyNotFoundException">
    ///     The user identified by the <see cref="ClaimsPrincipal" /> does not exist in the
    ///     database.
    /// </exception>
    public async Task<Person> GetUserAsync()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext is null)
            throw new InvalidOperationException("The HttpContext is not available!");

        if (httpContext.Items.TryGetValue(UserPersonObjectCacheKey, out var cachedUserObject) &&
            cachedUserObject is Person cachedUser)
            return cachedUser;

        if (!httpContext.User.Identity?.IsAuthenticated ?? true)
            throw new InvalidOperationException("The user is not logged in!");

        if (!httpContext.User.HasClaim(claim => claim.Type == AfraAppClaimTypes.Id))
            throw new InvalidOperationException($"The user does not have a {AfraAppClaimTypes.Id} claim");

        var user = await _dbContext.Personen.FindAsync(new Guid(httpContext.User.Claims
            .First(claim => claim.Type == AfraAppClaimTypes.Id).Value));

        if (user is not null)
        {
            httpContext.Items.Add(UserPersonObjectCacheKey, user);
            return user;
        }

        using var scope = _serviceScopeFactory.CreateScope();
        var authenticationLifetimeService = scope.ServiceProvider.GetRequiredService<IAuthenticationLifetimeService>();
        await authenticationLifetimeService.SignOutAsync();
        throw new KeyNotFoundException("The specified User does not exist");
    }
}
