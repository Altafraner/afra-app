using Altafraner.Backbone.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Quartz;
using Quartz.AspNetCore;

namespace Altafraner.Backbone.Scheduling;

public class SchedulingModule : IModule
{
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
        services.AddQuartzServer(options => { options.WaitForJobsToComplete = true; });
    }
}
