using System.Security.Claims;
using Afra_App.Data;
using Afra_App.Data.People;
using Microsoft.AspNetCore.SignalR;

namespace Afra_App.Authentication;

/// <summary>
///     Contains a extension method for <see cref="HttpContext" /> to retrieve the <see cref="Person" /> associated with
///     the current authenticated user.
/// </summary>
public static class AfraAppHttpContextGetPersonExtension
{
    /// <summary>
    ///     Retrieves the Person associated with the current authenticated user from the database.
    /// </summary>
    /// <param name="httpContext">The current HttpContext.</param>
    /// <param name="dbContext">The database context to use for retrieving the Person.</param>
    /// <returns>The Person associated with the current authenticated user.</returns>
    /// <exception cref="InvalidOperationException">
    ///     Thrown if the user is not authenticated or does not have the required
    ///     claim.
    /// </exception>
    /// <exception cref="KeyNotFoundException">Thrown if the specified user does not exist in the database.</exception>
    public static Person GetPerson(this HttpContext httpContext, AfraAppContext dbContext)
    {
        return GetPerson(httpContext.User, dbContext);
    }

    /// <inheritdoc cref="GetPerson" />
    public static Task<Person> GetPersonAsync(this HttpContext httpContext, AfraAppContext dbContext)
    {
        return GetPersonAsync(httpContext.User, dbContext);
    }

    /// <summary>
    /// Retrieves the Person associated with the current authenticated user from the database.
    /// </summary>
    /// <param name="hubContext">The current hub context</param>
    /// <param name="dbContext">The current dbContext</param>
    /// <returns>The user in the database</returns>
    public static Task<Person> GetPersonAsync(this HubCallerContext hubContext, AfraAppContext dbContext)
    {
        return GetPersonAsync(hubContext.User!, dbContext);
    }

    /// <inheritdoc cref="GetPersonAsync(Microsoft.AspNetCore.SignalR.HubCallerContext,Afra_App.Data.AfraAppContext)"/>
    public static Person GetPerson(this HubCallerContext hubContext, AfraAppContext dbContext)
    {
        return GetPerson(hubContext.User!, dbContext);
    }

    private static async Task<Person> GetPersonAsync(ClaimsPrincipal principal, AfraAppContext dbContext)
    {
        if (principal.Identity?.IsAuthenticated ?? true)
            throw new InvalidOperationException("The user is not logged in!");

        if (!principal.HasClaim(claim => claim.Type == AfraAppClaimTypes.Id))
            throw new InvalidOperationException($"The user does not have a {AfraAppClaimTypes.Id} claim");

        var user = await dbContext.Personen.FindAsync(new Guid(principal.Claims
            .First(claim => claim.Type == AfraAppClaimTypes.Id).Value));

        if (user is null)
            throw new KeyNotFoundException("The specified User does not exist");

        return user;
    }

    private static Person GetPerson(ClaimsPrincipal principal, AfraAppContext dbContext)
    {
        if (principal.Identity?.IsAuthenticated ?? true)
            throw new InvalidOperationException("The user is not logged in!");

        if (!principal.HasClaim(claim => claim.Type == AfraAppClaimTypes.Id))
            throw new InvalidOperationException($"The user does not have a {AfraAppClaimTypes.Id} claim");

        var user = dbContext.Personen.Find(new Guid(principal.Claims
            .First(claim => claim.Type == AfraAppClaimTypes.Id).Value));

        if (user is null)
            throw new KeyNotFoundException("The specified User does not exist");

        return user;
    }
}