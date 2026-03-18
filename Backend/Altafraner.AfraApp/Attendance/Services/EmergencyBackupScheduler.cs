using Altafraner.AfraApp.Attendance.Jobs;
using Quartz;

namespace Altafraner.AfraApp.Attendance.Services;

/// <summary>
/// Service that schedules emergency backups for Otium enrollments.
/// </summary>
public class EmergencyBackupScheduler : BackgroundService
{
    private const string GroupName = "attendance-emergency-backup";
    private const string JobName = "attendance-emergency-backup";
    private readonly ILogger<EmergencyBackupScheduler> _logger;
    private readonly IConfiguration _config;

    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Called from DI
    /// </summary>
    public EmergencyBackupScheduler(ILogger<EmergencyBackupScheduler> logger,
        IServiceProvider serviceProvider,
        IConfiguration config)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _config = config;
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
        }

        if (!_config.GetValue<bool>("Attendance:Backup"))
        {
            _logger.LogInformation("Attendance backup job disabled");
            return;
        }

        var job = JobBuilder.Create<EmergencyUploadJob>()
            .WithIdentity(key)
            .Build();

        await scheduler.ScheduleJob(job, trigger, stoppingToken);
    }
}
