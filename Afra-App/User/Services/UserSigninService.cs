using System.Security.Claims;
using Afra_App.Backbone.Authentication;
using Afra_App.User.Services.LDAP;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Person = Afra_App.User.Domain.Models.Person;

namespace Afra_App.User.Services;

/// <summary>
///     A Service for handling user related operations, such as signing in.
/// </summary>
public class UserSigninService
{
    private readonly AfraAppContext _dbContext;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly LdapService _ldapService;
    private readonly UserAccessor _userAccessor;

    /// <summary>
    ///     Creates a new user service.
    /// </summary>
    public UserSigninService(AfraAppContext dbContext, LdapService ldapService,
        IHttpContextAccessor httpContextAccessor,
        UserAccessor userAccessor)
    {
        _dbContext = dbContext;
        _ldapService = ldapService;
        _httpContextAccessor = httpContextAccessor;
        _userAccessor = userAccessor;
    }

    /// <summary>
    ///     Signs in the <see cref="Person" /> with the given id for the given <see cref="HttpContent" />
    /// </summary>
    /// <param name="userId">The id of the <see cref="Person" /> to sign in</param>
    /// <exception cref="InvalidOperationException">The user with the given id does not exist.</exception>
    public async Task SignInAsync(Guid userId)
    {
        var user = await _dbContext.Personen.FindAsync(userId);
        if (user is null) throw new InvalidOperationException("The user does not exist");
        await SignInAsync(user);
    }

    /// <summary>
    ///     Signs out the current user.
    /// </summary>
    public async Task SignOutAsync()
    {
        await _httpContextAccessor.HttpContext!.SignOutAsync();
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
        var user = (_ldapService.IsEnabled, environment.IsDevelopment()) switch
        {
            (true, _) => await _ldapService.VerifyUserAsync(request.Username.Trim(), request.Password.Trim()),
            (false, true) => await _dbContext.Personen.FirstOrDefaultAsync(u =>
                u.Email.StartsWith(request.Username.Trim())),
            _ => null
        };

        if (user is null) return Results.Unauthorized();

        await SignInAsync(user);
        return Results.Ok();
    }

    private async Task SignInAsync(Person user)
    {
        var claimsPrincipal = GenerateClaimsPrincipal(user);
        await _httpContextAccessor.HttpContext!.SignInAsync(claimsPrincipal);
    }

    /// <summary>
    ///     Check whether the current user is authorized.
    /// </summary>
    /// <returns>The logged-in user if the user is authorized; Otherwise, unauthorized.</returns>
    public async Task<Person?> GetAuthorized()
    {
        // Check if the user is authenticated
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.User.Identity is null || !httpContext.User.Identity.IsAuthenticated)
            return null;

        try
        {
            // Retrieve the Person associated with the current user and return it
            var person = await _userAccessor.GetUserAsync();
            return person;
        }
        catch (Exception e) when (e is InvalidOperationException or KeyNotFoundException)
        {
            // Sign out the user if they are (for some bizarre reason) authenticated, but the Person could not be found
            await httpContext.SignOutAsync();
            return null;
        }
    }

    /// <summary>
    ///     Generates a <see cref="ClaimsPrincipal" /> for the given <see cref="Person" />
    /// </summary>
    /// <param name="user">The user to generate the <see cref="ClaimsPrincipal" /> for.</param>
    /// <returns>A <see cref="ClaimsPrincipal" /> for the user</returns>
    private static ClaimsPrincipal GenerateClaimsPrincipal(Person user)
    {
        var claims = new List<Claim>
        {
            new(AfraAppClaimTypes.Id, user.Id.ToString()),
            new(AfraAppClaimTypes.GivenName, user.Vorname),
            new(AfraAppClaimTypes.LastName, user.Nachname),
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
    public record SignInRequest(string Username, string Password);
}
