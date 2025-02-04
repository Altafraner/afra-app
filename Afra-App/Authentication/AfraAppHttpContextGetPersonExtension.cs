using Afra_App.Models;

namespace Afra_App.Authentication;

public static class AfraAppHttpContextGetPersonExtension
{
    public static Person GetPerson(this HttpContext httpContext, AfraAppContext dbContext)
    {
        if (!httpContext.User.Identity?.IsAuthenticated ?? true)
            throw new InvalidOperationException("The user is not logged in!");

        if (!httpContext.User.HasClaim((claim) => claim.Type == AfraAppClaimTypes.Id))
            throw new InvalidOperationException($"The user does not have a {AfraAppClaimTypes.Id} claim");

        var user = dbContext.People.Find(new Guid(httpContext.User.Claims
            .First(claim => claim.Type == AfraAppClaimTypes.Id).Value));

        if (user is null)
            throw new Exception("The specified User does not exist");

        return user;
    }
}