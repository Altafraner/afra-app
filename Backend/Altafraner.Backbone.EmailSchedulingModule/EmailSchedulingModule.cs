using Altafraner.Backbone.Abstractions;
using Altafraner.Backbone.EmailOutbox;
using Altafraner.Backbone.EmailSchedulingModule.Contracts;
using Altafraner.Backbone.EmailSchedulingModule.Services;
using Altafraner.Backbone.Scheduling;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Altafraner.Backbone.EmailSchedulingModule;

[DependsOn<EmailOutboxModule>]
[DependsOn<SchedulingModule>]
public class EmailSchedulingModule<TPerson> : IModule<EmailSchedulingSettings<TPerson>>
    where TPerson : class, IEmailRecipient
{
    public void ConfigureServices(IServiceCollection services, IConfiguration config, IHostEnvironment env)
    {
        services.AddScoped<IScheduledEmailContext<TPerson>>(sp =>
        {
            var settings = sp.GetRequiredService<IOptions<EmailSchedulingSettings<TPerson>>>();
            var contextType = settings.Value.DbContextType ??
                              throw new InvalidOperationException("Cannot find EmailSchedulingSettings");

            return sp.GetRequiredService(contextType) as IScheduledEmailContext<TPerson> ??
                   throw new InvalidOperationException("Module not configured");
        });

        services.AddScoped<INotificationService, EmailNotificationService<TPerson>>();
    }

    public void Configure(WebApplication app)
    {
    }

    public Task InitializeAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
