using Altafraner.AfraApp.Calendar.API.Endpoints;
using Altafraner.AfraApp.Calendar.Services;
using Altafraner.Backbone.Abstractions;

namespace Altafraner.AfraApp.Calendar;

/// <summary>
///     A Module for calendar integrations
/// </summary>
public class CalendarModule : IModule
{
    /// <inheritdoc />
    public void ConfigureServices(IServiceCollection services, IConfiguration config, IHostEnvironment env)
    {
        services.AddScoped<CalendarService>();
    }

    /// <inheritdoc />
    public void Configure(WebApplication app)
    {
        app.MapCalendarEndpoints();
    }
}
