using Afra_App.Services.Otium;

namespace Afra_App.Endpoints.Otium;

/// <summary>
///     Contains endpoints for managing kategories.
/// </summary>
public static class KategorienEndpoints
{
    /// <summary>
    ///     Maps the kategorie endpoints to the given <see cref="IEndpointRouteBuilder" />.
    /// </summary>
    /// <param name="app"></param>
    public static void MapKategorienEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/kategorie",
            (KategorieService kategorienService) => kategorienService.GetKategorienTreeAsyncEnumerable());
    }
}