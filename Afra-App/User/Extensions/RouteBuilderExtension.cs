using Afra_App.User.API.Endpoints;

namespace Afra_App.User.Extensions;

/// <summary>
/// A static class that contains extension methods for <see cref="WebApplication"/> to map user-related endpoints.
/// </summary>
public static class RouteBuilderExtension
{
    /// <summary>
    /// Maps user-related endpoints to the given <see cref="WebApplication"/>.
    /// </summary>
    public static void MapUser(this WebApplication app)
    {
        app.MapUserEndpoints();
        app.MapPeopleEndpoints();
    }
}
