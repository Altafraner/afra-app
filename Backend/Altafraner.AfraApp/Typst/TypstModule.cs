using Altafraner.Backbone.Abstractions;
using Altafraner.Typst;

namespace Altafraner.AfraApp.Typst;

/// <summary>
///     A Module for handling the Typst document generation
/// </summary>
public class TypstModule : IModule
{
    /// <inheritdoc />
    public void ConfigureServices(IServiceCollection services, IConfiguration config, IHostEnvironment env)
    {
        services.AddScoped<Altafraner.Typst.Typst>();
        services.AddOptions<TypstConfiguration>().Bind(config.GetSection("Typst"));
    }
}
