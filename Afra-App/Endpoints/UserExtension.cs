using Afra_App.Services;

namespace Afra_App.Endpoints;

/// <summary>
/// Extension Methods for the <see cref="UserService" /> class.
/// </summary>
public static class UserExtension
{

    /// <summary>
    /// Maps the user endpoints to the given <see cref="IEndpointRouteBuilder" />.
    /// </summary>
    public static void MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/user/login", async (UserService userService, UserService.SignInRequest request, HttpContext context) => await userService.HandleSignInRequestAsync(request, context)).WithName("sign-in")
        .WithOpenApi()
        .AllowAnonymous();

        app.MapGet("/api/user",
            async (UserService userService, HttpContext context) => await userService.IsAuthorized(context));

        app.MapGet("/api/user/logout",
            async (UserService userService, HttpContext context) => await userService.SignOutAsync(context));
    }
}