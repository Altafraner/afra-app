using Altafraner.Backbone.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Quartz;

namespace Altafraner.Backbone.Scheduling;

/// <summary>
///     A Module for job scheduling
/// </summary>
public class SchedulingModule : IModule
{
    /// <inheritdoc />
    public void ConfigureServices(IServiceCollection services, IConfiguration config, IHostEnvironment env)
    {
        services.AddQuartz(q =>
            {
                q.UsePersistentStore(storeOptions =>
                    {
                        var conString = config.GetConnectionString("DefaultConnection")!;
                        storeOptions.UsePostgres(pgOptions =>
                            pgOptions.ConnectionString = conString
                        );
                        storeOptions.UseSystemTextJsonSerializer();
                    }
                );
            }
        );
        services.AddQuartzHostedService(options => { options.WaitForJobsToComplete = true; });
    }
}
