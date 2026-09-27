using Altafraner.Backbone.EmailOutbox.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;

namespace Altafraner.Backbone.EmailOutbox.Services;

/// <summary>
///     Notifies admins when a job fails
/// </summary>
internal class FailureNotificationListener : ITriggerListener
{
    private readonly ILogger<FailureNotificationListener> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly EmailConfiguration _emailConfiguration;

    ///
    public FailureNotificationListener(ILogger<FailureNotificationListener> logger,
        IOptions<EmailConfiguration> emailConfiguration,
        IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
        _emailConfiguration = emailConfiguration.Value;
    }

    public async ValueTask TriggerRetriesExhausted(ITrigger trigger,
        IJobExecutionContext context,
        JobExecutionException exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(
            exception,
            "{Job} gave up after {Attempts} retries; the occurrence scheduled for {Scheduled} never succeeded",
            context.JobDetail.Key,
            context.RetryAttempt,
            context.ScheduledFireTimeUtc);

        using var scope = _scopeFactory.CreateScope();
        var outbox = scope.ServiceProvider.GetRequiredService<IEmailOutbox>();

        foreach (var administrator in _emailConfiguration.SystemAdministrators)
            await outbox.SendReportAsync(administrator,
                "Job Fehlgeschlagen",
                $"""
                 Ein wiederholter Hintergrundjob ist fehlgeschlagen.
                 Name: {context.JobDetail.Key}
                 Versuche: {context.RetryAttempt}
                 Geplant: {context.ScheduledFireTimeUtc}
                 """);
    }
}
