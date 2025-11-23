using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Altafraner.Backbone.Abstractions;

/// <summary>
///     A module in the Altafraner framework
/// </summary>
public interface IModule
{
    /// <summary>
    ///     Adds the services to the service collection
    /// </summary>
    void ConfigureServices(IServiceCollection services, IConfiguration config, IHostEnvironment env);

    /// <summary>
    ///     Configures the web application
    /// </summary>
    void Configure(WebApplication app)
    {
    }

    /// <summary>
    ///     Registers the middleware provided by this module. Is run before <see cref="Configure" />
    /// </summary>
    void RegisterMiddleware(WebApplication app)
    {
    }

    /// <summary>
    ///     Performs initializing steps
    /// </summary>
    Task InitializeAsync(WebApplication app) => Task.CompletedTask;
}

// ReSharper disable once UnusedTypeParameter
/// <inheritdoc />
/// <typeparam name="TConfig">The type used for configuring this module</typeparam>
public interface IModule<TConfig> : IModule where TConfig : class;
