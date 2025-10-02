using Quartz;
using Quartz.AspNetCore;

namespace Altafraner.AfraApp.Backbone.Scheduler.Extensions;

internal static class AppBuilderExtension
{
    internal static void AddScheduler(this WebApplicationBuilder builder)
    {
        builder.Services.AddQuartz(q =>
            {
                q.UsePersistentStore(storeOptions =>
                    {
                        var conString = builder.Configuration.GetConnectionString("DefaultConnection")!;
                        storeOptions.UsePostgres(pgOptions =>
                            pgOptions.ConnectionString = conString
                        );
                        storeOptions.UseSystemTextJsonSerializer();
                    }
                );
            }
        );
        builder.Services.AddQuartzServer(options => { options.WaitForJobsToComplete = true; });
    }
}
