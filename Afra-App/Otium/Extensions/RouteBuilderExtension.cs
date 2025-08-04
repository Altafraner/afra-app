using Afra_App.Backbone.Authentication;
using Afra_App.Otium.API.Endpoints;
using Afra_App.Otium.API.Hubs;

namespace Afra_App.Otium.Extensions;

/// <summary>
/// A static class that contains extension methods for <see cref="IEndpointRouteBuilder"/> to map Otium-related endpoints.
/// </summary>
public static class RouteBuilderExtension
{
    /// <summary>
    /// Maps the Otium endpoints to the given <see cref="IEndpointRouteBuilder"/>.
    /// </summary>
    public static void MapOtium(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/otium")
            .WithOpenApi()
            .RequireAuthorization();
        app.MapSchuljahr();
        group.MapKategorienEndpoints();
        group.MapKatalogEndpoints();
        group.MapDashboardEndpoints();
        group.MapManagementEndpoints();
        group.MapHub<AttendanceHub>("/attendance")
            .RequireAuthorization(AuthorizationPolicies.TutorOnly);
    }
}
