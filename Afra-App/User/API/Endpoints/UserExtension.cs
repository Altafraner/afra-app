using Afra_App.User.Services;

namespace Afra_App.User.API.Endpoints;

/// <summary>
///     Extension Methods for the <see cref="UserSigninService" /> class.
/// </summary>
public static class UserExtension
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
            async (UserSigninService userSigninService) => await userSigninService.GetAuthorized());

        app.MapGet("/api/user/logout",
                async (UserSigninService userSigninService) => await userSigninService.SignOutAsync())
            .RequireAuthorization();
    }
}
