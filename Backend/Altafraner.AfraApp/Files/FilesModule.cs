using Altafraner.AfraApp.Files.Services;
using Altafraner.Backbone.Abstractions;

namespace Altafraner.AfraApp.Files;

/// <summary>
///     A module for file handling
/// </summary>
public class FilesModule : IModule
{
    /// <inheritdoc />
    public void ConfigureServices(IServiceCollection services, IConfiguration config, IHostEnvironment env)
    {
        services.AddOptions<FilesConfiguration>()
            .Bind(config.GetSection("Files"))
            .Validate(FilesConfiguration.Validate)
            .ValidateOnStart();

        services.AddScoped<ImageService>();
        services.AddScoped<FileService>();
    }
}
