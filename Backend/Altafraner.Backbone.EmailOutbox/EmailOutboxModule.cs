using Altafraner.Backbone.Abstractions;
using Altafraner.Backbone.EmailOutbox.Configuration;
using Altafraner.Backbone.EmailOutbox.Services;
using Altafraner.Backbone.Scheduling;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Altafraner.Backbone.EmailOutbox;

/// <summary>
///     Adds services for sending e-mail
/// </summary>
[DependsOn<SchedulingModule>]
public class EmailOutboxModule : IModule
{
    /// <inheritdoc />
    public void ConfigureServices(IServiceCollection services, IConfiguration config, IHostEnvironment env)
    {
        services.AddOptions<EmailConfiguration>()
            .Bind(config.GetSection("SMTP"))
            .Validate(EmailConfiguration.Validate)
            .ValidateOnStart();
        services.AddTransient<IEmailService, SmtpEmailService>();
        services.AddScoped<IEmailOutbox, Services.EmailOutbox>();
    }
}
