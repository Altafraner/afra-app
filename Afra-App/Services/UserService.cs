using System.Security.Claims;
using Afra_App.Authentication;
using Afra_App.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Afra_App.Services;

/// <summary>
/// A Service for handling user related operations, such as signing in.
/// </summary>
public class UserService
{
    private readonly AfraAppContext _context;
    
    public UserService(AfraAppContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Signs in the <see cref="Person"/> with the given id for the given <see cref="HttpContent"/>
    /// </summary>
    /// <param name="userId">The id of the <see cref="Person"/> to sign in</param>
    /// <param name="httpContext">The <see cref="HttpContent"/> of the current session</param>
    /// <exception cref="InvalidOperationException">The user with the given id does not exist.</exception>
    public async Task SignInAsync(Guid userId, HttpContext httpContext)
    {
        var user = await _context.People.FindAsync(userId);
        if (user is null) throw new InvalidOperationException("The user does not exist");
        var claimsPrincipal = GenerateClaimsPrincipal(user);

        await httpContext.SignInAsync(claimsPrincipal);
    }

    /// <summary>
    /// Generates a <see cref="ClaimsPrincipal"/> for the given <see cref="Person"/>
    /// </summary>
    /// <param name="user">The user to generate the <see cref="ClaimsPrincipal"/> for.</param>
    /// <returns>A <see cref="ClaimsPrincipal"/> for the user</returns>
    private static ClaimsPrincipal GenerateClaimsPrincipal(Person user)
    {
        var claims = new List<Claim>
            {
                new(AfraAppClaimTypes.Id, user.Id.ToString()),
                new(AfraAppClaimTypes.GivenName, user.FirstName),
                new(AfraAppClaimTypes.LastName, user.LastName)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            return new ClaimsPrincipal(identity);
    }
}