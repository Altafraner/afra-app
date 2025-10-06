using System.Security.Claims;
using Altafraner.AfraApp.Backbone.Authorization;
using Altafraner.AfraApp.User.Domain.Models;
using Altafraner.AfraApp.User.Services.LDAP;
using Altafraner.Backbone.CookieAuthentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Models_Person = Altafraner.AfraApp.User.Domain.Models.Person;

namespace Altafraner.AfraApp.User.Services;

/// <summary>
///     A Service for handling user related operations, such as signing in.
/// </summary>
public class UserSigninService
{
    private readonly IMemoryCache _cache;
    private readonly AfraAppContext _dbContext;
    private readonly LdapService _ldapService;
    private readonly IAuthenticationLifetimeService _authenticationLifetimeService;

    /// <summary>
    ///     Creates a new user service.
    /// </summary>
    public UserSigninService(AfraAppContext dbContext, LdapService ldapService, IMemoryCache cache,
        IAuthenticationLifetimeService authenticationLifetimeService)
    {
        _dbContext = dbContext;
        _ldapService = ldapService;
        _cache = cache;
        _authenticationLifetimeService = authenticationLifetimeService;
    }

    /// <summary>
    ///     Signs in the <see cref="Person" /> with the given id for the given <see cref="HttpContent" />
    /// </summary>
    /// <param name="userId">The id of the <see cref="Person" /> to sign in</param>
    /// <param name="rememberMe">Whether to issue a persistent cookie</param>
    /// <exception cref="InvalidOperationException">The user with the given id does not exist.</exception>
    public async Task SignInAsync(Guid userId, bool rememberMe)
    {
        var user = await _dbContext.Personen.FindAsync(userId);
        if (user is null) throw new InvalidOperationException("The user does not exist");
        await SignInAsync(user, rememberMe);
    }

    /// <summary>
    ///     Handles a sign in request for a given username and password.
    /// </summary>
    /// <remarks>This is currently insecure.</remarks>
    /// <param name="request">The SignInRequest</param>
    /// <param name="environment">The application environment</param>
    /// <returns>Ok, if the credentials are valid; Otherwise, unauthorized</returns>
    public async Task<IResult> HandleSignInRequestAsync(SignInRequest request, IWebHostEnvironment environment)
    {
        var cacheResults = _cache.GetOrCreate($"user:login:{request.Username.ToLower()}", _ => new List<DateTime>()) ??
                           [];
        var now = DateTime.Now;

        // Check if the user has tried to log in too many times in the last 5 minutes
        cacheResults.RemoveAll(t => t < now.AddMinutes(-5));
        if (cacheResults.Count >= 5)
            return Results.Problem("Zu viele Anmeldeversuche. Bitte versuchen Sie es später erneut.",
                statusCode: StatusCodes.Status429TooManyRequests);

        var user = (_ldapService.IsEnabled, environment.IsDevelopment()) switch
        {
            (true, _) => await _ldapService.VerifyUserAsync(request.Username.Trim(), request.Password.Trim()),
            (false, true) => await _dbContext.Personen.FirstOrDefaultAsync(u =>
                u.Email.StartsWith(request.Username.Trim())),
            _ => null
        };

        if (user is null)
        {
            cacheResults.Add(now);
            _cache.Set($"user:login:{request.Username.ToLower()}", cacheResults, TimeSpan.FromMinutes(10));
            return Results.Unauthorized();
        }

        await SignInAsync(user, request.RememberMe);
        return Results.Ok();
    }

    private async Task SignInAsync(Models_Person user, bool rememberMe)
    {
        var claimsPrincipal = GenerateClaimsPrincipal(user);
        await _authenticationLifetimeService.SignInAsync(claimsPrincipal, rememberMe);
    }

    /// <summary>
    ///     Generates a <see cref="ClaimsPrincipal" /> for the given <see cref="Person" />
    /// </summary>
    /// <param name="user">The user to generate the <see cref="ClaimsPrincipal" /> for.</param>
    /// <returns>A <see cref="ClaimsPrincipal" /> for the user</returns>
    private static ClaimsPrincipal GenerateClaimsPrincipal(Models_Person user)
    {
        var claims = new List<Claim>
        {
            new(AfraAppClaimTypes.Id, user.Id.ToString()),
            new(AfraAppClaimTypes.GivenName, user.FirstName),
            new(AfraAppClaimTypes.LastName, user.LastName),
            new(AfraAppClaimTypes.Role, user.Rolle.ToString())
        };

        claims.AddRange(user.GlobalPermissions.Select(perm =>
            new Claim(AfraAppClaimTypes.GlobalPermission, perm.ToString())));

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        return new ClaimsPrincipal(identity);
    }

    /// <summary>
    ///     A request to sign in a user.
    /// </summary>
    /// <param name="Username">The username of the user.</param>
    /// <param name="Password">The password of the user.</param>
    /// <param name="RememberMe">Whether to issue a persistent cookie</param>
    public record SignInRequest(string Username, string Password, bool RememberMe);
}