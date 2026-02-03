using Altafraner.AfraApp.Otium.Services;

namespace Altafraner.AfraApp.Otium.API.Endpoints;

/// <summary>
///     Contains endpoints for managing kategories.
/// </summary>
public static class Kategorien
{
    /// <summary>
    ///     Maps the kategorie endpoints to the given <see cref="IEndpointRouteBuilder" />.
    /// </summary>
    /// <param name="app"></param>
    public static void MapKategorienEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet(
            "/kategorie",
            (KategorieService kategorienService) =>
                kategorienService.GetKategorienTreeAsyncEnumerable()
        );
    }
}
