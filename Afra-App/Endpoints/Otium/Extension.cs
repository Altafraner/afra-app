namespace Afra_App.Endpoints.Otium;

/// <summary>
///     This class contains extension methods for the Otium endpoints.
/// </summary>
public static class Extension
{
    /// <summary>
    ///     Maps the Otium endpoints to the given <see cref="IEndpointRouteBuilder" />.
    /// </summary>
    /// <param name="app"></param>
    public static void MapOtium(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/otium")
            .WithOpenApi()
            .RequireAuthorization();
        group.MapKategorienEndpoints();
        group.MapKatalogEndpoints();
        group.MapDashboardEndpoints();
        group.MapManagementEndpoints();
    }
}
