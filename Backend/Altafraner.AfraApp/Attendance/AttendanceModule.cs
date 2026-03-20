using Altafraner.AfraApp.Attendance.API.Endpoints;
using Altafraner.AfraApp.Attendance.Domain.Contracts;
using Altafraner.AfraApp.Attendance.Jobs;
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
        services.AddScoped<SimpleAttendanceNotificationService>();
        services.AddScoped<NotesService>();
        services.AddScoped<EmergencyUploadJob>();
        services.AddScoped<IAttendanceNotificationService, AttendanceNotificationService>();

        services.AddHostedService<EmergencyBackupScheduler>();
    }

    public void Configure(WebApplication app)
    {
        var group = app.MapGroup("/api/attendance")
            .RequireAuthorization();
        group.MapSupervisionEndpoints();
        group.MapNoteEndpoints();
        group.MapHub<AttendanceHub>("/hub")
            .RequireAuthorization(AuthorizationPolicies.TutorOnly);
    }
}
