using Altafraner.Backbone.Abstractions;
using Altafraner.Backbone.EmailOutbox;
using Altafraner.Backbone.EmailSchedulingModule.Services;
using Altafraner.Backbone.Scheduling;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Altafraner.Backbone.EmailSchedulingModule;

/// <summary>
///     A module for scheduling E-Mail deliveries
/// </summary>
/// <typeparam name="TPerson">The data type representing a user in the current context</typeparam>
[DependsOn<EmailOutboxModule>]
[DependsOn<SchedulingModule>]
public class EmailSchedulingModule<TPerson> : IModule<EmailSchedulingSettings<TPerson>>
    where TPerson : class, IEmailRecipient
{
    /// <inheritdoc />
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
}
