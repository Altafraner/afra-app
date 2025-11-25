using Altafraner.AfraApp.Backbone.Authorization;
using Altafraner.AfraApp.Otium.API.Endpoints;
using Altafraner.AfraApp.Otium.API.Hubs;
using Altafraner.AfraApp.Otium.Configuration;
using Altafraner.AfraApp.Otium.Domain.Contracts.Rules;
using Altafraner.AfraApp.Otium.Domain.Contracts.Services;
using Altafraner.AfraApp.Otium.Services;
using Altafraner.AfraApp.Otium.Services.Rules;
using Altafraner.AfraApp.User;
using Altafraner.Backbone.Abstractions;

namespace Altafraner.AfraApp.Otium;

/// <summary>
///     A Module for handling the Otium
/// </summary>
[DependsOn<UserModule>]
[DependsOn<DatabaseModule>]
public class OtiumModule : IModule
{
    /// <inheritdoc />
    public void ConfigureServices(IServiceCollection services, IConfiguration config, IHostEnvironment env)
    {
        services.AddOptions<OtiumConfiguration>()
            .Bind(config.GetSection("Otium"))
            .Validate(OtiumConfiguration.Validate)
            .ValidateOnStart();

        services.AddScoped<BlockHelper>();
        services.AddScoped<KategorieService>();
        services.AddScoped<OtiumEndpointService>();
        services.AddScoped<EnrollmentService>();
        services.AddScoped<ManagementService>();
        services.AddScoped<IAttendanceService, AttendanceService>();
        services.AddScoped<AttendanceRealtimeService>();
        services.AddScoped<NotesService>();

        services.AddHostedService<EnrollmentReminderScheduler>();
        services.AddHostedService<StudentMisbehaviourNotificationScheduler>();
        services.AddHostedService<EmergencyBackupScheduler>();

        AddRules(services);
    }

    /// <inheritdoc />
    public void Configure(WebApplication app)
    {
        var group = app.MapGroup("/api/otium")
            .WithOpenApi()
            .RequireAuthorization();
        group.MapKategorienEndpoints();
        group.MapKatalogEndpoints();
        group.MapDashboardEndpoints();
        group.MapManagementEndpoints();
        group.MapNoteEndpoints();
        group.MapHub<AttendanceHub>("/attendance")
            .RequireAuthorization(AuthorizationPolicies.TutorOnly);
    }

    private static void AddRules(IServiceCollection services)
    {
        services.AddScoped<IBlockRule, AlwaysAttendedRule>();
        services.AddScoped<IBlockRule, MustEnrollRule>();
        services.AddScoped<IBlockRule, ParallelEnrollmentRule>();

        services.AddScoped<IIndependentRule, EnrollmentTimeframeRule>();
        services.AddScoped<IIndependentRule, MaxEnrollmentsRule>();
        services.AddScoped<IIndependentRule, MustBeStudentRule>();
        services.AddScoped<IIndependentRule, NotCancelledRule>();
        services.AddScoped<IIndependentRule, KlassenLimitsRule>();

        services.AddScoped<IWeekRule, RequiredKategorienRule>();

        services.AddScoped<IRulesFactory, ServiceProviderRulesFactory>();
        services.AddScoped<RulesValidationService>();
    }
}
