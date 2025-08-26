using Afra_App.Backbone.Calendar.Services;

namespace Afra_App.Backbone.Calendar.Extensions;

///
public static class AppBuilderExtension
{
    ///
    public static void AddCalendar(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<CalendarService>();
    }
}
