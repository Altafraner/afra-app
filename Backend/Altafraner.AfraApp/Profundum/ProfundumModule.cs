using Altafraner.AfraApp.Profundum.API.Endpoints;
using Altafraner.AfraApp.Profundum.Configuration;
using Altafraner.AfraApp.Profundum.Extensions;
using Altafraner.AfraApp.Profundum.Services;
using Altafraner.AfraApp.Typst;
using Altafraner.AfraApp.User;
using Altafraner.Backbone.Abstractions;

namespace Altafraner.AfraApp.Profundum;

/// <summary>
///     A Module for handling the Profundum
/// </summary>
[DependsOn<UserModule>]
[DependsOn<DatabaseModule>]
[DependsOn<TypstModule>]
[DependsOn<GeneralConfigurationModule>]
internal class ProfundumModule : IModule
{
    public void ConfigureServices(IServiceCollection services, IConfiguration config, IHostEnvironment env)
    {
        services.AddOptions<ProfundumConfiguration>()
            .Bind(config.GetSection("Profundum"));

        services.AddScoped<ProfundumEnrollmentService>();
        services.AddScoped<ProfundumManagementService>();
        services.AddScoped<ProfundumMatchingService>();
        services.AddScoped<ProfundumFachbereicheService>();
        services.AddScoped<FeedbackAnkerService>();
        services.AddScoped<FeedbackKategorienService>();
        services.AddScoped<FeedbackPrintoutService>();
        services.AddScoped<FeedbackService>();

        services.AddRules();
    }

    public void Configure(WebApplication app)
    {
        var group = app.MapGroup("/api/profundum");
        group.MapEnrollmentEndpoints();
        group.MapManagementEndpoints();
        group.MapBewertungEndpoints();
    }
}
