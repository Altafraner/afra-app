using Altafraner.AfraApp.Dashboard.API;
using Altafraner.AfraApp.Dashboard.Services;
using Altafraner.Backbone.Abstractions;

namespace Altafraner.AfraApp.Dashboard;

internal class DashboardModule : IModule
{
    public void ConfigureServices(IServiceCollection services, IConfiguration config, IHostEnvironment env)
    {
        services.AddScoped<DashboardService>();
    }

    public void Configure(WebApplication app)
    {
        app.MapDashboardEndpoints();
    }
}
