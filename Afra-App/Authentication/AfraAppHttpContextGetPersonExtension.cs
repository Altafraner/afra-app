using Afra_App.Data;
using Afra_App.Data.People;

namespace Afra_App.Authentication;

/// <summary>
/// Contains a extension method for <see cref="HttpContext"/> to retrieve the <see cref="Person"/> associated with the current authenticated user.
/// </summary>
public static class AfraAppHttpContextGetPersonExtension
{
    /// <summary>
    /// Retrieves the Person associated with the current authenticated user from the database.
    /// </summary>
    /// <param name="httpContext">The current HttpContext.</param>
    /// <param name="dbContext">The database context to use for retrieving the Person.</param>
    /// <returns>The Person associated with the current authenticated user.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the user is not authenticated or does not have the required claim.</exception>
    /// <exception cref="KeyNotFoundException">Thrown if the specified user does not exist in the database.</exception>
    public static Person GetPerson(this HttpContext httpContext, AfraAppContext dbContext)
    {
        if (!httpContext.User.Identity?.IsAuthenticated ?? true)
            throw new InvalidOperationException("The user is not logged in!");

        if (!httpContext.User.HasClaim((claim) => claim.Type == AfraAppClaimTypes.Id))
            throw new InvalidOperationException($"The user does not have a {AfraAppClaimTypes.Id} claim");

        var user = dbContext.Personen.Find(new Guid(httpContext.User.Claims
            .First(claim => claim.Type == AfraAppClaimTypes.Id).Value));

        if (user is null)
            throw new KeyNotFoundException("The specified User does not exist");

        return user;
    }
    
    /// <inheritdoc cref="GetPerson"/>
    public static async Task<Person> GetPersonAsync(this HttpContext httpContext, AfraAppContext dbContext)
    {
        if (!httpContext.User.Identity?.IsAuthenticated ?? true)
            throw new InvalidOperationException("The user is not logged in!");

        if (!httpContext.User.HasClaim((claim) => claim.Type == AfraAppClaimTypes.Id))
            throw new InvalidOperationException($"The user does not have a {AfraAppClaimTypes.Id} claim");

        var user = await dbContext.Personen.FindAsync(new Guid(httpContext.User.Claims
            .First(claim => claim.Type == AfraAppClaimTypes.Id).Value));

        if (user is null)
            throw new KeyNotFoundException("The specified User does not exist");

        return user;
    }
}