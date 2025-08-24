using Afra_App.Otium.Configuration;
using Afra_App.Otium.Jobs;
using Microsoft.Extensions.Options;
using Quartz;

namespace Afra_App.Otium.Services;

/// <summary>
/// Service that schedules emergency backups for Otium enrollments.
/// </summary>
public class EmergencyBackupScheduler : BackgroundService
{
    private const string GroupName = "otium-emergency-backup";
    private const string JobName = "otium-emergency-backup";
    private readonly ILogger<EnrollmentReminderScheduler> _logger;
    private readonly IOptions<OtiumConfiguration> _otiumConfiguration;

    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Called from DI
    /// </summary>
    public EmergencyBackupScheduler(ILogger<EnrollmentReminderScheduler> logger,
        IOptions<OtiumConfiguration> otiumConfiguration, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _otiumConfiguration = otiumConfiguration;
        _serviceProvider = serviceProvider;
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var schedulerFactory = scope.ServiceProvider.GetRequiredService<ISchedulerFactory>();
        var scheduler = await schedulerFactory.GetScheduler(stoppingToken);

        var key = new JobKey(JobName, GroupName);
        var exists = await scheduler.CheckExists(key, stoppingToken);
        var trigger = TriggerBuilder.Create()
            .ForJob(key)
            .WithSchedule(CronScheduleBuilder.CronSchedule("0 * * * * ? *"))
            .StartNow()
            .Build();

        if (exists)
        {
            await scheduler.DeleteJob(key, stoppingToken);
            _logger.LogInformation("Emergency backup disabled. Deleted existing job.");
        }

        if (!_otiumConfiguration.Value.EnableEmergencyBackup) return;

        var job = JobBuilder.Create<EmergencyUploadJob>()
            .WithIdentity(key)
            .Build();

        await scheduler.ScheduleJob(job, trigger, stoppingToken);
    }
}
