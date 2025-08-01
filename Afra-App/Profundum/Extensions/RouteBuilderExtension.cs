using Afra_App.Profundum.API.Endpoints;

namespace Afra_App.Profundum.Extensions;

/// <summary>
/// A static class that contains extension methods for <see cref="IEndpointRouteBuilder"/> to map Profundum-related endpoints.
/// </summary>
public static class RouteBuilderExtension
{
    /// <summary>
    /// Maps the Profundum endpoints to the given <see cref="IEndpointRouteBuilder"/>.
    /// </summary>
    public static void MapProfundum(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/profundum")
            .WithOpenApi()
            .RequireAuthorization();
        group.MapEnrollmentEndpoints();
    }
}
