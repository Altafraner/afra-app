using Altafraner.AfraApp.Attendance.API.Endpoints;
using Altafraner.AfraApp.Attendance.Domain.Contracts;
using Altafraner.AfraApp.Attendance.Services;
using Altafraner.AfraApp.Backbone.Authorization;
using Altafraner.Backbone.Abstractions;
using AttendanceHub = Altafraner.AfraApp.Attendance.API.Hubs.AttendanceHub;

namespace Altafraner.AfraApp.Attendance;

internal class AttendanceModule : IModule
{
    public void ConfigureServices(IServiceCollection services, IConfiguration config, IHostEnvironment env)
    {
        services.AddScoped<IAttendanceService, AttendanceService>();
        services.AddScoped<AttendanceRealtimeService>();
        services.AddScoped<NotesService>();
    }

    public void Configure(WebApplication app)
    {
        var group = app.MapGroup("/attendance")
            .RequireAuthorization();
        group.MapNoteEndpoints();
        group.MapHub<AttendanceHub>("/attendance")
            .RequireAuthorization(AuthorizationPolicies.TutorOnly);
    }
}
