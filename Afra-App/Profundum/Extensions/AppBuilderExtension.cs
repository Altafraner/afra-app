using Afra_App.Profundum.Configuration;
using Afra_App.Profundum.Services;

namespace Afra_App.Profundum.Extensions;

/// <summary>
/// A static class that contains extension methods for <see cref="WebApplicationBuilder"/> to add Profundum services and configuration.
/// </summary>
public static class AppBuilderExtension
{
    /// <summary>
    /// Adds Profundum services and configuration to the specified <see cref="WebApplicationBuilder"/>.
    /// </summary>
    public static void AddProfundum(this WebApplicationBuilder builder)
    {
        builder.Services.AddOptions<ProfundumConfiguration>()
            .Bind(builder.Configuration.GetSection("Profundum"))
            .Validate(ProfundumConfiguration.Validate)
            .ValidateOnStart();

        builder.Services.AddScoped<ProfundumEnrollmentService>();
        builder.Services.AddScoped<ProfundumManagementService>();
    }
}
