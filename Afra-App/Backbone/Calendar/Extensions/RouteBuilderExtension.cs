using Afra_App.Backbone.Calendar.API.Endpoints;

namespace Afra_App.Backbone.Calendar.Extensions;

///
public static class RouteBuilderExtension
{
    ///
    public static void MapCalendar(this WebApplication app)
    {
        app.MapCalendarEndpoints();
    }
}
