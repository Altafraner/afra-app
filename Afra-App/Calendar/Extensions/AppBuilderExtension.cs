using Afra_App.Calendar.Services;

namespace Afra_App.Calendar.Extensions;

/// <summary>
///     A static class that provides extension methods for <see cref="WebApplicationBuilder"/> to add calendar services.
/// </summary>
public static class AppBuilderExtension
{
    /// <summary>
    ///     Adds calendar services to the <see cref="WebApplicationBuilder"/>.
    /// </summary>
    public static void AddCalendar(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<CalendarService>();
    }
}
