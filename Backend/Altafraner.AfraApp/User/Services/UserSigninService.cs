using System.Security.Claims;
using Altafraner.AfraApp.Backbone.Auth;
using Altafraner.AfraApp.User.Domain.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Models_Person = Altafraner.AfraApp.User.Domain.Models.Person;

namespace Altafraner.AfraApp.User.Services;

/// <summary>
///     A Service for handling user related operations, such as signing in.
/// </summary>
public class UserSigninService
{
    private readonly AfraAppContext _dbContext;
    private readonly IAuthenticationLifetimeService _authenticationLifetimeService;

    /// <summary>
    ///     Creates a new user service.
    /// </summary>
    public UserSigninService(AfraAppContext dbContext, IAuthenticationLifetimeService authenticationLifetimeService)
    {
        _dbContext = dbContext;
        _authenticationLifetimeService = authenticationLifetimeService;
    }

    /// <summary>
    ///     Signs in the <see cref="Person" /> with the given id for the given <see cref="HttpContent" />
    /// </summary>
    /// <param name="userId">The id of the <see cref="Person" /> to sign in</param>
    /// <param name="rememberMe">Whether to issue a persistent cookie</param>
    /// <param name="impersonatingUserId">Contains the id of the user that is starting an impersonation</param>
    /// <exception cref="InvalidOperationException">The user with the given id does not exist.</exception>
    public async Task SignInAsync(Guid userId, bool rememberMe, Guid? impersonatingUserId = null)
    {
        var user = await _dbContext.Personen.FindAsync(userId);
        if (user is null) throw new InvalidOperationException("The user does not exist");
        await SignInAsync(user, rememberMe, impersonatingUserId);
    }

    private async Task SignInAsync(Models_Person user, bool rememberMe, Guid? impersonatingUserId = null)
    {
        var claims = GenerateClaims(user);

        if (impersonatingUserId.HasValue)
            claims.Add(new Claim(AfraAppClaimTypes.ImpersonatingUserId, impersonatingUserId.Value.ToString()));

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        var claimsPrincipal = new ClaimsPrincipal(identity);

        await _authenticationLifetimeService.SignInAsync(claimsPrincipal, rememberMe);
    }

    /// <summary>
    ///     Generates a <see cref="ClaimsPrincipal" /> for the given <see cref="Person" />
    /// </summary>
    /// <param name="user">The user to generate the <see cref="ClaimsPrincipal" /> for.</param>
    /// <returns>A <see cref="ClaimsPrincipal" /> for the user</returns>
    internal static List<Claim> GenerateClaims(Models_Person user)
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

        return claims;
    }
}
