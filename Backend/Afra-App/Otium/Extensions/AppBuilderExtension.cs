using Altafraner.AfraApp.Otium.Configuration;
using Altafraner.AfraApp.Otium.Domain.Contracts.Services;
using Altafraner.AfraApp.Otium.Services;

namespace Altafraner.AfraApp.Otium.Extensions;

/// <summary>
///     A static class that contains extension methods for <see cref="WebApplicationBuilder" /> to add Otium services and
///     configuration.
/// </summary>
public static class AppBuilderExtension
{
    /// <summary>
    ///     Adds Otium services and configuration to the specified <see cref="WebApplicationBuilder" />.
    /// </summary>
    public static void AddOtium(this WebApplicationBuilder builder)
    {
        builder.Services.AddOptions<OtiumConfiguration>()
            .Bind(builder.Configuration.GetSection("Otium"))
            .Validate(OtiumConfiguration.Validate)
            .ValidateOnStart();

        builder.Services.AddScoped<BlockHelper>();
        builder.Services.AddScoped<KategorieService>();
        builder.Services.AddScoped<OtiumEndpointService>();
        builder.Services.AddScoped<EnrollmentService>();
        builder.Services.AddScoped<ManagementService>();
        builder.Services.AddScoped<IAttendanceService, AttendanceService>();

        builder.Services.AddHostedService<EnrollmentReminderScheduler>();
        builder.Services.AddHostedService<StudentMisbehaviourNotificationScheduler>();
        builder.Services.AddHostedService<EmergencyBackupScheduler>();

        builder.Services.AddRules();
    }
}
