using System.ComponentModel.DataAnnotations;
using Altafraner.AfraApp.Attendance.AbsenceProviders.Cevex;
using Altafraner.AfraApp.Attendance.API.Endpoints;
using Altafraner.AfraApp.Attendance.Configuration;
using Altafraner.AfraApp.Attendance.Domain.Contracts;
using Altafraner.AfraApp.Attendance.Jobs;
using Altafraner.AfraApp.Attendance.Services;
using Altafraner.AfraApp.Backbone.Authorization;
using Altafraner.Backbone.Abstractions;
using Altafraner.Backbone.Defaults;
using AttendanceHub = Altafraner.AfraApp.Attendance.API.Hubs.AttendanceHub;

namespace Altafraner.AfraApp.Attendance;

[DependsOn<CachingModule>]
internal class AttendanceModule : IModule
{
    public void ConfigureServices(IServiceCollection services, IConfiguration config, IHostEnvironment env)
    {
        var section = config.GetSection("Attendance");
        services.AddOptions<AttendanceConfiguration>().Bind(section);

        var settings = section.Exists()
            ? section.Get<AttendanceConfiguration>() ??
              throw new ValidationException("Cannot bind CookieAuthenticationSettings")
            : new AttendanceConfiguration();

        services.AddScoped<IAttendanceService, AttendanceService>();
        services.AddScoped<SimpleAttendanceNotificationService>();
        services.AddScoped<NotesService>();
        services.AddScoped<EmergencyUploadJob>();
        services.AddScoped<IAttendanceNotificationService, AttendanceNotificationService>();

        if (settings.Cevex is not null && !string.IsNullOrWhiteSpace(settings.Cevex.FilePath))
        {
            services.AddScoped<CevexDataParser>();
            services.AddScoped<IAttendanceAutomaticEntryProvider, CevexAttendanceEntryProvider>();
        }

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
