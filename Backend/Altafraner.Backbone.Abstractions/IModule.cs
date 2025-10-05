using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Altafraner.Backbone.Abstractions;

public interface IModule
{
    void ConfigureServices(IServiceCollection services, IConfiguration config, IHostEnvironment env);

    void Configure(WebApplication app)
    {
    }

    void BeforeConfigure(WebApplication app)
    {
    }

    Task InitializeAsync(WebApplication app) => Task.CompletedTask;
}

public interface IModule<TConfig> : IModule where TConfig : class;
