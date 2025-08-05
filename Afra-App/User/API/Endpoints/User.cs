using Afra_App.Backbone.Authentication;
using Afra_App.User.Domain.DTO;
using Afra_App.User.Services;

namespace Afra_App.User.API.Endpoints;

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
            .WithOpenApi()
            .AllowAnonymous();

        app.MapGet("/api/user",
            async (UserSigninService userSigninService) =>
            {
                var user = await userSigninService.GetAuthorized();
                return user is null
                    ? Results.Unauthorized()
                    : Results.Ok(new PersonLoginInfo
                    {
                        Id = user.Id,
                        Vorname = user.Vorname,
                        Nachname = user.Nachname,
                        Rolle = user.Rolle,
                        Berechtigungen = user.GlobalPermissions.ToArray()
                    });
            });

        app.MapGet("/api/user/logout",
                async (UserSigninService userSigninService) => await userSigninService.SignOutAsync())
            .RequireAuthorization();

        app.MapGet("/api/user/{id:guid}/impersonate",
                async (UserSigninService userSigninService, ILogger<Program> logger, Guid id) =>
                {
                    logger.LogWarning("Impersonating user with ID {Id}", id);
                    await userSigninService.SignInAsync(id);
                })
            .RequireAuthorization(AuthorizationPolicies.AdminOnly);
    }
}
