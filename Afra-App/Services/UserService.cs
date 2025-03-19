using System.Security.Claims;
using Afra_App.Authentication;
using Afra_App.Data;
using Afra_App.Data.DTO;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Person = Afra_App.Data.People.Person;

namespace Afra_App.Services;

/// <summary>
///     A Service for handling user related operations, such as signing in.
/// </summary>
public class UserService
{
    private readonly AfraAppContext _context;

    /// <summary>
    ///     Creates a new user service.
    /// </summary>
    public UserService(AfraAppContext context)
    {
        _context = context;
    }

    /// <summary>
    ///     Signs in the <see cref="Data.People.Person" /> with the given id for the given <see cref="HttpContent" />
    /// </summary>
    /// <param name="userId">The id of the <see cref="Data.People.Person" /> to sign in</param>
    /// <param name="httpContext">The <see cref="HttpContent" /> of the current session</param>
    /// <exception cref="InvalidOperationException">The user with the given id does not exist.</exception>
    public async Task SignInAsync(Guid userId, HttpContext httpContext)
    {
        var user = await _context.Personen.FindAsync(userId);
        if (user is null) throw new InvalidOperationException("The user does not exist");
        await SignInAsync(user, httpContext);
    }

    /// <summary>
    ///     Signs out the current user.
    /// </summary>
    /// <param name="httpContext">The <see cref="HttpContent" /> of the current session</param>
    public async Task SignOutAsync(HttpContext httpContext)
    {
        await httpContext.SignOutAsync();
    }

    /// <summary>
    ///     Handles a sign in request for a given username and password.
    /// </summary>
    /// <remarks>This is currently insecure.</remarks>
    /// <param name="request">The SignInRequest</param>
    /// <param name="httpContext">The current HttpContext</param>
    /// <returns>Ok, if the credentials are valid; Otherwise, Unauthorized</returns>
    public async Task<IResult> HandleSignInRequestAsync(SignInRequest request, HttpContext httpContext)
    {
        var user = await FindUserWithUsernameAsync(request.Username);
        if (user is null) return Results.Unauthorized();
        var authorized = await CheckPasswordAsync(user, request.Password);
        if (!authorized) return Results.Unauthorized();

        await SignInAsync(user, httpContext);
        return Results.Ok();
    }

    private Task<bool> CheckPasswordAsync(Person user, string password)
    {
        return Task.FromResult(true);
    }

    private async Task<Person?> FindUserWithUsernameAsync(string username)
    {
        return await _context.Personen.FirstOrDefaultAsync(u => u.Email == username);
    }

    private static async Task SignInAsync(Person user, HttpContext httpContext)
    {
        var claimsPrincipal = GenerateClaimsPrincipal(user);
        await httpContext.SignInAsync(claimsPrincipal);
    }

    /// <summary>
    ///     Check whether the current user is authorized.
    /// </summary>
    /// <param name="httpContext">The current HttpContext</param>
    /// <returns>The logged-in user if the user is authorized; Otherwise, unauthorized.</returns>
    public async Task<IResult> IsAuthorized(HttpContext httpContext)
    {
        // Check if the user is authenticated
        if (!(httpContext.User.Identity?.IsAuthenticated ?? false)) return Results.Unauthorized();

        try
        {
            // Retrieve the Person associated with the current user and return it
            var person = await httpContext.GetPersonAsync(_context);
            return Results.Ok(new PersonInfoMinimal(person));
        }
        catch (Exception e) when (e is InvalidOperationException or KeyNotFoundException)
        {
            // Sign out the user if they are (for some bizarre reason) authenticated, but the Person could not be found
            await httpContext.SignOutAsync();
            return Results.Unauthorized();
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
            new(AfraAppClaimTypes.LastName, user.Nachname)
        };

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