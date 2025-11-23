using Altafraner.Backbone.Abstractions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Altafraner.Backbone.Defaults;

/// <summary>
///     A module adding some development convenience features such as swagger and open api
/// </summary>
public class DevelopmentModule : IModule
{
    /// <inheritdoc />
    public void ConfigureServices(IServiceCollection services, IConfiguration config, IHostEnvironment env)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
    }

    /// <inheritdoc />
    public void Configure(WebApplication app)
    {
        app.MapOpenApi();

        if (!app.Environment.IsDevelopment()) return;
        app.UseSwagger();
        app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", app.Environment.ApplicationName); });
    }
}
