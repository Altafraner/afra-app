using Altafraner.AfraApp.Backbone.Authorization;
using Altafraner.AfraApp.Otium.API.Endpoints;
using Altafraner.AfraApp.Otium.API.Hubs;

namespace Altafraner.AfraApp.Otium.Extensions;

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
        group.MapKategorienEndpoints();
        group.MapKatalogEndpoints();
        group.MapDashboardEndpoints();
        group.MapManagementEndpoints();
        group.MapHub<AttendanceHub>("/attendance")
            .RequireAuthorization(AuthorizationPolicies.TutorOnly);
    }
}