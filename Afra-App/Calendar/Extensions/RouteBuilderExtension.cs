using Afra_App.Calendar.API.Endpoints;

namespace Afra_App.Calendar.Extensions;

/// <summary>
///     A static class that contains extension methods for <see cref="WebApplication"/> to map calendar endpoints.
/// </summary>
public static class RouteBuilderExtension
{
    /// <summary>
    ///     Maps calendar endpoints to the given <see cref="WebApplication"/>.
    /// </summary>
    public static void MapCalendar(this WebApplication app)
    {
        app.MapCalendarEndpoints();
    }
}
