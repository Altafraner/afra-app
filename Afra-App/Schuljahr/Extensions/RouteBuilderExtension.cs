using Altafraner.AfraApp.Schuljahr.API.Endpoints;

namespace Altafraner.AfraApp.Schuljahr.Extensions;

/// <summary>
/// A static class that contains extension methods for <see cref="IEndpointRouteBuilder"/> to map Schuljahr-related endpoints.
/// </summary>
public static class RouteBuilderExtension
{
    /// <summary>
    /// Maps the Schuljahr endpoints to the given <see cref="IEndpointRouteBuilder"/>.
    /// </summary>
    public static void MapSchuljahr(this IEndpointRouteBuilder app)
    {
        app.MapSchuljahrEndpoints();
    }
}
