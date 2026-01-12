using Altafraner.AfraApp.Calendar.API.Endpoints;
using Altafraner.AfraApp.Calendar.Services;
using Altafraner.AfraApp.Otium;
using Altafraner.AfraApp.Profundum;
using Altafraner.AfraApp.User;
using Altafraner.Backbone.Abstractions;

namespace Altafraner.AfraApp.Calendar;

/// <summary>
///     A Module for calendar integrations
/// </summary>
[DependsOn<UserModule>]
[DependsOn<DatabaseModule>]
[DependsOn<OtiumModule>]
[DependsOn<ProfundumModule>]
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
