using System.Security.Claims;
using Altafraner.AfraApp.Backbone.Authorization;
using Altafraner.AfraApp.User.Domain.DTO;
using Altafraner.AfraApp.User.Services;
using Altafraner.Backbone.CookieAuthentication;

namespace Altafraner.AfraApp.User.API.Endpoints;

/// <summary>
///     Extension Methods for the <see cref="UserSigninService" /> class.
/// </summary>
public static class User
{
    /// <summary>
    ///     Maps the user endpoints to the given <see cref="IEndpointRouteBuilder" />.
    /// </summary>
    public static void MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/user/login",
                async (UserSigninService userSigninService, UserSigninService.SignInRequest request,
                        IWebHostEnvironment environment) =>
                    await userSigninService.HandleSignInRequestAsync(request, environment))
            .WithName("sign-in")
            .AllowAnonymous();

        app.MapGet("/api/user",
            async (UserAccessor userAccessor, ClaimsPrincipal claimsPrincipal) =>
            {
                try
                {
                    var user = await userAccessor.GetUserAsync();
                    var impersonationId = claimsPrincipal.FindFirst(AfraAppClaimTypes.ImpersonatingUserId)?.Value;
                    return Results.Ok(new PersonLoginInfo
                    {
                        Id = user.Id,
                        Vorname = user.FirstName,
                        Nachname = user.LastName,
                        Rolle = user.Rolle,
                        Berechtigungen = user.GlobalPermissions.ToArray(),
                        ImpersonationId = impersonationId
                    });
                }
                catch (InvalidOperationException)
                {
                    return Results.Unauthorized();
                }
            });

        app.MapGet("/api/user/logout",
                async (IAuthenticationLifetimeService authenticationLifetimeService) =>
                    await authenticationLifetimeService.SignOutAsync())
            .RequireAuthorization();

        app.MapGet("/api/user/{id:guid}/impersonate",
                async (UserSigninService userSigninService,
                    ILogger<Program> logger,
                    Guid id,
                    UserAccessor userAccessor,
                    ClaimsPrincipal claimsPrincipal) =>
                {
                    var impersonationId = claimsPrincipal.FindFirst(AfraAppClaimTypes.ImpersonatingUserId)?.Value;
                    var currentUserId = impersonationId is not null
                        ? Guid.Parse(impersonationId)
                        : userAccessor.GetUserId();
                    logger.LogWarning("{oldUser} is Impersonating user with ID {newUser}", currentUserId, id);
                    await userSigninService.SignInAsync(id, false, currentUserId);
                })
            .RequireAuthorization(AuthorizationPolicies.AdminOnly);
    }
}
