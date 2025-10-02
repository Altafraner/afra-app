using System.Net.Http.Headers;
using System.Text;
using Altafraner.AfraApp.Backbone.EmergencyBackup.Configuration;
using Altafraner.AfraApp.Backbone.EmergencyBackup.Services.Contracts;
using Altafraner.AfraApp.Backbone.EmergencyBackup.Services.Implementations;
using Microsoft.Extensions.Options;

namespace Altafraner.AfraApp.Backbone.EmergencyBackup.Extensions;

/// <summary>
/// Contains a static extension method to add the Emergency Post Backup service to the WebApplicationBuilder.
/// </summary>
public static class AppBuilderExtension
{
    /// <summary>
    /// Adds the Emergency Post Backup service to the WebApplicationBuilder.
    /// </summary>
    public static void AddEmergencyPostBackup(this WebApplicationBuilder builder)
    {
        builder.Services.AddOptions<FilePostConfiguration>()
            .Bind(builder.Configuration.GetSection("EmergencyBackup"))
            .ValidateOnStart();
        builder.Services.AddTransient<IEmergencyBackupService, FilePostEmergencyBackup>();
        builder.Services.AddHttpClient(FilePostConfiguration.HttpClientName, ConfigureHttpClient);
    }

    private static void ConfigureHttpClient(IServiceProvider serviceProvider, HttpClient client)
    {
        var config = serviceProvider.GetRequiredService<IOptions<FilePostConfiguration>>().Value;
        client.BaseAddress = new Uri(config.BaseUri);
        if (string.IsNullOrWhiteSpace(config.Username) || string.IsNullOrWhiteSpace(config.Password)) return;
        var byteArray = Encoding.ASCII.GetBytes($"{config.Username}:{config.Password}");
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
    }
}
