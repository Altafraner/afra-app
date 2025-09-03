using Afra_App.Schuljahr.Services;

namespace Afra_App.Schuljahr.Extensions;

/// <summary>
///     A static class that contains extension methods for <see cref="WebApplicationBuilder" /> to add Schuljahr services and
///     configuration.
/// </summary>
public static class AppBuilderExtension
{
    /// <summary>
    ///     Adds Schuljahr services and configuration to the specified <see cref="WebApplicationBuilder" />.
    /// </summary>
    public static void AddSchuljahr(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<SchuljahrService>();
    }
}
