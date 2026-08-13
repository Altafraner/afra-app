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
        where T : class, IValidatable<T>, new()
    {
        var configSection = config.GetSection(section);
        services.AddOptions<T>()
            .Bind(configSection)
            .Validate(T.Validate)
            .ValidateOnStart();

        var configObject = configSection.Exists()
            ? configSection.Get<T>() ??
              throw new ValidationException("Cannot bind CookieAuthenticationSettings")
            : new T();

        return configObject;
    }
}

/// <summary>
///     An interface for validatable config
/// </summary>
public interface IValidatable<in T> where T : IValidatable<T>
{
    /// <summary>
    ///     Validates the object
    /// </summary>
    static virtual bool Validate(T entity)
    {
        return true;
    }
}
