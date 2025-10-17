using System.Security.Claims;
using Altafraner.AfraApp.Backbone.Authorization;
using Altafraner.AfraApp.User.Domain.Models;
using Altafraner.Backbone.CookieAuthentication;

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
        var httpContext = GetHttpContextOrThrow();

        var cachedUser = GetUserFromCache(httpContext);
        if (cachedUser != null) return cachedUser;

        var user = await _dbContext.Personen.FindAsync(GetUserIdOrThrow(httpContext));

        if (user is null)
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var authenticationLifetimeService =
                scope.ServiceProvider.GetRequiredService<IAuthenticationLifetimeService>();
            await authenticationLifetimeService.SignOutAsync();
            throw new KeyNotFoundException("The specified User does not exist");
        }

        httpContext.Items.Add(UserPersonObjectCacheKey, user);
        return user;
    }

    /// <summary>
    ///     Gets the currently logged-in users id
    /// </summary>
    /// <exception cref="InvalidOperationException">
    ///     There is no <see cref="HttpContext" /> or the user is not signed in.
    /// </exception>
    /// <exception cref="KeyNotFoundException">
    ///     The user identified by the <see cref="ClaimsPrincipal" /> does not exist in the database.
    /// </exception>
    public Guid GetUserId()
    {
        var httpContext = GetHttpContextOrThrow();
        return GetUserFromCache(httpContext)?.Id ?? GetUserIdOrThrow(httpContext);
    }

    private HttpContext GetHttpContextOrThrow()
    {
        return _httpContextAccessor.HttpContext ??
               throw new InvalidOperationException("The HttpContext is not available!");
    }

    private Person? GetUserFromCache(HttpContext httpContext)
    {
        if (httpContext.Items.TryGetValue(UserPersonObjectCacheKey, out var cachedUserObject) &&
            cachedUserObject is Person cachedUser)
            return cachedUser;
        return null;
    }

    private Guid GetUserIdOrThrow(HttpContext httpContext)
    {
        if (!httpContext.User.Identity?.IsAuthenticated ?? true)
            throw new InvalidOperationException("The user is not logged in!");

        return !httpContext.User.HasClaim(claim => claim.Type == AfraAppClaimTypes.Id)
            ? throw new InvalidOperationException($"The user does not have a {AfraAppClaimTypes.Id} claim")
            : new Guid(httpContext.User.Claims.First(claim => claim.Type == AfraAppClaimTypes.Id).Value);
    }
}
