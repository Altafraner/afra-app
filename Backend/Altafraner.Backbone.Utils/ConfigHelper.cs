using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Altafraner.Backbone.Utils;

/// <summary>
///     Contains helper methods for handling configuration in modules
/// </summary>
public static class ConfigHelper
{
    /// <summary>
    ///     Gets and registers config
    /// </summary>
    public static T GetAndRegisterConfig<T>(IServiceCollection services, IConfiguration config, string section)
        where T : class, new()
    {
        var configSection = config.GetSection(section);
        services.AddOptions<T>().Bind(configSection);

        var configObject = configSection.Exists()
            ? configSection.Get<T>() ??
              throw new ValidationException("Cannot bind CookieAuthenticationSettings")
            : new T();

        return configObject;
    }
}
