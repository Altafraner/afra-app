using System.Net.Http.Headers;
using System.Text;
using Altafraner.AfraApp.Backbone.EmergencyBackup.Configuration;
using Altafraner.AfraApp.Backbone.EmergencyBackup.Services.Contracts;
using Altafraner.AfraApp.Backbone.EmergencyBackup.Services.Implementations;
using Altafraner.Backbone.Abstractions;
using Microsoft.Extensions.Options;

namespace Altafraner.AfraApp.Backbone.EmergencyBackup;

/// <summary>
/// A module for pushing emergency backups to an external service.
/// </summary>
public class EmergencyBackupModule : IModule
{
    /// <inheritdoc />
    public void ConfigureServices(IServiceCollection services, IConfiguration config, IHostEnvironment env)
    {
        services.AddOptions<FilePostConfiguration>()
            .Bind(config.GetSection("EmergencyBackup"))
            .ValidateOnStart();
        services.AddTransient<IEmergencyBackupService, FilePostEmergencyBackup>();
        services.AddHttpClient(FilePostConfiguration.HttpClientName, ConfigureHttpClient);
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
