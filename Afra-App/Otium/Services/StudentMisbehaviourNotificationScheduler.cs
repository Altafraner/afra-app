using Afra_App.Otium.Configuration;
using Afra_App.Otium.Jobs;
using Microsoft.Extensions.Options;
using Quartz;

namespace Afra_App.Otium.Services;

/// <summary>
///     A service for scheduling notifications for student misbehaviour.
/// </summary>
public class StudentMisbehaviourNotificationScheduler : BackgroundService
{
    private const string GroupName = "otium-student-misbehaviour-notification";
    private const string JobName = "student-misbehaviour-notification-job";
    private readonly IOptions<OtiumConfiguration> _otiumConfiguration;
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    ///     Called by DI.
    /// </summary>
    public StudentMisbehaviourNotificationScheduler(IServiceProvider serviceProvider,
        IOptions<OtiumConfiguration> otiumConfiguration)
    {
        _serviceProvider = serviceProvider;
        _otiumConfiguration = otiumConfiguration;
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var defaultNotificationTime = _otiumConfiguration.Value.StudentMisbehaviourNotification.Time;

        using var scope = _serviceProvider.CreateScope();
        var schedulerFactory = scope.ServiceProvider.GetRequiredService<ISchedulerFactory>();
        var scheduler = await schedulerFactory.GetScheduler(stoppingToken);
        var key = new JobKey(JobName, GroupName);

        var triggerNow = TriggerBuilder.Create()
            .ForJob(key)
            .StartNow()
            .Build();

        var triggerCron = TriggerBuilder.Create()
            .ForJob(key)
            .WithSchedule(CronScheduleBuilder
                .DailyAtHourAndMinute(defaultNotificationTime.Hour, defaultNotificationTime.Minute)
                .WithMisfireHandlingInstructionFireAndProceed())
            .Build();

        var exists = await scheduler.CheckExists(key, stoppingToken);
        if (exists)
        {
            var triggers = await scheduler.GetTriggersOfJob(key, stoppingToken);
            await scheduler.UnscheduleJobs(triggers.Select(t => t.Key).ToList(), stoppingToken);

            await scheduler.ScheduleJob(triggerCron, stoppingToken);
            await scheduler.ScheduleJob(triggerNow, stoppingToken);
            return;
        }

        var job = JobBuilder.Create<StudentMisbehaviourNotificationJob>()
            .PersistJobDataAfterExecution()
            .DisallowConcurrentExecution()
            .StoreDurably()
            .WithIdentity(key)
            .Build();

        await scheduler.AddJob(job, false, stoppingToken);
        await scheduler.ScheduleJob(triggerCron, stoppingToken);
        await scheduler.ScheduleJob(triggerNow, stoppingToken);
    }
}