using Altafraner.AfraApp.Profundum.API.Endpoints;

namespace Altafraner.AfraApp.Profundum.Extensions;

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
            .WithOpenApi();
        group.MapEnrollmentEndpoints();
        group.MapManagementEndpoints();
    }
}
