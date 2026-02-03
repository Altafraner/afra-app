using Quartz;

namespace Altafraner.AfraApp.User.Services.LDAP;

/// <summary>
/// A background service that schedules the LDAP synchronization job
/// </summary>
public class LdapAutoSyncScheduler : BackgroundService
{
    private const string JobIdentity = "sync";
    private const string TriggerIdentity = "sync-trigger";
    private const string GroupIdentity = "LDAP";
    private readonly ILogger _logger;
    private readonly IServiceProvider _serviceProvider;

    /// <inheritdoc />
    public LdapAutoSyncScheduler(
        IServiceProvider serviceProvider,
        ILogger<LdapAutoSyncScheduler> logger
    )
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var schedulerFactory = scope.ServiceProvider.GetRequiredService<ISchedulerFactory>();
        var scheduler = await schedulerFactory.GetScheduler(stoppingToken);
        var ldapService = scope.ServiceProvider.GetRequiredService<LdapService>();

        var key = new JobKey(JobIdentity, GroupIdentity);

        if (await scheduler.CheckExists(key, stoppingToken))
        {
            _logger.LogInformation("LDAP Sync Job already exists. Stopping.");
            await scheduler.DeleteJob(key, stoppingToken);
        }

        if (!ldapService.IsEnabled)
        {
            _logger.LogInformation("LDAP Sync Job not enabled. Not scheduling.");
            return;
        }

        var job = JobBuilder.Create<LdapAutoSyncJob>().WithIdentity(key).Build();
        var trigger = TriggerBuilder
            .Create()
            .WithIdentity(TriggerIdentity, GroupIdentity)
            .ForJob(job)
            .StartNow()
            .WithSimpleSchedule(x => x.WithInterval(TimeSpan.FromHours(1)).RepeatForever())
            .WithPriority(0)
            .Build();

        await scheduler.ScheduleJob(job, trigger, stoppingToken);
        _logger.LogInformation("LDAP Sync Job scheduled.");
    }
}
